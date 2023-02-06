namespace Webhook.Interface
{
    public interface ICacheService
    {
        Task<Dictionary<string, string>> GetFacebookPagesSetting();
        Task<Dictionary<string, string>> GetZaloPagesSetting();
        Task<Dictionary<string, string>> GetAllPagesSetting();
    }
}
