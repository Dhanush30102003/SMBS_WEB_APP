using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.Transactions;
using System.Data;


namespace SMBS_SERVER.Controllers
{
    [Authorize(Roles = "Admin")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IConfiguration _configuration;

        public DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            List<SalesInvoice> invoices = new();
            string connStr = _configuration.GetConnectionString("DefaultConnection");

            using MySqlConnection con = new(connStr);
            con.Open();

            string query = @"SELECT InvoiceID, InvoiceNumber, InvoiceDate,
                             CustomerName, Subtotal, TotalAmount,
                             PaidAmount, BalanceAmount,
                             PaymentStatus, InvoiceStatus
                             FROM salesinvoice
                             ORDER BY CreatedDate DESC";

            using MySqlCommand cmd = new(query, con);
            using MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                invoices.Add(new SalesInvoice
                {
                    InvoiceID = reader.GetInt32("InvoiceID"),

                    InvoiceNumber = reader.IsDBNull("InvoiceNumber")
        ? string.Empty
        : reader.GetString("InvoiceNumber"),

                    InvoiceDate = reader.IsDBNull("InvoiceDate")
        ? DateTime.MinValue
        : reader.GetDateTime("InvoiceDate"),

                    CustomerName = reader.IsDBNull("CustomerName")
        ? "Walk-in"
        : reader.GetString("CustomerName"),

                    Subtotal = reader.IsDBNull("Subtotal")
        ? 0
        : reader.GetDecimal("Subtotal"),

                    TotalAmount = reader.IsDBNull("TotalAmount")
        ? 0
        : reader.GetDecimal("TotalAmount"),

                    PaidAmount = reader.IsDBNull("PaidAmount")
        ? 0
        : reader.GetDecimal("PaidAmount"),

                    BalanceAmount = reader.IsDBNull("BalanceAmount")
        ? 0
        : reader.GetDecimal("BalanceAmount"),

                    PaymentStatus = reader.IsDBNull("PaymentStatus")
        ? "Pending"
        : reader.GetString("PaymentStatus"),

                    InvoiceStatus = reader.IsDBNull("InvoiceStatus")
        ? "Active"
        : reader.GetString("InvoiceStatus")
                });
            }

            return View(invoices);
        }
    }
}
