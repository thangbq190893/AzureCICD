namespace Webhook.Model.Event.Facebook.Message
{
    public class Messaging
    {
        public Sender sender { get; set; }
        public Recipient recipient { get; set; }
        public long timestamp { get; set; }
        public Message? message { get; set; }
        public Reaction? reaction { get; set; }
    }
}
