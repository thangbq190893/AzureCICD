using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Webhook.Interface;
using Webhook.Model;
using Webhook.Model.Event.Zalo;

namespace Webhook.Controllers
{
    [Route("api/zalo-webhook")]
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
                    MessageAttachment mes;
                    Attachment attachment = new Attachment();
                    attachment.payload = new Payload();
                    if (input.message.attachments == null)
                    {
                        mes = new MessageAttachment("Zalo"
                            , long.Parse(input.timestamp), input.sender.id, sender.name
                            , sender.profile_pic, "", input.recipient.id, input.message.msg_id
                            , input.message.text, null, input.app_id);
                    }
                    else
                    {
                        string fileName;
                        Attachment attr = input.message.attachments[0];
                        Payload payload = new Payload();
                        if (attr.type.Equals("sticker"))
                        {
                            string stickerId = attr.payload.url.Split("?")[1]
                                    .Split("&")[0].Split("=")[1];

                            fileName = stickerId + ".png";

                            payload.(stickerId);
                        }
                        else
                        {
                            // file
                            if (payload.getName() != null)
                            {
                                fileName = payload.getName();
                            }
                            // Ảnh
                            else
                            {
                                fileName = payload.getUrl().split("/")[payload.getUrl().split("/").length - 1];
                            }
                        }


                        mes = new MessageAttachment("Zalo"
                             , long.Parse(input.timestamp), input.sender.id, sender.name
                             , sender.profile_pic, "", input.recipient.id, input.message.msg_id
                             , input.message.text, null, input.app_id);
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
