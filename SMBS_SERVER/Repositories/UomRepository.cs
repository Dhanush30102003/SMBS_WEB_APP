using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.Masters;

namespace SMBS_SERVER.Repositories
{
    public class UomRepository
    {
        private readonly IConfiguration _config;
        public UomRepository(IConfiguration config) => _config = config;

        public List<UomMaster> GetAll()
        {
            var list = new List<UomMaster>();
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new MySqlCommand(
                "SELECT UOMID, UOMName FROM uommaster WHERE IsActive=1", con);

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new UomMaster
                {
                    UOMID = rd.GetInt32("UOMID"),
                    UOMName = rd.GetString("UOMName")
                });
            }
            return list;
        }
    }
}
