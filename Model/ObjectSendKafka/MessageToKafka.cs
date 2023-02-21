namespace Webhook.Model.ObjectSendKafka
{
    public class MessageToKafka
    {
        public int code { get; set; }
        public string message { get; set; }
        public Object data { get; set; }
    }
}
