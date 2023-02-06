using Microsoft.AspNetCore.Mvc;
using Webhook.Interface;
using Webhook.Model.Event.Zalo;

namespace Webhook.Controllers
{
    [Route("api/messenger-webhook")]
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

        //[HttpPost]
        //public async Task<IActionResult> WebhookAsync(Data input)
        //{

        //}
    }
}
