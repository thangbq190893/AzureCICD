namespace SocialOut.Model.Input
{
    public class SendAttachmentRequest
    {
        public IFormFile filePath { get; set; }
        public string recipient { get; set; }
        public string message { get; set; }
        public string type { get; set; }
    }
}
