using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.Masters;
using SMBS_SERVER.Services;

namespace SMBS_SERVER.Repositories
{
    public class CustomerRepository
    {
        private readonly string _conStr;

        public CustomerRepository(IConfiguration config)
        {
            _conStr = config.GetConnectionString("DefaultConnection");
        }

        private MySqlConnection GetConnection()
            => new MySqlConnection(_conStr);

        public List<CustomerMaster> GetAll(int tenantId)
        {
            var list = new List<CustomerMaster>();

            using var con = GetConnection();
            con.Open();

            string sql = @"SELECT * FROM customermaster WHERE TenantID=@TenantID";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@TenantID", tenantId);

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new CustomerMaster
                {
                    CustomerID = Convert.ToInt32(rd["CustomerID"]),
                    CustomerCode = rd["CustomerCode"].ToString(),
                    CustomerName = rd["CustomerName"].ToString(),
                    Phone = rd["Phone"].ToString(),
                    Email = rd["Email"].ToString(),
                    CustomerType = rd["CustomerType"].ToString(),
                    IsActive = Convert.ToBoolean(rd["IsActive"])
                });
            }

            return list;
        }
        public CustomerMaster GetByPhone(string phone)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"
    SELECT * FROM customermaster 
    WHERE TRIM(Phone) = TRIM(@Phone) 
    AND IsActive = 1
    LIMIT 1";


            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Phone", phone);

            using var rd = cmd.ExecuteReader();

            if (!rd.Read()) return null;

            return new CustomerMaster
            {
                CustomerID = Convert.ToInt32(rd["CustomerID"]),
                CustomerName = rd["CustomerName"].ToString(),
                Phone = rd["Phone"].ToString(),
                Email = rd["Email"].ToString()
            };
        }

        public void Insert(CustomerMaster c)
        {
            using var con = GetConnection();
            con.Open();

            // 🔥 Step 1: Get Next CustomerID
            string getIdSql = "SELECT IFNULL(MAX(CustomerID),0) + 1 FROM customermaster";
            using var getIdCmd = new MySqlCommand(getIdSql, con);
            int nextId = Convert.ToInt32(getIdCmd.ExecuteScalar());

            // 🔥 Step 2: Generate CustomerCode
            string customerCode = "CUST" + nextId.ToString("D5"); // CUST00001

            // 🔥 Step 3: Insert
            string sql = @"
INSERT INTO customermaster
(CustomerID, TenantID, CustomerCode, Username, CustomerName, Phone, Email, PasswordHash, CustomerType, IsActive, CreatedDate)
VALUES
(@CustomerID, @TenantID, @CustomerCode, @Username, @CustomerName, @Phone, @Email, @PasswordHash, @CustomerType, 1, NOW())";

            using var cmd = new MySqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@CustomerID", nextId);
            cmd.Parameters.AddWithValue("@Username", c.Username ?? c.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", c.PasswordHash);
            cmd.Parameters.AddWithValue("@TenantID", c.TenantID);
            cmd.Parameters.AddWithValue("@CustomerCode", customerCode);
            cmd.Parameters.AddWithValue("@CustomerName", c.CustomerName);
            cmd.Parameters.AddWithValue("@Phone", c.Phone ?? "");
            cmd.Parameters.AddWithValue("@Email", c.Email);
            cmd.Parameters.AddWithValue("@CustomerType", c.CustomerType);

            cmd.ExecuteNonQuery();
        }
        public List<CustomerMaster> GetAllWithToken(int tenantId)
        {
            var list = new List<CustomerMaster>();

            using var con = GetConnection();
            con.Open();

            string sql = @"
    SELECT * FROM customermaster
    WHERE TenantID = @TenantID
    AND IsActive = 1
    AND FCMToken IS NOT NULL
    AND FCMToken <> ''";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@TenantID", tenantId);

            using var rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                list.Add(new CustomerMaster
                {
                    CustomerID = Convert.ToInt32(rd["CustomerID"]),
                    CustomerName = rd["CustomerName"].ToString(),
                    Email = rd["Email"].ToString(),
                    Phone = rd["Phone"].ToString(),
                    FCMToken = rd["FCMToken"]?.ToString(),
                    TenantID = Convert.ToInt32(rd["TenantID"])
                });
            }

            return list;
        }
        public CustomerMaster GetById(int id)
        {
            using var con = GetConnection();
            con.Open();

            string sql = "SELECT * FROM customermaster WHERE CustomerID=@id";
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);

            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            return new CustomerMaster
            {
                CustomerID = id,
                TenantID = Convert.ToInt32(rd["TenantID"]),
                CustomerCode = rd["CustomerCode"]?.ToString(),
                CustomerName = rd["CustomerName"]?.ToString(),
                Phone = rd["Phone"]?.ToString(),
                Email = rd["Email"]?.ToString(),
                Gender = rd["Gender"]?.ToString(),
                Address = rd["Address"]?.ToString(),
                Pincode = rd["Pincode"]?.ToString(),
                CustomerType = rd["CustomerType"]?.ToString(),
                DateOfBirth = rd["DateOfBirth"] as DateTime?,
                Anniversary = rd["Anniversary"] as DateTime?,
                CityID = rd["CityID"] as int?,
                StateID = rd["StateID"] as int?,
                CountryID = rd["CountryID"] as int?,
                FCMToken = rd["FCMToken"]?.ToString(),
                IsActive = Convert.ToBoolean(rd["IsActive"]),
                CreatedDate = Convert.ToDateTime(rd["CreatedDate"]),
                ModifiedDate = rd["ModifiedDate"] as DateTime?
            };
        }
        public (string Name, string Phone) GetCustomerBasicInfo(int customerId)
        {
            using var conn = GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
        SELECT CustomerName, Phone
        FROM customermaster
        WHERE CustomerID = @CustomerID
    ", conn);

            cmd.Parameters.AddWithValue("@CustomerID", customerId);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return (
                    reader["CustomerName"]?.ToString() ?? "",
                    reader["Phone"]?.ToString() ?? ""
                );
            }

            return ("", "");
        }
        public CustomerMaster Authenticate(string email, string password)
        {
            using var con = GetConnection();
            con.Open();
            string sql = @"
    SELECT * FROM customermaster
    WHERE LOWER(Email) = LOWER(@Email)
    AND IsActive = 1
    AND PasswordHash IS NOT NULL
    ORDER BY CustomerID DESC
    LIMIT 1";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Email", email.Trim());

            using var rd = cmd.ExecuteReader();

            if (!rd.Read())
                return null;

            string dbHash = rd["PasswordHash"]?.ToString();
            string inputHash = PasswordHasher.Hash(password.Trim());
            Console.WriteLine("DB HASH: " + dbHash);
            Console.WriteLine("INPUT HASH: " + inputHash);

            if (!string.Equals(dbHash, inputHash, StringComparison.OrdinalIgnoreCase))
                return null;

            return new CustomerMaster
            {
                CustomerID = Convert.ToInt32(rd["CustomerID"]),
                CustomerName = rd["CustomerName"].ToString(),
                Email = rd["Email"].ToString(),
                TenantID = Convert.ToInt32(rd["TenantID"])
            };
        }

        public void Update(CustomerMaster c)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"
                UPDATE customermaster SET
                CustomerName=@CustomerName,
                Phone=@Phone,
                Email=@Email,
                CustomerType=@CustomerType,
                ModifiedDate=NOW()
                WHERE CustomerID=@CustomerID";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@CustomerName", c.CustomerName);
            cmd.Parameters.AddWithValue("@Phone", c.Phone);
            cmd.Parameters.AddWithValue("@Email", c.Email);
            cmd.Parameters.AddWithValue("@CustomerType", c.CustomerType);
            cmd.Parameters.AddWithValue("@CustomerID", c.CustomerID);

            cmd.ExecuteNonQuery();
        }
        public void ToggleStatus(int customerId)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"
        UPDATE customermaster
        SET IsActive = CASE 
                        WHEN IsActive = 1 THEN 0 
                        ELSE 1 
                      END,
            ModifiedDate = NOW()
        WHERE CustomerID = @id";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", customerId);

            cmd.ExecuteNonQuery();
        }
        public CustomerMaster GetByUsernameOrEmail(string username)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"
        SELECT * FROM customermaster
        WHERE (LOWER(Email) = LOWER(@Username)
               OR LOWER(Username) = LOWER(@Username))
        AND IsActive = 1
        LIMIT 1";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Username", username);

            using var rd = cmd.ExecuteReader();

            if (!rd.Read()) return null;

            return new CustomerMaster
            {
                CustomerID = Convert.ToInt32(rd["CustomerID"]),
                CustomerName = rd["CustomerName"].ToString(),
                Email = rd["Email"].ToString(),
                Username = rd["Username"]?.ToString(),
                PasswordHash = rd["PasswordHash"]?.ToString(),
                IsActive = Convert.ToBoolean(rd["IsActive"])
            };
        }

        public CustomerMaster GetByCode(string customerCode, int tenantId)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"
        SELECT * FROM customermaster
        WHERE CustomerCode = @Code
        AND TenantID = @TenantID
        LIMIT 1";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Code", customerCode);
            cmd.Parameters.AddWithValue("@TenantID", tenantId);

            using var rd = cmd.ExecuteReader();

            if (!rd.Read())
                return null;

            return new CustomerMaster
            {
                CustomerID = Convert.ToInt32(rd["CustomerID"]),
                CustomerCode = rd["CustomerCode"].ToString(),
                CustomerName = rd["CustomerName"].ToString(),
                Phone = rd["Phone"].ToString(),
                Email = rd["Email"].ToString(),
                CustomerType = rd["CustomerType"].ToString(),
                TenantID = Convert.ToInt32(rd["TenantID"])
            };
        }
        public void UpdateFcmToken(int customerId, string token)
        {
            using var conn = GetConnection();
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                // Remove token from any other user first
                var clearCmd = new MySqlCommand(@"
            UPDATE customermaster
            SET FcmToken = NULL
            WHERE FcmToken = @Token
        ", conn, transaction);

                clearCmd.Parameters.AddWithValue("@Token", token);
                clearCmd.ExecuteNonQuery();

                // Assign token to this customer
                var cmd = new MySqlCommand(@"
            UPDATE customermaster
            SET FcmToken = @Token
            WHERE CustomerID = @CustomerID
        ", conn, transaction);

                cmd.Parameters.AddWithValue("@Token", token);
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                cmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public void UpdateProfile(int customerId, UpdateCustomerProfileDto dto)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"
        UPDATE customermaster
        SET CustomerName = @Name,
            Phone = @Phone,
            Gender = @Gender,
            Address = @Address,
            Pincode = @Pincode,
            ModifiedDate = NOW()
        WHERE CustomerID = @CustomerID";

            using var cmd = new MySqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@Name", dto.CustomerName);
            cmd.Parameters.AddWithValue("@Phone", dto.Phone);
            cmd.Parameters.AddWithValue("@Gender", dto.Gender);
            cmd.Parameters.AddWithValue("@Address", dto.Address);
            cmd.Parameters.AddWithValue("@Pincode", dto.Pincode);
            cmd.Parameters.AddWithValue("@CustomerID", customerId);

            cmd.ExecuteNonQuery();
        }
        public void UpdatePassword(string email, string newPasswordHash)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"
        UPDATE customermaster
        SET PasswordHash = @PasswordHash,
            ModifiedDate = NOW()
        WHERE LOWER(Email) = LOWER(@Email)
        AND IsActive = 1";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
            cmd.Parameters.AddWithValue("@Email", email);

            cmd.ExecuteNonQuery();
        }
        public void Delete(int id)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"
        UPDATE customermaster
        SET IsActive = 0,
            ModifiedDate = NOW()
        WHERE CustomerID = @id";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }

    }
}
