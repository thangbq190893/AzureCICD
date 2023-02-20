namespace SocialOut.Model.Input.Facebook
{
    public class SendMessageTextGraphApiInput
    {
        public string messaging_type { get; set; }
        public Recipient recipient { get; set; }
        public MessageText message { get; set; }
    }
}
