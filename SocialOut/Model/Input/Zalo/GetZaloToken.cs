namespace SocialOut.Model.Input.Zalo
{
    public class GetZaloToken
    {
        public string app_id { get; set; }
        public string refresh_token { get; set; }
        public string secret_key { get; set; }
    }
}
