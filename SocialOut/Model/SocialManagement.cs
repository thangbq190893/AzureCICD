namespace SocialOut.Model
{
    public class SocialManagement
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public string PageName { get; set; }
        public string PageId { get; set; }
        public int Channel { get; set; }

        public SocialManagement(Guid id, string token, string pageName, string pageId, int channel)
        {
            Id = id;
            Token = token;
            PageName = pageName;
            PageId = pageId;
            Channel = channel;
        }
    }
}
