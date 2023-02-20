namespace SocialOut.Interface
{
    public interface ICacheService
    {
        Task<Dictionary<string, string>> GetZaloInfo();
        Task<Dictionary<string, string>> GetFbInfo();
        Task<Dictionary<string, string>> GetInfo();
    }
}
