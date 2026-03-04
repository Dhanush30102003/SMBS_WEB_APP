using System.ComponentModel.DataAnnotations;

namespace SMBS_SERVER.Models.Masters
{
    public class ProductMaster
    {
        [Key]
        public int ProductID { get; set; }
        public int TenantID { get; set; }
        public string ProductCode { get; set; }
        public string SKU { get; set; }   // ✅ ADD THIS

        public string ProductName { get; set; }
        public string Barcode { get; set; }
        public string Description { get; set; }
         
        public int? CategoryID { get; set; }
        public int? SubCategoryID { get; set; }
        public int? BrandID { get; set; }
        public int? UOMID { get; set; }
        public int CreatedBy { get; set; }
        public byte[]? ProductImage { get; set; }
        public int ReorderLevel { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal MRP { get; set; }
        public decimal DiscountPercentage { get; set; }

        public bool IsPerishable { get; set; }
        public bool IsSerialized { get; set; }
        public bool IsTaxable { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

    }
}
