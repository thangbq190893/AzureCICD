using SocialOut.Interface;
using SocialOut.Model.Input;
using SocialOut.Model.Input.Zalo;
using System.Text;

namespace SocialOut.Service
{
    public class ZaloService : IZaloService
    {
        private readonly ICacheService _cacheService;
        private string zaloApi;
        private string zaloVersion;
        private string zaloOauthApi;
        private string zaloOauthVersion;
        public ZaloService(ICacheService cacheService, IConfiguration configuration)
        {
            _cacheService = cacheService;
            zaloApi = configuration.GetSection("social:domain-api:zalo").Value;
            zaloVersion = configuration.GetSection("social:version:zalo").Value;
            zaloOauthApi = configuration.GetSection("social:domain-api:zalo-oauth").Value;
            zaloOauthVersion = configuration.GetSection("social:version:zalo-oauth").Value;
        }

        public async Task SendText(MessageData mes)
        {
            Dictionary<string, string> FacebookPages = await _cacheService.GetZaloInfo();
            string url;

            string accessToken = FacebookPages["" + mes.senderId];

            if (accessToken != null)
            {
                url = string.Format(zaloApi + "/" + zaloVersion + "/oa/message?access_token={0}", accessToken);

                string json = string.Format("{\"recipient\":{\"user_id\":\"%s\"},\"message\":{\"text\":\"%s\"}}", mes.receiveId, mes.text);

                var data = new StringContent(json, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var result = client.PostAsync(url, data).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var model = result.Content.ReadAsStringAsync().Result;
                    }
                }
            }
        }

        public async Task RefreshTokenZaloOa(GetZaloToken input)
        {
            string url = string.Format(zaloOauthApi + "/" + zaloOauthVersion + "/oa/access_token");
        }
    }
}
