using Confluent.Kafka;
using Newtonsoft.Json;
using Serilog;

namespace Webhook.Engine
{
    public class FacebookProducer
    {
        private string topicName;
        private string topicFeed;
        private string kafkaServer;

        private ProducerConfig config;
        public FacebookProducer(IConfiguration configuration)
        {
            topicName = configuration.GetSection("kafka-topic:facebook-message").Value;
            topicFeed = configuration.GetSection("kafka-topic:facebook-feed").Value;
            kafkaServer = configuration.GetSection("kafka:bootstrap-servers").Value;
            config = new ProducerConfig
            {
                BootstrapServers = kafkaServer,
                MessageMaxBytes = 25 * 1024 * 1024,
            };
        }

        public async Task SendMessage(string msg)
        {
            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                var result = await producer.ProduceAsync(topicName, new Message<Null, string> { Value = msg });
            }
        }

        public async Task SendFeed(string feed)
        {
            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                var result = await producer.ProduceAsync(topicFeed, new Message<Null, string> { Value = feed });
            }
        }
    }
}
