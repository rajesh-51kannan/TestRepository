using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Utils
{
    public static class Helper
    {
        public const string DefaultExchangeName = "";

        public static void AddMessagingTags(Activity activity)
        {
            AddMessagingTags(activity, string.Empty);
        }

        public static void AddMessagingTags(Activity activity, string queueName)
        {
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.destination", DefaultExchangeName);
            activity?.SetTag("messaging.rabbitmq.routing_key", queueName);
        }

        public static void InjectTraceContextIntoBasicProperties(IBasicProperties props, string key, string value, ILogger logger)
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
                logger.LogError(ex, "Failed to inject trace context.");
            }
        }
    }
}
