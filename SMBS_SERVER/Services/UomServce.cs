using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.Masters;

namespace SMBS_SERVER.Services
{
    public class UomService
    {
        private readonly IConfiguration _config;

        public UomService(IConfiguration config)
        {
            _config = config;
        }

        public List<UomMaster> GetAll()
        {
            var list = new List<UomMaster>();

            using var con = new MySqlConnection(
                _config.GetConnectionString("DefaultConnection"));
            con.Open();

            var cmd = new MySqlCommand(@"
                SELECT UOMID, UOMName
                FROM uommaster
                WHERE IsActive = 1
                ORDER BY UOMName", con);

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                list.Add(new UomMaster
                {
                    UOMID = rdr.GetInt32("UOMID"),
                    UOMName = rdr.GetString("UOMName")
                });
            }

            return list;
        }
    }
}
