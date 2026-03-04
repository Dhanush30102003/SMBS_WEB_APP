using Microsoft.EntityFrameworkCore;
using SMBS_SERVER.Data;
using SMBS_SERVER.Models.Security;

namespace SMBS_SERVER.Repositories
{
    public class EmailOtpRepository
    {
        private readonly ApplicationDbContext _context;

        public EmailOtpRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Save(EmailOtp otp)
        {
            _context.EmailOtps.Add(otp);
            await _context.SaveChangesAsync();
        }

        public async Task<EmailOtp?> GetValidOtp(string email, string otp)
        {
            Console.WriteLine("===== DB DEBUG =====");
            Console.WriteLine("Searching Email: " + email);
            Console.WriteLine("Searching OTP: " + otp);
            return await _context.EmailOtps
                .Where(x =>
                    x.Email.ToLower() == email.Trim().ToLower() &&
                    x.OtpCode.Trim() == otp.Trim() &&
                    !x.IsUsed &&
                    x.ExpiryTime > DateTime.Now)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }


        public async Task MarkAsUsed(EmailOtp otp)
        {
            otp.IsUsed = true;
            await _context.SaveChangesAsync();
        }
    }
}
