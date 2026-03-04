namespace SMBS_SERVER.Models.Transactions
{
    public class SalesInvoiceDetails
    {
        public int DetailID { get; set; }
        public int InvoiceID { get; set; }
        public int ProductID { get; set; }
        public int? VariantID { get; set; }

        public decimal Quantity { get; set; }
        public int UOMID { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal MRP { get; set; }

        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }

        public decimal LineTotal { get; set; }

        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? SerialNumber { get; set; }

        public string? Remarks { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsSynced { get; set; }
    }
}
