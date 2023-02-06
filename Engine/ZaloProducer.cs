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
            topicName = configuration.GetConnectionString("kafka-topic:zalo-message");
            kafkaServer = configuration.GetConnectionString("kafka:bootstrap-servers");
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
