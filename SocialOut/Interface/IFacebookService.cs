using SocialOut.Model.Input;
using SocialOut.Model.Input.Facebook;
using SocialOut.Model.Response;
using System.Net.Http;

namespace SocialOut.Interface
{
    public interface IFacebookService
    {
        Task<SendMessageResponseData> SendText(MessageData mes);
        Task ReplyComment(ReplyComment input);
        Task<List<SocialInformation>> GetPageInfo(PageInfo input);
        Task SendAttachment(MultipartFileData file, string senderId, string recipient, string message, string type);
    }
}
