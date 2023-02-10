using Newtonsoft.Json;

namespace Webhook.Model.Event.Zalo
{
    public class Data
    {
        public string app_id { get; set; }
        public string user_id_by_app { get; set; }
        public string event_name { get; set; }
        public string timestamp { get; set; }
        public Sender sender { get; set; }
        public Recipient recipient { get; set; }
        public Message message { get; set; }
    }
}
