namespace SMBS_SERVER.Models.DTOs
{
    public class ProductSyncDto
    {
        public int ProductID { get; set; }
        public int TenantID { get; set; }

        public string? ProductCode { get; set; }
        public string? SKU { get; set; }
        public string? ProductName { get; set; }
        public string? Barcode { get; set; }
        public string? Description { get; set; }

        public int? CategoryID { get; set; }
        public int? SubCategoryID { get; set; }
        public int? BrandID { get; set; }
        public int? UOMID { get; set; }

        public int ReorderLevel { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }

        public bool IsPerishable { get; set; }
        public bool IsSerialized { get; set; }
        public bool IsTaxable { get; set; }

        public bool IsActive { get; set; }
    }
}
