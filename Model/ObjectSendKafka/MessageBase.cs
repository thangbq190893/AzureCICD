namespace Webhook.Model.ObjectSendKafka
{
    public abstract class MessageBase
    {
        public string channel { get; set; }// from Facebook or Zalo
        public long time { get; set; }//time send message
        public string senderId { get; set; } //page-scope id of user for page
        public string senderName { get; set; } //facebook's username
        public string senderProfilePicture { get; set; }//facebook's avatar url
        public string senderGender { get; set; }// user's gender
        public string recipientId { get; set; }// id of page
        public string appId { get; set; }//id of application
        public MessageBase(string channel, long time, string senderId, string senderName, string senderProfilePicture, string senderGender, string recipientId, string appId)
        {
            this.channel = channel;
            this.time = time;
            this.senderId = senderId;
            this.senderName = senderName;
            this.senderProfilePicture = senderProfilePicture;
            this.senderGender = senderGender;
            this.recipientId = recipientId;
            this.appId = appId;
        }
    }
}
