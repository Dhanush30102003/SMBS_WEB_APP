using Microsoft.Extensions.Hosting;
using SMBS_SERVER.Services;

public class FlashSaleBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FlashSaleBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Background Service Running at: " + DateTime.Now);
            using var scope = _scopeFactory.CreateScope();

            var flashRepo = scope.ServiceProvider.GetRequiredService<FlashSaleRepository>();
            var customerService = scope.ServiceProvider.GetRequiredService<CustomerService>();
            var fcmService = scope.ServiceProvider.GetRequiredService<FcmService>();

            var pendingSales = flashRepo.GetPendingSales();

            foreach (var sale in pendingSales)
            {
                Console.WriteLine("Checking Sale: " + sale.Title);
                Console.WriteLine("StartTime: " + sale.StartTime);
                Console.WriteLine("EndTime: " + sale.EndTime);
                Console.WriteLine("Now: " + DateTime.Now);

                if (sale.StartTime <= DateTime.Now
                    && sale.EndTime >= DateTime.Now
                    && !sale.IsNotified)
                {
                    Console.WriteLine("🔥 Sending Flash Sale Notification...");

                    var customers = customerService.GetAllWithToken(sale.TenantID);

                    var tokens = customers
                        .Where(c => !string.IsNullOrEmpty(c.FCMToken))
                        .Select(c => c.FCMToken)
                          .Distinct()
                        .ToList();

                    Console.WriteLine("🔥 Total Tokens: " + tokens.Count);

                    if (tokens.Any())
                    {
                        await fcmService.SendMulticast(
                            tokens,
                            sale.Title,
                            sale.Description
                        );

                        flashRepo.MarkAsNotified(sale.FlashSaleID);
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}