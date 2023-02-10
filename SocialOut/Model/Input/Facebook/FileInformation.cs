namespace SocialOut.Model.Input.Facebook
{
    public class FileInformation
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile File { get; set; }
    }
}
