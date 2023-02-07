using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Text.Json;
using Webhook.Interface;
using Webhook.Model;
using Webhook.Model.Event.Facebook.Comment;
using Webhook.Model.Event.Facebook.Message;
using Webhook.Service;

namespace Webhook.Controllers
{
    [Route("api/messenger-webhook")]
    [ApiController]
    public class MessengerWebhook : ControllerBase
    {
        private readonly ILogger<MessengerWebhook> _logger;
        private readonly IKafkaService _kafkaService;
        private readonly ICacheService _cacheService;
        private readonly ISocialManagementRepository _socialManagementRepository;
        public MessengerWebhook(ICacheService cacheService
            , ILogger<MessengerWebhook> logger
            , IKafkaService kafkaService
            , ISocialManagementRepository socialManagementRepository)
        {
            _logger = logger;
            _kafkaService = kafkaService;
            _cacheService = cacheService;
            _socialManagementRepository = socialManagementRepository;
        }

        [HttpGet]
        public IActionResult VerifyWebhook([FromQuery(Name = "hub.mode")] string mode, [FromQuery(Name = "hub.verify_token")] string verify_token, [FromQuery(Name = "hub.challenge")] string challenge)
        {
            try
            {
                if (!verify_token.Equals("11111"))
                {
                    return StatusCode(422);
                }

                return Ok(challenge);
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> WebhookAsync(Data input)
        {
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
                        //Lấy thông tin người gửi
                        //sender = SocialUtils.checkUserInfoInList(senderId, usersInformation);
                        if (sender == null)
                        {
                            string fields = "id,name,profile_pic";
                            string url = string.Format("https://graph.facebook.com/v13.0/{0}?fields={1}&access_token={2}", senderId, fields, accessToken);
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
                    catch (Exception ex)
                    {
                        _logger.LogError("get user info fail");
                        return Ok();
                    }

                    Reaction? reaction = messaging.reaction;
                    if (reaction == null)
                    {
                        string text = messaging.message.text;
                        //message with text only
                        if (text != null)
                        {
                            MessageAttachment mes = new MessageAttachment(channel, messaging.timestamp,
                                senderId, sender.name, sender.profile_pic,
                                "null", recipientId, messaging.message.mid,
                                text, null, recipientId);

                            //Send to kafka server
                            await _kafkaService.SendFacebookMessage(System.Text.Json.JsonSerializer.Serialize(
                                new MessageToKafka
                                {
                                    code = 200,
                                    message = "successfully",
                                    data = mes
                                }));
                        }
                        //message with attachment only
                        else
                        {
                            int i = 0;
                            foreach (Attachment attachment in messaging.message.attachments)
                            {
                                Attachment attr = attachment;
                                Payload payload = attachment.payload;

                                // Tạo đường dẫn trong ổ
                                if (attachment.type.Equals("file") || attachment.type.Equals("image") || attachment.type.Equals("video"))
                                {
                                    attachment.payload.name = payload.url.Split("/")[payload.url.Split("/").Length - 1].Split("?")[0];
                                }

                                if (payload.sticker_id != null)
                                {
                                    attachment.type = "sticker";
                                }

                                //convert to base64
                                string encodeStr = (new WebClient()).DownloadString(payload.url);
                                attachment.payload.fileBase64 = encodeStr;
                                attachment.payload.type = attachment.payload.name.Split(".")[1];

                                MessageAttachment mes = new MessageAttachment(channel, messaging.timestamp,
                                       sender.id, sender.name, sender.profile_pic, "null", recipientId, messaging.message.mid + "_" + i,
                                       "", attachment, recipientId);

                                //Send to kafka server
                                await _kafkaService.SendFacebookMessage(System.Text.Json.JsonSerializer.Serialize(
                                    new MessageToKafka
                                    {
                                        code = 200,
                                        message = "successfully",
                                        data = mes
                                    }));

                                i++;
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

                        await _kafkaService.SendFacebookMessage(System.Text.Json.JsonSerializer.Serialize(
                            new MessageToKafka
                            {
                                code = 200,
                                message = "successfully",
                                data = mes
                            }));
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
                                    string url = string.Format("https://graph.facebook.com/v13.0/{0}?fields={1}&access_token={2}", senderId, fields, accessToken);
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
                                _logger.LogError("get user info fail");
                                return Ok();
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
                                       , value.item, sender.profile_pic, null);

                                    await _kafkaService.SendFacebookFeed(System.Text.Json.JsonSerializer.Serialize(
                                   new MessageToKafka
                                   {
                                       code = 200,
                                       message = "successfully",
                                       data = comment
                                   }));
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
                                    , value.item, sender.profile_pic, attachment);
                                }
                            }
                        }
                    }
                }
            }

            return Ok();
        }
    }
}