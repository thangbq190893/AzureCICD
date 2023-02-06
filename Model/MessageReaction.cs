namespace Webhook.Model
{
    public class MessageReaction : MessageBase
    {
        public string messageId { get; set; }
        public string action { get; set; }// react or unreact
        public string emoji { get; set; } // icon
        public string reaction { get; set; }// icon's main
        public MessageReaction(String channel, long time, String senderId, String senderName, String senderProfilePicture, String senderGender, String recipientId,
                           String messageId, String action, String emoji, String reaction, String appId) : base(channel, time, senderId, senderName, senderProfilePicture, senderGender, recipientId,appId)
        {
            this.action = action;
            this.messageId = messageId;
            this.emoji = emoji;
            this.reaction = reaction;
        }
    }
}
