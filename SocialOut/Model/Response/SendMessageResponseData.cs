using System.Text.Json.Serialization;

namespace SocialOut.Model.Response
{
    public class SendMessageResponseData
    {
        [JsonPropertyName("userId")]
        public string recipient_id { get; set; }
        [JsonPropertyName("messageId")]
        public string message_id { get; set; }
    }
}
