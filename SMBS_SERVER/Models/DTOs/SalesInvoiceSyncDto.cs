
namespace SMBS_SERVER.Models.DTOs
{
    public class SalesInvoiceSyncDto
    {
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public TimeSpan InvoiceTime { get; set; }

        public int TenantID { get; set; }
        public int BranchID { get; set; }
        public int CashierUserID { get; set; }
        public int POSTerminalID { get; set; }
        public int? ShiftTypeID { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal RoundOff { get; set; }
        public int? PromotionID { get; set; }
        public int LoyaltyPointsEarned { get; set; }
        public int LoyaltyPointsRedeemed { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        public string PaymentStatus { get; set; } // Paid / Partial / Pending
        public string? Remarks { get; set; }
        public int? CustomerID { get; set; }
        public string CustomerName { get; set; }

        public string? CustomerPhone { get; set; }

    }
}