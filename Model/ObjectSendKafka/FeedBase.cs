namespace Webhook.Model.ObjectSendKafka
{
    public abstract class FeedBase
    {
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public long Time { get; set; }
        public string Item { get; set; }

        public FeedBase(string senderId, string senderName, long time, string item)
        {
            this.SenderId = senderId;
            this.SenderName = senderName;
            this.Time = time;
            this.Item = item;
        }
    }
}
