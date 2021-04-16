using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace WorkerServiceHelper
{
    public class Worker : BackgroundService
    {
        private static readonly ActivitySource ActivitySource = new ActivitySource("Worker Service 1");
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();
        private readonly ILogger<Worker> _logger;

        ConnectionFactory _connectionFactory;
        IConnection _connection;
        IModel _channel;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            InitRabbitMQ();
            return base.StartAsync(cancellationToken);
        }

        private void InitRabbitMQ()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? "localhost",
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS") ?? "TestMeHacker2021",
                Port = 5672,
                RequestedConnectionTimeout = TimeSpan.FromMilliseconds(3000)
            };

            // create connection  
            _connection = _connectionFactory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "Worker_1_Queue_In", false, false, false, null);


            _channel.BasicQos(0, 1, false);


            //_connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            ListenToWorker1InQueue();
            ListenToWorker2OutQueue();

            await Task.CompletedTask;

        }


        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _connection.Close();
        }

        private void ListenToWorker1InQueue()
        {

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
           {
               var parentContext = Propagator.Extract(default, ea.BasicProperties, this.ExtractTraceContextFromBasicProperties);
               Baggage.Current = parentContext.Baggage;

               var activityName = $"Worker_1_Queue_In receive";
               using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);

               var body = ea.Body.ToArray();
               var message = Encoding.UTF8.GetString(body);
               activity?.SetTag("message", message);

               Helper.AddMessagingTags(activity, "Worker_1_Queue_In");

               int dots = message.Split('.').Length - 1;
                //Thread.Sleep(dots * 1000);
                Thread.Sleep(1000);

               WriteToWorker2InQueue(_connection, message);
           };

            _channel.BasicConsume(queue: "Worker_1_Queue_In", autoAck: true, consumer: consumer);

        }

        private void WriteToWorker2InQueue(IConnection connection, string message)
        {
            var activityName = "Worker_2_Queue_In send";

            using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Producer);
            //using var channel = connection.CreateModel();
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

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

            Helper.AddMessagingTags(activity, "Worker_2_Queue_In");

            var body = Encoding.UTF8.GetBytes(message);

            _channel.QueueDeclare(queue: "Worker_2_Queue_In", false, false, false, null);

            _channel.BasicPublish("", "Worker_2_Queue_In", basicProperties: properties, body: body);

            Console.WriteLine("Sent your message to worker 2 In: {0}", message);
        }

        private void ListenToWorker2OutQueue()
        {

            _channel.QueueDeclare(queue: "Worker_2_Queue_Out", false, false, false, null);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var parentContext = Propagator.Extract(default, ea.BasicProperties, this.ExtractTraceContextFromBasicProperties);
                Baggage.Current = parentContext.Baggage;

                var activityName = $"Worker_2_Queue_Out receive";
                using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                activity?.SetTag("message", message);

                Helper.AddMessagingTags(activity, "Worker_2_Queue_Out");


                int dots = message.Split('.').Length - 1;
                Thread.Sleep(1000);
                //Thread.Sleep(dots * 1000);


                WriteToWorker1OutQueue(_connection, message);
            };

            _channel.BasicConsume(queue: "Worker_2_Queue_Out", autoAck: true, consumer: consumer);
        }

        private void WriteToWorker1OutQueue(IConnection connection, string message)
        {
            var activityName = "Worker_1_Queue_Out send";

            using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Producer);
            //using var channel = connection.CreateModel();
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

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

            Helper.AddMessagingTags(activity, "Worker_1_Queue_Out");

            var body = Encoding.UTF8.GetBytes(message);

            _channel.QueueDeclare(queue: "Worker_1_Queue_Out", false, false, false, null);

            _channel.BasicPublish("", "Worker_1_Queue_Out", basicProperties: properties, body: body);
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
