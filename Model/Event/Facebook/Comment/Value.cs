namespace Webhook.Model.Event.Facebook.Comment
{
    public class Value
    {
        public From? from { get; set; }
        public Post? post { get; set; }
        public string? message { get; set; }
        public string? post_id { get; set; }
        public string? comment_id { get; set; }
        public long created_time { get; set; }
        public string? item { get; set; }
        public string? parent_id { get; set; }
        public string? verb { get; set; }
        public string? photo { get; set; }
        public string? video { get; set; }
        public string? link { get; set; }
        public string? photo_id { get; set; }
    }
}
