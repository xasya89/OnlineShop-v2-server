using Dapper;
using MySql.Data.MySqlClient;

namespace OnlineShop2.Api.Services.Legacy
{
    public class GoodGroupLegacyService
    {
        private readonly IConfiguration _configuration;

        public GoodGroupLegacyService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> Create(int shopLegacyId, string groupName)
        {
            using (MySqlConnection con = new MySqlConnection(_configuration.GetConnectionString("shop" + shopLegacyId)))
            {
                con.Open();
                return await con.QuerySingleAsync<int>($"INSERT INTO goodgroups (Name) VALUES (@GroupName); SELECT LAST_INSERT_ID()",
                    new { GroupName = groupName });
            }
        }
    }
}
