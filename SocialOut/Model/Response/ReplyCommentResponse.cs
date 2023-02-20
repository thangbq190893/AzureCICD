using Newtonsoft.Json;

namespace SocialOut.Model.Response
{
    public class ReplyCommentResponse
    {
        [JsonProperty("id")]
        public string commentId { get; set; }
    }
}
