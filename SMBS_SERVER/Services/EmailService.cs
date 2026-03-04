using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

using SMBS_SERVER.Models.Security;


namespace SMBS_SERVER.Services
{
    public class EmailService
    {
        private readonly SmtpSettings _smtp;

        public EmailService(IOptions<SmtpSettings> smtp)
        {
            _smtp = smtp.Value;
        }

        public async Task SendOtpEmail(string toEmail, string otp)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_smtp.DisplayName, _smtp.Email));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "SMBS Registration OTP";

            email.Body = new TextPart("html")
            {
                Text = $"<h2>Your OTP is: {otp}</h2><p>Valid for 5 minutes.</p>"
            };
            using var smtpClient = new MailKit.Net.Smtp.SmtpClient();

            await smtpClient.ConnectAsync(
                                             _smtp.Host,
                                             _smtp.Port,
                                             SecureSocketOptions.SslOnConnect);

            await smtpClient.AuthenticateAsync(_smtp.Email, _smtp.Password);
            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);
        }
    }
}
