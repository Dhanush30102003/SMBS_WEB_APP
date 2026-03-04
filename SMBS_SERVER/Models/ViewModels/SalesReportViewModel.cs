namespace SMBS_SERVER.Models.ViewModels
{
    public class SalesReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public decimal TotalSales { get; set; } = 0;
        public int TotalInvoices { get; set; } = 0;
        public decimal AvgInvoiceValue { get; set; } = 0;
        public decimal TotalPending { get; set; } = 0;
    }
}
