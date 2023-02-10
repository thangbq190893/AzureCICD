using Newtonsoft.Json;
using SocialOut.Interface;
using SocialOut.Model.Input;
using SocialOut.Model.Input.Facebook;
using SocialOut.Model.Input.Facebook.GraphApi;
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

        public async Task<DetailCommentResponse> ReplyComment(ReplyComment input)
        {
            Dictionary<string, string> FacebookPages = await _cacheService.GetFbInfo();
            string token = FacebookPages["" + input.pageId];
            if (token != null)
            {
                using (var client = new HttpClient())
                {
                    string url = string.Format(facebookApi + "/" + facebookVersion + "/" + input.commentId + "/comments?access_token=" + token);
                    string json = JsonConvert.SerializeObject(new ReplyCommentText
                    {
                        message = input.text
                    });

                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                    var result = client.PostAsync(url, data).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var model = result.Content.ReadAsStringAsync().Result;

                        ReplyCommentResponse res = JsonConvert.DeserializeObject<ReplyCommentResponse>(model);

                        if(res.commentId != null)
                        {
                            string urlDetailComment = String.Format(facebookApi + "/" + facebookVersion + "/" + res.commentId + "?access_token=" + token);

                            var resultDetailComment = client.GetAsync(urlDetailComment).Result;

                            if (resultDetailComment.IsSuccessStatusCode)
                            {
                                var modelDetailComment = resultDetailComment.Content.ReadAsStringAsync().Result;

                                DetailCommentResponse resDetailComment = JsonConvert.DeserializeObject<DetailCommentResponse>(modelDetailComment);

                                return resDetailComment;
                            }
                        }
                    }
                }

                return null;
            }

            return null;
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

        public async Task SendAttachment(IFormFile filedata, string senderId, string recipient, string message, string type)
        {
            try
            {
                Dictionary<string, string> FacebookPages = await _cacheService.GetFbInfo();
                string token = FacebookPages["" + senderId];
                if (token != null)
                {
                    string uploadFileUrl = string.Format(facebookApi + "/" + facebookVersion + "/me/messages?access_token=" + token);

                    using (var client = new HttpClient())
                    {
                        SendAttachmentRequest requestObj = new SendAttachmentRequest
                        {
                            filePath = filedata,
                            recipient = recipient,
                            message = message,
                            type = type
                        };

                        MultipartFormDataContent content = new MultipartFormDataContent();
                        byte[] fileBytes = new byte[99999999];
                        if (filedata.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                filedata.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                        }
                        ByteArrayContent bytes = new ByteArrayContent(fileBytes);

                        //content.Add(bytes, "filedata", filedata.FileName);
                        content.Add(new StringContent(type), "type");
                        content.Add(new StringContent(recipient), "recipient");
                        content.Add(new StringContent(message), "message");

                        var result = client.PostAsync(uploadFileUrl, content).Result;
                        if (result.IsSuccessStatusCode)
                        {
                            var model = result.Content.ReadAsStringAsync().Result;
                            GetSocialInformation getSocialInformation = JsonConvert.DeserializeObject<GetSocialInformation>(model);
                        }
                    }

                    //Facebook.FacebookClient Client = new Facebook.FacebookClient(token);

                    //Dictionary<string, object> tmp = new Dictionary<string, object>();

                    //tmp.Add("recipient", JsonConvert.SerializeObject(new Recipient1
                    //{
                    //    id = recipient
                    //}));
                    //tmp.Add("message", message);
                    //tmp.Add("messaging_type", "RESPONSE");

                    //var med = new FacebookMediaObject();
                    //med.FileName = filedata.FileName;
                    //med.ContentType = "image/png";

                    //if (filedata.Length > 0)
                    //{
                    //    using (var ms = new MemoryStream())
                    //    {
                    //        filedata.CopyTo(ms);
                    //        var fileBytes = ms.ToArray();
                    //        med.SetValue(fileBytes);
                    //    }
                    //}

                    //tmp.Add(filedata.FileName, med);

                    //var a = Client.Post("me/messages", tmp);
                }
            }
            catch (Exception ex)
            {

            }
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