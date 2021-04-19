using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace MainService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataProcessController : ControllerBase
    {
        private static readonly ActivitySource ActivitySource = new ActivitySource(nameof(DataProcessController));
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;


        private readonly ILogger<DataProcessController> _logger;

        public DataProcessController(ILogger<DataProcessController> logger)
        {
            _logger = logger;
        }


        [HttpGet("getdata/{data}")]
        public async Task<string> GetData(string data)
        {
            
            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? "localhost",
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS") ?? "TestMeHacker2021",
                Port = 5672,
                RequestedConnectionTimeout = TimeSpan.FromMilliseconds(3000),
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            var activityName = "Worker_1_Queue_In send";

            using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Producer);

            var properties = channel.CreateBasicProperties();


            ActivityContext contextToInject = default;
            if (activity != null)
            {
                contextToInject = activity.Context;
            }
            else if (Activity.Current != null)
            {
                contextToInject = Activity.Current.Context;
            }

            Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), properties, InjectTraceContextIntoBasicProperties);

            Helper.AddMessagingTags(activity, "Worker_1_Queue_In");

            properties.Persistent = true;

            var body = Encoding.UTF8.GetBytes(data);

            channel.BasicPublish("", routingKey: "Worker_1_Queue_In", basicProperties: properties, body: body);


            channel.QueueDeclare(queue: "Worker_1_Queue_Out", false, false, false, null);

            var consumer = new EventingBasicConsumer(channel);

            string outputMessage = string.Empty;
            //consumer.Received += Consumer_Received;
            var tasks = new List<Task<string>>();
            consumer.Received += (model, ea) =>
           {
               var body = ea.Body.ToArray();
               outputMessage = Encoding.UTF8.GetString(body);

               var parentContext = Propagator.Extract(default, ea.BasicProperties, this.ExtractTraceContextFromBasicProperties);
               Baggage.Current = parentContext.Baggage;
               var activityName = $"Worker_1_Queue_Out receive";
               using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);

               activity?.SetTag("message", outputMessage);
               Helper.AddMessagingTags(activity, "Worker_1_Queue_In");


           };

            Thread.Sleep(3000);
            channel.BasicConsume(queue: "Worker_1_Queue_Out", autoAck: true, consumer: consumer);

            return data;
        }

        private IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
        {
            try
            {
                if (props.Headers.TryGetValue(key, out var value))
                {
                    var bytes = value as byte[];
                    return new[] { Encoding.UTF8.GetString(bytes) };
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Failed to extract trace context: {ex}");
            }

            return Enumerable.Empty<string>();
        }

        private void InjectTraceContextIntoBasicProperties(IBasicProperties props, string key, string value)
        {
            try
            {
                if (props.Headers == null)
                {
                    props.Headers = new Dictionary<string, object>();
                }

                props.Headers[key] = value;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Failed to inject trace context.");
            }
        }
       
    }
    
}
