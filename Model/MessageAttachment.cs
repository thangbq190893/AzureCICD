using Webhook.Model.Event.Facebook.Message;

namespace Webhook.Model
{
    public class MessageAttachment : MessageBase
    {
        public string messageId { get; set; }
        public string text { get; set; } // text content
        public Attachment attachment { get; set; }

        public MessageAttachment(string channel, long time, string senderId, string senderName, string senderProfilePicture, string senderGender, string recipientId,
                             string messageId, string text, Attachment attachment, string appId) : base(channel, time, senderId, senderName, senderProfilePicture, senderGender, recipientId,appId)
        {
            this.attachment = attachment;
            this.messageId = messageId;
            this.text = text;
        }
    }
}
