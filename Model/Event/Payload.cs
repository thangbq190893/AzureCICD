namespace Webhook.Model.Event
{
    public class Payload
    {
        public int? size { get; set; }
        public string? name { get; set; }
        public string? type { get; set; }
        public string? url { get; set; }
        public string? fileBase64 { get; set; }
        public long? sticker_id { get; set; }
    }
}
