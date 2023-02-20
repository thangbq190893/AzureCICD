namespace SocialOut.Model.Response
{
    public class GetSocialInformation
    {
        public string name { get; set; }
        public int followers_count { get; set; }
        public Picture picture { get; set; }
        public string id { get; set; }
    }
}
