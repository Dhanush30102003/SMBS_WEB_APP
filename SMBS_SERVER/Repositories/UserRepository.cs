using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.Masters;
using SMBS_SERVER.Services;
using System.Data;


namespace SMBS_SERVER.Repositories
{
    public class UserRepository
    {
        private readonly string _connStr;

        public UserRepository(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection");
        }
        public UserMaster GetByUserName(string userName, int tenantId)
        {
            using var con = new MySqlConnection(_connStr);
            con.Open();

            string sql = @"
        SELECT * FROM usermaster
        WHERE Username = @Username
        AND TenantID = @TenantID
        LIMIT 1";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Username", userName);
            cmd.Parameters.AddWithValue("@TenantID", tenantId);

            using var rd = cmd.ExecuteReader();

            if (!rd.Read())
                return null;

            return new UserMaster
            {
                UserID = Convert.ToInt32(rd["UserID"]),
                UserName = rd["Username"].ToString(),
                Email = rd["Email"]?.ToString(),
                Phone = rd["Phone"]?.ToString(),
                RoleID = Convert.ToInt32(rd["RoleID"]),
                TenantID = Convert.ToInt32(rd["TenantID"]),
                IsLocked = Convert.ToBoolean(rd["IsLocked"]),
                IsActive = Convert.ToBoolean(rd["IsActive"])
            };
        }

        public UserMaster Authenticate(string username, string password, int tenantId)
        {
            Console.WriteLine("==== AUTHENTICATE HIT ====");
            Console.WriteLine($"Username Param: '{username}'");
            Console.WriteLine($"Password Param: '{password}'");
            Console.WriteLine($"TenantID: {tenantId}");

            using var con = new MySqlConnection(_connStr);
            con.Open();

            string sql = @"
SELECT UserID, Username, PasswordHash, RoleID, TenantID, IsLocked, IsActive
FROM usermaster
WHERE (LOWER(Email) = LOWER(@Login) OR LOWER(Username) = LOWER(@Login))
  AND TenantID = @TenantID
  AND IsActive = 1
LIMIT 1";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Login", username.Trim());
            cmd.Parameters.AddWithValue("@TenantID", tenantId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            string dbHash = reader.GetString("PasswordHash");
            string inputHash = PasswordHasher.Hash(password.Trim());

            Console.WriteLine($"DB HASH    : {dbHash}");
            Console.WriteLine($"INPUT HASH : {inputHash}");

            // ✅ FIX — CHECK PASSWORD HERE
            if (inputHash != dbHash)
            {
                Console.WriteLine("❌ Password mismatch");
                return null;
            }

            Console.WriteLine("✅ Password matched");

            return new UserMaster
            {
                UserID = reader.GetInt32("UserID"),
                UserName = reader.GetString("Username"),
                RoleID = reader.GetInt32("RoleID"),
                TenantID = reader.GetInt32("TenantID"),
                IsLocked = reader.GetBoolean("IsLocked"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }


        // ============================================================
        // INSERT USER (FOR SYNC)
        // ============================================================
        public void Insert(UserMaster user)
        {
            using var con = new MySqlConnection(_connStr);
            con.Open();

            string sql = @"
        INSERT INTO usermaster
        (
            TenantID,
            BranchID,
            EmployeeID,
            Username,
            FullName,
            Email,
            Phone,
            PasswordHash,
            RoleID,
            IsLocked,
            IsActive,
            CreatedBy,
            CreatedDate,
            ModifiedBy,
            ModifiedDate
        )
        VALUES
        (
            @TenantID,
            @BranchID,
            @EmployeeID,
            @Username,
            @FullName,
            @Email,
            @Phone,
            @PasswordHash,
            @RoleID,
            @IsLocked,
            @IsActive,
            @CreatedBy,
            NOW(),
            @ModifiedBy,
            NOW()
        )";

            using var cmd = new MySqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@TenantID", user.TenantID);
            cmd.Parameters.AddWithValue("@BranchID", user.BranchID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@EmployeeID", user.EmployeeID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Username", user.UserName);
            cmd.Parameters.AddWithValue("@FullName", user.FullName ?? "");
            cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", user.Phone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? "");
            cmd.Parameters.AddWithValue("@RoleID", user.RoleID);
            cmd.Parameters.AddWithValue("@IsLocked", user.IsLocked);
            cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
            cmd.Parameters.AddWithValue("@CreatedBy", user.CreatedBy ?? 1);
            cmd.Parameters.AddWithValue("@ModifiedBy", user.ModifiedBy ?? 1);

            cmd.ExecuteNonQuery();
        }
        public void Update(UserMaster user)
        {
            using var con = new MySqlConnection(_connStr);
            con.Open();

            string sql = @"
        UPDATE usermaster SET
            Email=@Email,
            Phone=@Phone,
            RoleID=@RoleID,
            IsLocked=@IsLocked,
            IsActive=@IsActive,
            ModifiedDate=NOW()
        WHERE UserID=@UserID";

            using var cmd = new MySqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@UserID", user.UserID);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@Phone", user.Phone);
            cmd.Parameters.AddWithValue("@RoleID", user.RoleID);
            cmd.Parameters.AddWithValue("@IsLocked", user.IsLocked);
            cmd.Parameters.AddWithValue("@IsActive", user.IsActive);

            cmd.ExecuteNonQuery();
        }



    }
}
