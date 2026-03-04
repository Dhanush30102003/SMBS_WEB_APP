namespace SMBS_SERVER.Models.DTOs
{
    public class SalesInvoiceDetailSyncDto
    {
        public string InvoiceNumber { get; set; }
        public int TenantID { get; set; }

        public string ProductCode { get; set; }
        public int? VariantID { get; set; }

        public decimal Quantity { get; set; }
        public int UOMID { get; set; }
        public decimal MRP { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        

        public decimal LineTotal { get; set; }

        // Optional but useful
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? SerialNumber { get; set; }
        public string Remarks { get; set; }
        

    }
}
