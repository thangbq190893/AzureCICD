using Newtonsoft.Json;
using SocialOut.Interface;
using SocialOut.Model.Input;
using SocialOut.Model.Input.Facebook;
using SocialOut.Model.Response;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SocialOut.Service
{
    public class FacebookService : IFacebookService
    {
        protected bool TrustedHostWithoutCertificateValidated = true;
        private readonly ICacheService _cacheService;
        private string facebookApi;
        private string facebookVersion;
        public FacebookService(ICacheService cacheService, IConfiguration configuration)
        {
            _cacheService = cacheService;
            facebookApi = configuration.GetSection("social:domain-api:facebook").Value;
            facebookVersion = configuration.GetSection("social:version:facebook").Value;
        }
        public async Task<SendMessageResponseData> SendText(MessageData mes)
        {
            Dictionary<string, string> FacebookPages = await _cacheService.GetFbInfo();
            string url;

            string token = FacebookPages["" + mes.senderId];

            if (token != null)
            {
                url = string.Format(facebookApi + "/" + facebookVersion + "/" + mes.senderId + "/messages?access_token=" + token);

                string json = JsonConvert.SerializeObject(new SendMessageTextGraphApiInput
                {
                    messaging_type = "RESPONSE",
                    recipient = new Recipient { id = mes.receiveId },
                    message = new MessageText { text = mes.text }
                });

                var data = new StringContent(json, Encoding.UTF8, "application/json");
                HttpClientHandler handler;
                try
                {

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.SystemDefault;
                    handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = ServerCertificateCustomValidation;
                }
                catch (Exception ex)
                {
                    throw;
                }

                using (var client = new HttpClient(handler))
                {
                    var result = client.PostAsync(url, data).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        SendMessageResponseData res = JsonConvert.DeserializeObject<SendMessageResponseData>(result.Content.ReadAsStringAsync().Result);
                        return res;
                    }
                }
            }
            return null;
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

        protected bool ServerCertificateCustomValidation(HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return CertificateValidationCallBack(requestMessage, certificate, chain, sslPolicyErrors);
        }

        protected bool CertificateValidationCallBack(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (!TrustedHostWithoutCertificateValidated)
                {
                    if (chain != null && chain.ChainStatus != null)
                    {
                        foreach (X509ChainStatus status in chain.ChainStatus)
                        {
                            if ((certificate.Subject == certificate.Issuer) &&
                               (status.Status == X509ChainStatusFlags.UntrustedRoot))
                            {
                                // Self-signed certificates with an untrusted root are valid. 
                                continue;
                            }
                            else
                            {
                                if (status.Status != X509ChainStatusFlags.NoError)
                                {
                                    // If there are any other errors in the certificate chain, the certificate is invalid,
                                    // so the method returns false.
                                    return false;
                                }
                            }
                        }
                    }
                }

                // When processing reaches this line, the only errors in the certificate chain are 
                // untrusted root errors for self-signed certificates. These certificates are valid
                // for default Exchange server installations, so return true.
                return true;
            }
            else
            {
                // In all other cases, return false.
                return false;
            }

        }

    }
}