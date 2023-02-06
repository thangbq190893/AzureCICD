using Newtonsoft.Json;
using SocialOut.Interface;
using SocialOut.Model.Input;
using SocialOut.Model.Input.Facebook;
using SocialOut.Model.Response;
using System.Text;

namespace SocialOut.Service
{
    public class FacebookService : IFacebookService
    {
        private readonly ICacheService _cacheService;
        private string facebookApi;
        private string facebookVersion;
        public FacebookService(ICacheService cacheService, IConfiguration configuration)
        {
            _cacheService = cacheService;
            facebookApi = configuration.GetSection("social:domain-api:facebook").Value;
            facebookVersion = configuration.GetSection("social:version:facebook").Value;
        }
        public async Task SendText(MessageData mes)
        {
            Dictionary<string, string> FacebookPages = await _cacheService.GetFbInfo();
            string url;

            string token = FacebookPages["" + mes.senderId];

            if (token != null)
            {
                url = string.Format(facebookApi + "/" + facebookVersion + "/%s/messages?access_token=%s", mes.senderId, token);

                string json = string.Format("{\"messaging_type\":\"RESPONSE\",\"recipient\":{\"id\":\"{0}\"},\"message\":{\"text\":\"{1}\"}}",
                        mes.receiveId, mes.text);

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

        public async Task ReplyComment(ReplyComment input)
        {
            Dictionary<string, string> FacebookPages = await _cacheService.GetFbInfo();
            string token = FacebookPages["" + input.pageId];
            if (token != null)
            {
                string url = string.Format(facebookApi + "/" + facebookVersion + "/%s/comments?access_token=%s", input.commentId, token);
                string json = string.Format("{\"message\" : \"%s\"}", input.text);

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

        public async Task<List<SocialInformation>> GetPageInfo(PageInfo input)
        {
            Dictionary<string, string> FacebookPages = await _cacheService.GetFbInfo();

            List<SocialInformation> res = new List<SocialInformation>();
            foreach (string pageId in input.PageId)
            {
                SocialInformation socialInfo = new SocialInformation();
                string token = FacebookPages["" + pageId];
                if (token != null)
                {
                    string url = string.Format(facebookApi + "/{0}?access_token={1}&fields=name,followers_count,picture", pageId, token);

                    using (var client = new HttpClient())
                    {
                        var result = client.GetAsync(url).Result;
                        if (result.IsSuccessStatusCode)
                        {
                            var model = result.Content.ReadAsStringAsync().Result;
                            GetSocialInformation getSocialInformation = JsonConvert.DeserializeObject<GetSocialInformation>(model);

                            socialInfo.id = getSocialInformation.id;
                            socialInfo.name = getSocialInformation.name;
                            socialInfo.picture = getSocialInformation.picture.data.url;
                            socialInfo.followers_count = getSocialInformation.followers_count;

                            res.Add(socialInfo);
                        }
                    }
                }
            }

            return res;
        }

        public async Task SendAttachment(MultipartFileData file, string senderId, string recipient, string message, string type)
        {
            Dictionary<string, string> FacebookPages = await _cacheService.GetFbInfo();
            string token = FacebookPages["" + senderId];
            string uploadFileUrl = string.Format(facebookApi + "/" + facebookVersion + "/me/messages?access_token=%s", token);
        }
    }
}