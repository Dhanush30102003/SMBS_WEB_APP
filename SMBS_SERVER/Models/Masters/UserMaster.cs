using System.ComponentModel.DataAnnotations.Schema;




namespace SMBS_SERVER.Models.Masters
{
    public class UserMaster
    {
        public int UserID { get; set; }
        public int TenantID { get; set; }
        public int? BranchID { get; set; }
        public int? EmployeeID { get; set; }
        [NotMapped]
        public string? Password { get; set; }
        public string UserName { get; set; } = string.Empty;

        // 🔥 REQUIRED BY MVC VIEWS
        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? Phone { get; set; }

        public string PasswordHash { get; set; } = string.Empty;
        public string? PasswordSalt { get; set; }

        public int RoleID { get; set; }

        public bool IsLocked { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int? CreatedBy { get; set; }
      
        public int? ModifiedBy { get; set; }


        public DateTime? LastLoginDate { get; set; }
        public int FailedLoginAttempts { get; set; }
    }
}
