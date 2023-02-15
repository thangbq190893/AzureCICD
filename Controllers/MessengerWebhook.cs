using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System.Dynamic;
using System.Net;
using System.Text;
using System.Text.Json;
using Volo.Abp.BlobStoring;
using Webhook.Interface;
using Webhook.Model;
using Webhook.Model.Event;
using Webhook.Model.Event.Facebook.Comment;
using Webhook.Model.Event.Facebook.Message;
using Webhook.Service;

namespace Webhook.Controllers
{
    [Route("api/messenger-webhook")]
    [ApiController]
    public class MessengerWebhook : ControllerBase
    {
        private readonly IBlobContainer _blobContainer;
        private readonly IKafkaService _kafkaService;
        private readonly ICacheService _cacheService;
        private readonly ISocialManagementRepository _socialManagementRepository;
        private string facebookApi;
        private string facebookVersion;
        public MessengerWebhook(ICacheService cacheService
            , IKafkaService kafkaService
            , ISocialManagementRepository socialManagementRepository
            , IConfiguration configuration
            , IBlobContainer blobContainer)
        {
            _kafkaService = kafkaService;
            _cacheService = cacheService;
            _socialManagementRepository = socialManagementRepository;
            facebookApi = configuration.GetSection("social:domain-api:facebook").Value;
            facebookVersion = configuration.GetSection("social:version:facebook").Value;
            _blobContainer = blobContainer;
        }

        [HttpGet]
        public IActionResult VerifyWebhook([FromQuery(Name = "hub.mode")] string mode, [FromQuery(Name = "hub.verify_token")] string verify_token, [FromQuery(Name = "hub.challenge")] string challenge)
        {
            try
            {
                if (!verify_token.Equals("11111"))
                {
                    Log.Warning("Wrong verify_token : " + verify_token);
                    return StatusCode(422);
                }

                return Ok(challenge);
            }
            catch (Exception e)
            {
                Log.Error("VerifyWebhook Error: " + JsonConvert.SerializeObject(e));
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> WebhookAsync(Data input)
        {
            Log.Information("Event message received");
            string channel = "Facebook";
            // Lấy DS quản lý FB page
            Dictionary<string, string> facebookPages = await _socialManagementRepository.GetFbPages();

            if (input.entry[0].messaging != null)
            {
                Messaging messaging = input.entry[0].messaging[0];
                // id người gửi và người nhận
                string senderId = messaging.sender.id;
                string recipientId = messaging.recipient.id;
                // token của page
                string accessToken = facebookPages["" + recipientId];

                if (accessToken != null)
                {
                    SocialUserInformation sender = null;
                    try
                    {
                        if (sender == null)
                        {
                            string fields = "id,name,profile_pic";
                            string url = string.Format(facebookApi + "/" + facebookVersion + "/" + senderId + "?fields=" + fields + "&access_token=" + accessToken);
                            using (var client = new HttpClient())
                            {
                                var result = await client.GetAsync(url);
                                if (result.IsSuccessStatusCode)
                                {
                                    var model = result.Content.ReadAsStringAsync().Result;
                                    sender = JsonConvert.DeserializeObject<SocialUserInformation>(model);
                                }
                            }
                        }

                        if (sender == null)
                        {
                            return Ok(new MessageToKafka
                            {
                                code = 500,
                                message = "Fb get user info fail",
                                data = null
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Information("Fb get user info fail : " + JsonConvert.SerializeObject(ex));
                    }

                    Reaction? reaction = messaging.reaction;
                    if (reaction == null)
                    {
                        string text = messaging.message.text;
                        //message with text only
                        if (text != null)
                        {
                            MessageAttachment mes = new MessageAttachment(channel, messaging.timestamp,
                                senderId, sender.name != null ? sender.name : "", sender.profile_pic != null ? sender.profile_pic : "",
                                "null", recipientId, messaging.message.mid,
                                text, null, recipientId);

                            //Send to kafka server
                            try
                            {
                                await _kafkaService.SendFacebookMessage(System.Text.Json.JsonSerializer.Serialize(
                                                                new MessageToKafka
                                                                {
                                                                    code = 200,
                                                                    message = "successfully",
                                                                    data = mes
                                                                }));

                                Log.Information("Fb Message send to kafka server success");
                            }
                            catch (Exception e)
                            {
                                Log.Error("Fb Message send to kafka server fail : " + JsonConvert.SerializeObject(e));
                            }
                        }
                        //message with attachment only
                        else
                        {
                            int i = 0;
                            foreach (Attachment attachment in messaging.message.attachments)
                            {
                                Attachment attr = attachment;
                                Payload payload = attr.payload;

                                // Tạo đường dẫn trong ổ
                                if (attr.type.Equals("file") || attr.type.Equals("image") || attr.type.Equals("video"))
                                {
                                    attr.payload.name = payload.url.Split("/")[payload.url.Split("/").Length - 1].Split("?")[0];
                                }

                                if (payload.sticker_id != null)
                                {
                                    attr.type = "sticker";
                                }

                                //convert to base64
                                byte[] contentByte = GetImage(payload.url);
                                string encodeStr = Convert.ToBase64String(contentByte);

                                if(!(await _blobContainer.ExistsAsync(messaging.message.mid)))
                                {
                                    await _blobContainer.SaveAsync(messaging.message.mid, contentByte);
                                }
                                attr.payload.fileBase64 = encodeStr;
                                attr.payload.type = attr.payload.name.Split(".")[1];

                                MessageAttachment mes = new MessageAttachment(channel, messaging.timestamp,
                                       sender.id, sender.name != null ? sender.name : "", sender.profile_pic != null ? sender.profile_pic : "", "null", recipientId, messaging.message.mid + "_" + i,
                                       "", attr, recipientId);

                                //Send to kafka server
                                try
                                {
                                    await _kafkaService.SendFacebookMessage(System.Text.Json.JsonSerializer.Serialize(
                                   new MessageToKafka
                                   {
                                       code = 200,
                                       message = "successfully",
                                       data = mes
                                   }));

                                    i++;

                                    Log.Information("Fb Attachment send to kafka server success");
                                }
                                catch (Exception e)
                                {
                                    Log.Error("Fb Attachment send to kafka server fail : " + JsonConvert.SerializeObject(e));
                                }
                            }
                        }
                    }
                    //message with reaction
                    else
                    {
                        MessageReaction mes = new MessageReaction(channel, messaging.timestamp,
                           senderId, sender.name, sender.profile_pic,
                           "null", recipientId, reaction.mid,
                           reaction.action, reaction.emoji, reaction.reaction, recipientId);

                        try
                        {
                            await _kafkaService.SendFacebookMessage(System.Text.Json.JsonSerializer.Serialize(
                                                        new MessageToKafka
                                                        {
                                                            code = 200,
                                                            message = "successfully",
                                                            data = mes
                                                        }));

                            Log.Information("Fb Message React send to kafka server success");
                        }
                        catch (Exception e)
                        {
                            Log.Error("Fb Message React send to kafka server fail : " + JsonConvert.SerializeObject(e));
                        }
                    }
                }
            }
            else
            {
                Value value = input.entry[0].changes[0].value;
                if (value.verb.Equals("remove"))
                {
                    return Ok();
                }
                else
                {
                    if (!value.verb.Equals("edited"))
                    {
                        string senderId = value.from.id;
                        string pageId = input.entry[0].id;

                        if (value.item.Equals("comment"))
                        {
                            string accessToken = facebookPages["" + pageId];

                            SocialUserInformation sender = null;

                            try
                            {
                                if (sender == null && accessToken != null)
                                {
                                    string fields = "id,name,profile_pic";
                                    string url = string.Format(facebookApi + "/" + facebookVersion + "/" + senderId + "?fields=" + fields + "&access_token=" + accessToken);
                                    using (var client = new HttpClient())
                                    {
                                        var result = client.GetAsync(url).Result;
                                        if (result.IsSuccessStatusCode)
                                        {
                                            var model = result.Content.ReadAsStringAsync().Result;
                                            sender = JsonConvert.DeserializeObject<SocialUserInformation>(model);
                                        }
                                    }
                                }
                            }
                            catch (IOException e)
                            {
                                Log.Information("Fb get user info fail : " + JsonConvert.SerializeObject(e));
                            }

                            if (sender != null)
                            {
                                Comment comment;
                                Attachment attachment = new Attachment();
                                attachment.payload = new Payload();
                                if (value.photo == null && value.video == null)
                                {
                                    comment = new Comment(value.comment_id, value.from.id
                                       , value.from.name, value.post_id, value.message
                                       , value.created_time, value.parent_id
                                       , value.item, sender.profile_pic != null ? sender.profile_pic : "", null);

                                    try
                                    {
                                        await _kafkaService.SendFacebookFeed(System.Text.Json.JsonSerializer.Serialize(
                                          new MessageToKafka
                                          {
                                              code = 200,
                                              message = "successfully",
                                              data = comment
                                          }));

                                        Log.Information("Fb Feed send to kafka server success");
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Error("Fb Feed send to kafka server fail : " + JsonConvert.SerializeObject(e));
                                    }
                                }
                                else
                                {
                                    if (value.photo != null)
                                    {
                                        attachment.type = "image";
                                        attachment.payload.url = value.photo;
                                    }
                                    else
                                    {
                                        attachment.type = "video";
                                        attachment.payload.url = value.video;
                                    }

                                    // Sticker
                                    if (attachment.type.Equals("file") || attachment.type.Equals("image") || attachment.type.Equals("video"))
                                    {
                                        attachment.payload.name = attachment.payload.url.Split("/")[attachment.payload.url.Split("/").Length - 1].Split("?")[0];
                                    }

                                    var url = attachment.payload.url;

                                    //đọc content file
                                    string encodeStr = (new WebClient()).DownloadString(url);
                                    attachment.payload.fileBase64 = encodeStr;
                                    attachment.payload.type = attachment.payload.name.Split(".")[1];

                                    comment = new Comment(value.comment_id, value.from.id
                                    , value.from.name, value.post.id, value.message
                                    , value.created_time, value.parent_id
                                    , value.item, sender.profile_pic != null ? sender.profile_pic : "", attachment);
                                }
                            }
                        }
                    }
                }
            }

            return Ok();
        }

        [HttpPost]
        [Route("/test-kafka")]
        public async Task<IActionResult> TestKafkaAsync()
        {
            dynamic mes = new ExpandoObject();
            mes.messageId = "m_s95_ZROpqPLBx-OpjD7qdRGsHw5pNkMuziMKQ0_lamn7wpNPJPpfS-GneGmK_aRXOUUTrIqWmDtRWhSQxjeFVg";
            mes.text = "Hello";
            mes.attachment = null;
            mes.channel = "Facebook";
            mes.time = 1676011955233;
            mes.senderId = "6097077896993079";
            mes.senderName = "Ng\u1ECDc Nam";
            mes.senderProfilePictur = "https://platform-lookaside.fbsbx.com/platform/profilepic/?psid=6097077896993079\u0026width=1024\u0026ext=1678603963\u0026hash=AeSHdbX41LVWCcJ_m68";
            mes.senderGender = "null";
            mes.recipientId = "107188248928072";
            mes.appId = "10718824.8928072";
            try
            {
                await _kafkaService.SendFacebookMessage(System.Text.Json.JsonSerializer.Serialize(
                new MessageToKafka
                {
                    code = 200,
                    message = "successfully",
                    data = mes
                }));
                Log.Information("TestKafkaAsync OKE! ");
            }
            catch (Exception ex)
            {
                Log.Error("TestKafkaAsync Error: " + JsonConvert.SerializeObject(ex));
            }
            return Ok();
        }

        byte[] GetImage(string url)
        {
            Stream stream = null;
            byte[] buf;

            try
            {
                WebProxy myProxy = new WebProxy();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                stream = response.GetResponseStream();

                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    buf = ms.ToArray();
                }

                stream.Close();
                response.Close();
            }
            catch (Exception exp)
            {
                buf = null;
            }

            return (buf);
        }
    }
}