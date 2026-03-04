using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using static Google.Apis.Requests.BatchRequest;
namespace SMBS_SERVER.Services
{
    public class FcmService
    {
        public FcmService(IWebHostEnvironment env, IConfiguration config)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var relativePath = config["Firebase:ServiceAccountPath"];
                var fullPath = Path.Combine(env.ContentRootPath, relativePath);

                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(fullPath)
                });
            }
        }
        public async Task SendMulticast(List<string> tokens, string title, string body)
        {
            Console.WriteLine("🔥 Total Tokens: " + tokens.Count);

            foreach (var token in tokens)
            {
                try
                {
                    var message = new Message()
                    {
                        Token = token,
                        Notification = new Notification
                        {
                            Title = title,
                            Body = body
                        },
                        Data = new Dictionary<string, string>
                {
                    { "type", "flash_sale" },
                    { "title", title },
                    { "body", body }
                }
                    };

                    var result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                    Console.WriteLine($"✅ Sent to: {token}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed Token: {token}");
                    Console.WriteLine(ex.Message);
                }
            }
        }
        public async Task SendNotification(string token, string title, string body)
        {
            var message = new Message()
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }

            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }
}