using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMBS_SERVER.Models.DTOs;
using SMBS_SERVER.Models.Transactions;
using SMBS_SERVER.Repositories;


namespace SMBS_SERVER.Controllers.Api
{
    [Authorize(Roles = "Customer")]
    [Route("api/salesinvoice")]
    [ApiController]
    public class SalesInvoiceApiController : ControllerBase
    {
        private readonly SalesInvoiceRepository _repo;
        private readonly CustomerRepository _customerRepo;

        public SalesInvoiceApiController(SalesInvoiceRepository repo, CustomerRepository customerRepo)
        {
            _repo = repo;
            _customerRepo = customerRepo;
        }


        [Authorize(Roles = "Customer")]
        [HttpPost("create")]
        public IActionResult Create([FromBody] CreateInvoiceRequest request)
        {
            try
            {
                var customerId = int.Parse(User.FindFirst("CustomerID").Value);
             

                var customerInfo = _customerRepo.GetCustomerBasicInfo(customerId);


                var invoice = new SalesInvoice
                {
                    TenantID = 1,
                    BranchID = 1,

                    InvoiceNumber = _repo.GenerateInvoiceNumber(),
                    InvoiceDate = DateTime.Now.Date,
                    InvoiceTime = DateTime.Now.TimeOfDay,

                    // 🔥 IMPORTANT FIX
                    CustomerID = customerId,
                    CustomerName = customerInfo.Name,
                    CustomerPhone = customerInfo.Phone,

                    CashierUserID = 1,
                    POSTerminalID = 1,
                    ShiftTypeID = 1,

                    Subtotal = request.TotalAmount,
                    DiscountPercentage = 0,
                    TotalTaxAmount = 0,
                    TotalDiscountAmount = 0,
                    RoundOff = 0,

                    PromotionID = null,
                    LoyaltyPointsEarned = 0,
                    LoyaltyPointsRedeemed = 0,

                    TotalAmount = request.TotalAmount,
                    PaidAmount = 0,
                    BalanceAmount = request.TotalAmount,

                    PaymentStatus = "Pending",
                    InvoiceStatus = "Active",

                    IsPaused = false,
                    IsCompleted = false,

                    Remarks = "",
                    CreatedDate = DateTime.Now
                };

                _repo.Insert(invoice);

                int invoiceId = _repo.GetInvoiceIdByNumber(
                    invoice.InvoiceNumber,
                    invoice.TenantID
                );
                // 🔥 INSERT PRODUCTS
                foreach (var item in request.Items)
                {
                    Console.WriteLine($"Product: {item.ProductId}, Qty: {item.Qty}");
                    _repo.InsertDetail(
                        invoiceId,
                        item.ProductId,
                        item.Qty,
                        item.Price
                    );
                }

                return Ok(new { invoiceId = invoiceId });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already exists"))
                    return Conflict(new { message = ex.Message });

                return BadRequest(ex.Message);

            }
        }
    }
}