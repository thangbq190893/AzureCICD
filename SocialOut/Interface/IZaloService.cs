using SocialOut.Model.Input;
using SocialOut.Model.Input.Zalo;

namespace SocialOut.Interface
{
    public interface IZaloService
    {
        Task SendText(MessageData mes);
        Task RefreshTokenZaloOa(GetZaloToken input);
    }
}
