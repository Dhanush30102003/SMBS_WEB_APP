namespace SMBS_SERVER.Models.ViewModels
{
    public class InventoryListViewModel
    {
        public int StockID { get; set; }

        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }

        public decimal CurrentStock { get; set; }
        public decimal ReservedStock { get; set; }
        public decimal AvailableStock { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal StockValue => CurrentStock * UnitPrice;
    }
}
