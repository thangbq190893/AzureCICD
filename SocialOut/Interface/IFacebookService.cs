using SocialOut.Model.Input;
using SocialOut.Model.Input.Facebook;
using SocialOut.Model.Input.Facebook.GraphApi;
using SocialOut.Model.Response;
using System.Net.Http;

namespace SocialOut.Interface
{
    public interface IFacebookService
    {
        Task<SendMessageResponseData> SendText(MessageData mes);
        Task<DetailCommentResponse> ReplyComment(ReplyComment input);
        Task<List<SocialInformation>> GetPageInfo(PageInfo input);
        Task SendAttachment(IFormFile filedata, string senderId, string recipient, string message, string type);
    }
}
