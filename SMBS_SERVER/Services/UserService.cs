using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.Masters;
using SMBS_SERVER.Services;
using System.Numerics;
using System.Security.Policy;

namespace SMBS_SERVER.Services
{
    public class UserService
    {
        private readonly IConfiguration _config;

        public UserService(IConfiguration config)
        {
            _config = config;
        }
      

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        // ✅ GET ALL EMPLOYEES
        public List<UserMaster> GetAllEmployees()
        {
            var list = new List<UserMaster>();

            using var con = GetConnection();
            con.Open();

            string sql = @"SELECT
    UserID,
    TenantID,
    BranchID,
    EmployeeID,
    Username,
    FullName,
    Email,
    Phone,
    RoleID,
    IsLocked,
    IsActive
FROM usermaster";


            using var cmd = new MySqlCommand(sql, con);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new UserMaster
                {
                    UserID = reader.GetInt32("UserID"),
                    TenantID = reader.GetInt32("TenantID"),
                    BranchID = reader["BranchID"] as int?,
                    EmployeeID = reader["EmployeeID"] as int?,
                    UserName = reader.GetString("Username"),
                    FullName = reader["FullName"]?.ToString() ?? "",

                    Email = reader["Email"]?.ToString(),
                    Phone = reader["Phone"]?.ToString(),
                    RoleID = reader.GetInt32("RoleID"),
                    IsLocked = reader.GetBoolean("IsLocked"),
                    IsActive = reader.GetBoolean("IsActive")
                });
            }

            return list;
        }
        public UserMaster? GetByEmail(string email)
        {
            using var con = GetConnection();
            con.Open();

            string sql = "SELECT * FROM usermaster WHERE Username = @email LIMIT 1";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@email", email);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new UserMaster
            {
                UserID = reader.GetInt32("UserID"),
                UserName = reader.GetString("Username"),
                RoleID = reader.GetInt32("RoleID"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }

        public UserMaster? GetById(int id)
        {
            using var con = GetConnection();
            con.Open();

            string sql = "SELECT * FROM usermaster WHERE UserID=@id";
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new UserMaster
            {
                UserID = r.GetInt32("UserID"),
                UserName = r.GetString("Username"),
                FullName = r["FullName"]?.ToString() ?? "",

                Email = r["Email"]?.ToString(),
                Phone = r["Phone"]?.ToString(),
                RoleID = r.GetInt32("RoleID"),
                IsActive = r.GetBoolean("IsActive"),
                IsLocked = r.GetBoolean("IsLocked")
            };
        }
        // ✅ GET BY ID

        public void CreateUserFromRegistration(string email, string passwordHash)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"INSERT INTO usermaster
    (
        TenantID,
        Username,
        Email,
        PasswordHash,
        RoleID,
        IsActive,
        IsLocked,
        CreatedDate
    )
    VALUES
    (
        1,
        @Username,
        @Email,
        @PasswordHash,
        2,
        1,
        0,
        NOW()
    )";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Username", email);  // or separate username if you want
            cmd.Parameters.AddWithValue("@Email", email);     // 🔥 important
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

            cmd.ExecuteNonQuery();
        }

        // ✅ SAVE (INSERT / UPDATE)
        public void SaveEmployee(UserMaster user, int createdByUserId)
{
    using var con = GetConnection();
    con.Open();

    if (user.UserID == 0)
    {
        if (string.IsNullOrWhiteSpace(user.Password))
            throw new Exception("Password is required");

                // 🔐 CREATE HASH + SALT (FIX)
                string passwordHash = PasswordHasher.Hash(user.Password);

                string sql = @"INSERT INTO usermaster
        (
            TenantID,
            BranchID,
            Username,
            FullName,
            PasswordHash,
            Email,
            Phone,
            RoleID,
            IsActive,
            CreatedBy,
            CreatedDate
        )
        VALUES
        (
            1,
            @BranchID,
            @Username,
            @FullName,
            @PasswordHash,
            @Email,
            @Phone,
            @RoleID,
            1,
            @CreatedBy,
            NOW()
        )";

        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@BranchID", user.BranchID);
        cmd.Parameters.AddWithValue("@Username", user.UserName);
        cmd.Parameters.AddWithValue("@FullName", user.FullName);
        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@Phone", user.Phone);
        cmd.Parameters.AddWithValue("@RoleID", user.RoleID);
        cmd.Parameters.AddWithValue("@CreatedBy", createdByUserId);

        cmd.ExecuteNonQuery();
    }
    else
    {
        string sql = @"UPDATE usermaster SET
            FullName=@FullName,
            Email=@Email,
            Phone=@Phone,
            RoleID=@RoleID,
            BranchID=@BranchID,
            ModifiedBy=@ModifiedBy,
            ModifiedDate=NOW()
            WHERE UserID=@UserID";

        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@FullName", user.FullName);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@Phone", user.Phone);
        cmd.Parameters.AddWithValue("@RoleID", user.RoleID);
        cmd.Parameters.AddWithValue("@BranchID", user.BranchID);
        cmd.Parameters.AddWithValue("@ModifiedBy", createdByUserId);
        cmd.Parameters.AddWithValue("@UserID", user.UserID);

        cmd.ExecuteNonQuery();
    }
}


    }
}


    