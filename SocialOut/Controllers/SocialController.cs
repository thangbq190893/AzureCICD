using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SocialOut.Interface;
using SocialOut.Model.Input;
using SocialOut.Model.Input.Facebook;
using SocialOut.Model.Input.Zalo;
using SocialOut.Model.Response;
using System.Text;

namespace SocialOut.Controllers
{
    [Route("")]
    [ApiController]
    public class SocialController : ControllerBase
    {
        private readonly IFacebookService _facebookService;
        private readonly IZaloService _zaloService;
        public SocialController(IFacebookService facebookService, IZaloService zaloService)
        {
            _facebookService = facebookService;
            _zaloService = zaloService;
        }

        [HttpPost]
        [Route("/facebook/getPageInfo")]
        public async Task<IActionResult> GetPageInfo(PageInfo input)
        {
            try
            {
                List<SocialInformation> res = await _facebookService.GetPageInfo(input);

                return Ok(new PageInfoResponse
                {
                    code = 200,
                    message = "successfully",
                    data = res
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("/sendMessage/facebook/text")]
        public async Task<IActionResult> SendMessageToFacebook(MessageData mes)
        {
            try
            {
                SendMessageResponseData res = await _facebookService.SendText(mes);
                if (res != null)
                {
                    return Ok(new SendMessage
                    {
                        code = 200,
                        message = "successfully",
                        data = res
                    });
                }
                else
                {
                    return StatusCode(500);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("/sendMessage/facebook/attachment")]
        public async Task<IActionResult> SendMessageAttachmentToFacebook([FromForm] FileData input)
        {
            try
            {
                await _facebookService.SendAttachment(input.filedata, input.data.senderId, input.data.recipient, input.data.message, input.data.type);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("/refreshToken/zaloOa")]
        public async Task<IActionResult> RefreshToken(GetZaloToken input)
        {
            try
            {
                await _zaloService.RefreshTokenZaloOa(input);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
