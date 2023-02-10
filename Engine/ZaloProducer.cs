using Confluent.Kafka;

namespace Webhook.Engine
{
    public class ZaloProducer
    {
        private string topicName;
        private string kafkaServer;

        private readonly ProducerConfig config;
        public ZaloProducer(IConfiguration configuration)
        {
            topicName = configuration.GetSection("kafka-topic:zalo-message").Value;
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
    }
}
