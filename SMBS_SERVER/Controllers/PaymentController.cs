using Microsoft.AspNetCore.Mvc;
using SMBS_SERVER.Models.DTOs;
using SMBS_SERVER.Repositories;
[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly PayPalService _service;
    private readonly SalesInvoiceRepository _invoiceRepo;

    public PaymentController(
        PayPalService service,
        SalesInvoiceRepository invoiceRepo)
    {
        _service = service;
        _invoiceRepo = invoiceRepo;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] PaymentRequest request)
    {
        var result = await _service.CreateOrder(request.Amount);
        return Ok(result);
    }

    [HttpPost("capture/{orderId}")]
    public async Task<IActionResult> Capture(string orderId, [FromQuery] int invoiceId)
    {
        Console.WriteLine($"CAPTURE CALLED → InvoiceID: {invoiceId}");
        var order = await _service.CaptureOrder(orderId);

        var capture = order.PurchaseUnits
            .First()
            .Payments
            .Captures
            .First();

        decimal paidAmount = Convert.ToDecimal(capture.Amount.Value);
        string transactionId = capture.Id;

        Console.WriteLine($"Updating Invoice: {invoiceId}");

        _invoiceRepo.MarkAsPaid(invoiceId, paidAmount, transactionId);

        return Ok(new
        {
            status = "Success",
            transactionId = transactionId
        });
    }
}