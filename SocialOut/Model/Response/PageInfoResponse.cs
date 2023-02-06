namespace SocialOut.Model.Response
{
    public class PageInfoResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<SocialInformation> data { get; set; }
    }
}
