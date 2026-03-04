using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;

public class PayPalService
{
    private readonly string clientId = "AZHZRlxyd6nUcrtr9nk_eVoNMq4tpFpPLjTLONQBK40nFfTVqrUmgm6k0tiwuGoEaxTA3rQvjWQElBzt";
    private readonly string secret = "ECJ3izBg-Zw0Ef_rZR3CrsTVurP5mN8dgRUHrZILgxlA_BvT3faDmlZiIOZjIle-QJtJfnAOCfvAQFO5";

    private PayPalHttpClient GetClient()
    {
        var environment = new SandboxEnvironment(clientId, secret);
        return new PayPalHttpClient(environment);
    }

    public async Task<object> CreateOrder(decimal amount)
    {
        var request = new OrdersCreateRequest();
        request.Prefer("return=representation");

        request.RequestBody(new OrderRequest()
        {
            CheckoutPaymentIntent = "CAPTURE",

            ApplicationContext = new ApplicationContext
            {
                ReturnUrl = "http://localhost:5217/api/payment/success",
                CancelUrl = "http://localhost:5217/api/payment/cancel",
                BrandName = "SMBS Store",
                LandingPage = "LOGIN",
                UserAction = "PAY_NOW"
            },

            PurchaseUnits = new List<PurchaseUnitRequest>
        {
            new PurchaseUnitRequest
            {
                AmountWithBreakdown = new AmountWithBreakdown
                {
                    CurrencyCode = "USD",
                    Value = amount.ToString("F2")
                }
            }
        }
        });

        var response = await GetClient().Execute(request);
        var result = response.Result<Order>();

        var approveLink = result.Links
            .FirstOrDefault(x => x.Rel == "approve")?.Href;

        return new
        {
            orderId = result.Id,
            approveUrl = approveLink
        };
    }

    public async Task<Order> CaptureOrder(string orderId)
    {
        var request = new OrdersCaptureRequest(orderId);
        request.RequestBody(new OrderActionRequest());

        var response = await GetClient().Execute(request);
        return response.Result<Order>();
    }
}