using Newtonsoft.Json;

namespace SocialOut.Model.Input.Facebook.GraphApi
{
    public class DetailCommentResponse
    {
        [JsonProperty("commentId")]
        public string id { get; set; }
        [JsonProperty("from")]
        public CommentFrom from { get; set; }
        [JsonProperty("creationTime")]
        public string created_time;
        [JsonProperty("message")]
        public string message;
    }
}
