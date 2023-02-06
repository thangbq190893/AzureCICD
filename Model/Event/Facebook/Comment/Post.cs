namespace Webhook.Model.Event.Facebook.Facebook.Comment
{
    public class Post
    {
        public string status_type { get;set;}
        public bool is_published { get;set;}
        public string updated_time { get; set; }
        public string permalink_url { get; set; }
        public string promotion_status { get; set; }
        public string id { get; set; }
    }
}
