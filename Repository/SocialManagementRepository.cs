using System.Data.SqlClient;
using Webhook.Interface;
using Webhook.Model;
using Dapper;

namespace Webhook.Repository
{
    public class SocialManagementRepository : ISocialManagementRepository
    {
        private string connectionString;
        private readonly IConfiguration _configuration;
        public SocialManagementRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("Default");
        }

        public async Task<Dictionary<string, string>> GetZaloPages()
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

        public async Task<Dictionary<string, string>> GetFbPages()
        {
            string query = "SELECT Id, Token, PageName, PageId, Channel FROM AppSocialManagements WHERE IsDeleted = 0 AND Channel = 0";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                List<SocialManagement> zaloSettings = (await conn.QueryAsync<SocialManagement>(query)).ToList();

                Dictionary<string, string> result = zaloSettings.ToDictionary(keySelector: m => m.PageId, elementSelector: m => m.Token);

                return result;
            }
        }

        public async Task<Dictionary<string, string>> GetPages()
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
