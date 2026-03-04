using Microsoft.EntityFrameworkCore;
using SMBS_SERVER.Data;
using SMBS_SERVER.Models.Security;
using SMBS_SERVER.Repositories;
using System.Security.Cryptography;

namespace SMBS_SERVER.Services
{
    public class OtpService
    {
        private readonly EmailOtpRepository _otpRepo;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;

        public OtpService(EmailOtpRepository otpRepo, EmailService emailService, ApplicationDbContext context)
        {
            _otpRepo = otpRepo;
            _emailService = emailService;
            _context = context;

        }

        public async Task SendOtp(string email)
        {
            string otp = GenerateOtp();

            // 🔥 Invalidate previous unused OTPs
            var oldOtps = _context.EmailOtps
                .Where(x => x.Email.ToLower() == email.ToLower()
                            && !x.IsUsed
                            && x.ExpiryTime > DateTime.Now);

            foreach (var old in oldOtps)
            {
                old.IsUsed = true;
            }

            await _context.SaveChangesAsync();

            // Insert new OTP
            await _otpRepo.Save(new EmailOtp
            {
                Email = email,
                OtpCode = otp,
                IsUsed = false,
                ExpiryTime = DateTime.Now.AddMinutes(5),
                CreatedAt = DateTime.Now
            });

            await _emailService.SendOtpEmail(email, otp);
        }

        public async Task<bool> VerifyOtp(string email, string otp)
        {

            Console.WriteLine("===== VERIFY DEBUG =====");
            Console.WriteLine("EMAIL: '" + email + "'");
            Console.WriteLine("OTP FROM FLUTTER: '" + otp + "'");
            Console.WriteLine("OTP LENGTH: " + otp.Length);
            var validOtp = await _otpRepo.GetValidOtp(email, otp);

            if (validOtp == null)
                return false;

            await _otpRepo.MarkAsUsed(validOtp);
            return true;
        }

        private string GenerateOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            int number = Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000);
            return number.ToString("D6");
        }
    }
}
