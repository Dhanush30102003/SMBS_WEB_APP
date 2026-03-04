using Microsoft.EntityFrameworkCore;
using SMBS_SERVER.Models.Masters;
using SMBS_SERVER.Models.Security;

namespace SMBS_SERVER.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProductMaster> ProductMasters { get; set; }
        public DbSet<PendingUserRegistration> PendingUserRegistrations { get; set; }
        public DbSet<EmailOtp> EmailOtps { get; set; }
    }
}
