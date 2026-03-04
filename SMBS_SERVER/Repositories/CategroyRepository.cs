using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.Masters;

namespace SMBS_SERVER.Repositories
{
    public class CategoryRepository
    {
        private readonly IConfiguration _config;
        public CategoryRepository(IConfiguration config) => _config = config;

        public List<CategoryMaster> GetAll(int tenantId)
        {
            var list = new List<CategoryMaster>();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new MySqlCommand(
                "SELECT CategoryID, CategoryName FROM productcategorymaster WHERE TenantID=@TenantID AND IsActive=1", con);

            cmd.Parameters.AddWithValue("@TenantID", tenantId);

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new CategoryMaster
                {
                    CategoryID = rd.GetInt32("CategoryID"),
                    CategoryName = rd.GetString("CategoryName")
                });
            }
            return list;
        }
    }
}
