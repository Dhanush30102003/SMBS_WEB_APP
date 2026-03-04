namespace SMBS_SERVER.Models.Transactions
{
    public class StockMaster
    {
        public int StockID { get; set; }
        public int TenantID { get; set; }
        public int ProductID { get; set; }
        public int BranchID { get; set; }

        public decimal CurrentStock { get; set; }
        public decimal ReservedStock { get; set; }
        public decimal AvailableStock { get; set; }

        public DateTime LastUpdatedDate { get; set; }
    }
}
