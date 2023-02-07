using Dapper;
using SocialOut.Interface;
using SocialOut.Model;
using System.Data.SqlClient;

namespace SocialOut.Service
{
    public class CacheService : ICacheService
    {
        private string connectionString;
        private readonly IConfiguration _configuration;
        public CacheService(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("Default");
        }

        public async Task<Dictionary<string, string>> GetZaloInfo()
        {
            try
            {
                string query = "SELECT Id, Token, PageName, PageId, Channel FROM AppSocialManagements WHERE IsDeleted = 0 AND Channel = 1";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    List<SocialManagement> zaloSettings = (await conn.QueryAsync<SocialManagement>(query)).ToList();

                    Dictionary<string, string> result = zaloSettings.ToDictionary(keySelector: m => m.PageId, elementSelector: m => m.Token);

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Dictionary<string, string>> GetFbInfo()
        {
            string query = "SELECT Id, Token, PageName, PageId, Channel FROM AppSocialManagements WHERE IsDeleted = 0 AND Channel = 0";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                List<SocialManagement> zaloSettings = (await conn.QueryAsync<SocialManagement>(query)).ToList();

                Dictionary<string, string> result = zaloSettings.ToDictionary(keySelector: m => m.PageId, elementSelector: m => m.Token);

                return result;
            }
        }

        public async Task<Dictionary<string, string>> GetInfo()
        {
            string query = "SELECT id, token, pageName, pageId, channel FROM AppSocialManagements WHERE IsDeleted = 0";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                List<SocialManagement> zaloSettings = (await conn.QueryAsync<SocialManagement>(query)).ToList();

                Dictionary<string, string> result = zaloSettings.ToDictionary(keySelector: m => m.PageId, elementSelector: m => m.Token);

                return result;
            }
        }
    }
}
