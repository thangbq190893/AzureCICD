using Newtonsoft.Json;

namespace Webhook.Model.Event.Zalo
{
    public class Message
    {
        public string msg_id { get; set; }
        public string? text { get; set; }
        public Attachment[]? attachments { get; set; }
    }
}
