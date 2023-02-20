using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using Webhook.Interface;
using Webhook.Model;
using Webhook.Model.Event.Zalo;

namespace Webhook.Controllers
{
    [ApiController]
    public class ZaloWebhook : ControllerBase
    {
        private readonly ILogger<MessengerWebhook> _logger;
        private readonly ISocialManagementRepository _socialManagementRepository;
        private readonly IKafkaService _kafkaService;
        public ZaloWebhook(ISocialManagementRepository socialManagementRepository
            , ILogger<MessengerWebhook> logger
            , IKafkaService kafkaService)
        {
            _socialManagementRepository = socialManagementRepository;
            _logger = logger;
            _kafkaService = kafkaService;
        }

        [HttpPost]
        [Route("/api/zalo-webhook")]
        public async Task<IActionResult> WebhookAsync(Data input)
        {
            if(input.message.msg_id.Equals("This is message id") && input.message.text.Equals("This is testing message"))
            {
                return Ok();
            }
            if(input.event_name != null)
            {
                Dictionary<string, string> ZaloPages = await _socialManagementRepository.GetZaloPages();

                //get info
                string senderId = input.sender.id;
                string recipient = input.recipient.id;
                string appId = input.app_id;
                string accessToken = ZaloPages[appId];

                SocialUserInformation sender = new SocialUserInformation();

                if (accessToken != null)
                {
                    try
                    {
                        string url = string.Format("https://openapi.zalo.me/v2.0/oa/getprofile?data={\"user_id\":\"" + senderId + "\"}&access_token=" + accessToken);
                        using(HttpClient client = new HttpClient())
                        {
                            var result = await client.GetAsync(url);
                            if (result.IsSuccessStatusCode)
                            {
                                var model = result.Content.ReadAsStringAsync().Result;
                                sender = JsonConvert.DeserializeObject<SocialUserInformation>(model);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        sender.profile_pic = "";
                        sender.name = "";
                    }

                    //Gửi text hoặc attachment
                    MessageAttachment mes = null;
                    if (input.message.attachments == null)
                    {
                        mes = new MessageAttachment("Zalo"
                            , long.Parse(input.timestamp), input.sender.id, sender.name
                            , sender.profile_pic, "", input.recipient.id, input.message.msg_id
                            , input.message.text, null, input.app_id);
                    }
                    else
                    {
                        Webhook.Model.Event.Attachment attr = input.message.attachments[0];
                        string fileName;
                        if (attr.type.Equals("sticker"))
                        {
                            string stickerId = input.message.attachments[0].payload.url.Split("?")[1]
                                    .Split("&")[0].Split("=")[1];

                            fileName = stickerId + ".png";

                            attr.payload.sticker_id = long.Parse(stickerId);
                        }
                        else
                        {
                            // file
                            if (attr.payload.name != null)
                            {
                                fileName = input.message.attachments[0].payload.name;
                            }
                            // Ảnh
                            else
                            {
                                fileName = input.message.attachments[0].payload.url.Split("/")[input.message.attachments[0].payload.url.Split("/").Length - 1];
                            }

                            string encodeStr = (new WebClient()).DownloadString(input.message.attachments[0].payload.url);
                            attr.payload.fileBase64 = encodeStr;

                            if (attr.payload.name == null)
                            {
                                attr.payload.name = fileName;
                            }

                            if (attr.payload.type == null)
                            {
                                attr.payload.type = fileName.Split(".")[1];
                            }

                            if (input.message.text == null)
                            { // message with attachment only
                                mes = new MessageAttachment("Zalo", long.Parse(input.timestamp), senderId
                                    , sender.name, sender.profile_pic, "", input.recipient.id,
                                        input.message.msg_id, "", attr, appId);
                            }
                            else
                            { //message with text + attachment
                                mes = new MessageAttachment("Zalo", long.Parse(input.timestamp), senderId
                                    , sender.name, sender.profile_pic, "", input.recipient.id,
                                        input.message.msg_id, input.message.text, attr, appId);
                            }
                        }
                    }

                    await _kafkaService.SendZaloMessage(System.Text.Json.JsonSerializer.Serialize(
                           new MessageToKafka
                           {
                               code = 200,
                               message = "successfully",
                               data = mes
                           }));

                    return Ok();
                }
            }

            return Ok();
        }
    }
}
