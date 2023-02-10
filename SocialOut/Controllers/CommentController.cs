using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialOut.Interface;
using SocialOut.Model.Input.Facebook;
using SocialOut.Model.Input.Facebook.GraphApi;
using SocialOut.Model.Response;

namespace SocialOut.Controllers
{
    [Route("")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly IFacebookService _facebookService;
        public CommentController(IFacebookService facebookService)
        {
            _facebookService = facebookService;
        }

        [HttpPost]
        [Route("/comment/facebook/reply")]
        public async Task<IActionResult> ReplyComment(ReplyComment input)
        {
            try
            {
                DetailCommentResponse response = await _facebookService.ReplyComment(input);
                if(response == null)
                {
                    return Ok(new ReplyCommentToApp
                    {
                        code = 500,
                        message = "fail",
                        data = response
                    });
                }
                return Ok(new ReplyCommentToApp
                {
                    code = 200,
                    message = "successfully",
                    data = response
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
