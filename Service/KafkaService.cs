using Webhook.Engine;
using Webhook.Interface;

namespace Webhook.Service
{
    public class KafkaService : IKafkaService
    {
        private readonly FacebookProducer _facebookProducer;
        private readonly ZaloProducer _zaloProducer;
        public KafkaService(FacebookProducer facebookProducer, ZaloProducer zaloProducer)
        {
            _facebookProducer = facebookProducer;
            _zaloProducer = zaloProducer;
        }

        public async Task SendFacebookMessage(string message)
        {
            await _facebookProducer.SendMessage(message);
        }

        public async Task SendFacebookFeed(string feed)
        {
            await _facebookProducer.SendFeed(feed);
        }

        public async Task SendZaloMessage(string message)
        {
            await _zaloProducer.SendMessage(message);
        }
    }
}
