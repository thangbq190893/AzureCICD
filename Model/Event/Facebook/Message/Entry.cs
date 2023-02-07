using Webhook.Model.Event.Facebook.Comment;

namespace Webhook.Model.Event.Facebook.Message
{
    public class Entry
    {
        public string id { get; set; }
        public long time { get; set; }
        public Messaging[]? messaging { get; set; }
        public Change[]? changes { get; set; }
    }
}
