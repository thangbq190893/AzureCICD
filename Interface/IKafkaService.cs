namespace Webhook.Interface
{
    public interface IKafkaService
    {
        Task SendFacebookMessage(string message);
        Task SendFacebookFeed(string feed);
        Task SendZaloMessage(string message);
    }
}
