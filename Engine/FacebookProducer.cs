using Confluent.Kafka;

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
                BootstrapServers = kafkaServer
            };
        }

        public async Task SendMessage(string msg)
        {
            try
            {
                using (var producer = new ProducerBuilder<Null, string>(config).Build())
                {
                    var result = await producer.ProduceAsync(topicName, new Message<Null, string> { Value = msg });
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task SendFeed(string feed)
        {
            try
            {
                using (var producer = new ProducerBuilder<Null, string>(config).Build())
                {
                    var result = await producer.ProduceAsync(topicFeed, new Message<Null, string> { Value = feed });
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
