namespace SMBS_SERVER.Models.Masters
{
    public class FlashSaleMaster
    {
        public int FlashSaleID { get; set; }
        public int TenantID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsNotified { get; set; }
        public bool IsActive { get; set; }
    }
}