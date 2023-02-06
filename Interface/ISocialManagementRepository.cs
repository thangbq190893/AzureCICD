namespace Webhook.Interface
{
    public interface ISocialManagementRepository
    {
        Task<Dictionary<string, string>> GetZaloPages();
        Task<Dictionary<string, string>> GetFbPages();
        Task<Dictionary<string, string>> GetPages();
    }
}
