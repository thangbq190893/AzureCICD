namespace SocialOut.Model.Input.Facebook
{
    public class FileData
    {
        public IFormFile filedata { get; set; }
        public AdditionData data { get; set; }
    }

    public class AdditionData
    {
        public string recipient { get; set; }
        public string senderId { get; set; }
        public string message { get; set; }
        public string type { get; set; }
    }
}
