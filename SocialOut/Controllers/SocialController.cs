using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialOut.Interface;
using SocialOut.Model.Input;
using SocialOut.Model.Input.Facebook;
using SocialOut.Model.Input.Zalo;
using SocialOut.Model.Response;

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
                    message = "successful",
                    data = res
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        //[HttpPost]
        //[Route("/sendMessage/facebook/text")]
        //public async Task<IActionResult> SendMessageToFacebook(MessageData mes)
        //{
        //    try
        //    {
        //        await _facebookService.SendText(mes);
        //        return StatusCode(200);
        //    }
        //    catch(Exception ex)
        //    {
        //        return StatusCode(500);
        //    }
        //}

        //[HttpPost]
        //[Route("/refreshToken/zaloOa")]
        //public async Task<IActionResult> RefreshToken(GetZaloToken input)
        //{
        //    try
        //    {
        //        await _zaloService.RefreshTokenZaloOa(input);
        //        return Ok();
        //    }
        //    catch(Exception ex)
        //    {
        //        return StatusCode(500);
        //    }
        //}
    }
}
