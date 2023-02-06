namespace Webhook.Model.Event.Facebook.Message
{
    public class Message
    {
        public string? mid { get; set; }
        public string? text { get; set; }
        public Attachment[]? attachments { get; set; }
    }
}
