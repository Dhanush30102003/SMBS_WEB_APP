using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.Masters;

namespace SMBS_SERVER.Services
{
    public class CategoryService
    {
        private readonly IConfiguration _config;

        public CategoryService(IConfiguration config)
        {
            _config = config;
        }

        public List<CategoryMaster> GetAll(int tenantId)
        {
            var list = new List<CategoryMaster>();

            using var con = new MySqlConnection(
                _config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new MySqlCommand(@"
                SELECT CategoryID, CategoryName
                FROM productcategorymaster
                WHERE TenantID = @TenantID AND IsActive = 1
                ORDER BY CategoryName", con);

            cmd.Parameters.AddWithValue("@TenantID", tenantId);

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new CategoryMaster
                {
                    CategoryID = rdr.GetInt32("CategoryID"),
                    CategoryName = rdr.GetString("CategoryName")
                });
            }

            return list;
        }
    }
}
