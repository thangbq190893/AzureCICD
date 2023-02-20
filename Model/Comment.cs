using Webhook.Model.Event;
using Webhook.Model.Event.Facebook.Message;

namespace Webhook.Model
{
    public class Comment : FeedBase
    {
        public string CommentId { get;set; }
        public string PostId { get; set; }
        public string Message { get; set; }
        public string ParentId { get; set; }
        public string ProfilePicture { get; set; }
        public Attachment Attachment { get; set; }
        public Comment(string commentId, string senderId, string senderName, string postId
            , string message, long time, string parentId, string item, string profilePicture
            , Attachment attachment) : base(senderId,senderName, time, item)
        {
            this.CommentId = commentId;
            this.PostId = postId;
            this.Message = message;
            this.ParentId = parentId;
            this.ProfilePicture = profilePicture;
            this.Attachment = attachment;
        }
    }
}
