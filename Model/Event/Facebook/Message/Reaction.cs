namespace Webhook.Model.Event.Facebook.Message
{
    public class Reaction
    {
        public string mid { get; set; }
        public string action { get; set; }
        public string emoji { get; set; }
        public string reaction { get; set; }
    }
}
