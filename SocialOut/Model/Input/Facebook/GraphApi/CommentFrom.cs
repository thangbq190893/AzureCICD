using Newtonsoft.Json;

namespace SocialOut.Model.Input.Facebook.GraphApi
{
    public class CommentFrom
    {
        [JsonProperty("senderId")]
        public string id { get; set; }
        [JsonProperty("senderName")]
        public string name { get; set; }
    }
}
