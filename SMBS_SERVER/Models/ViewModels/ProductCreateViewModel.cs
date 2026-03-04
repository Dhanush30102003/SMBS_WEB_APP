using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace SMBS_SERVER.Models.ViewModels
{
    public class ProductCreateViewModel
    {
        [Required]
        public string ProductCode { get; set; }

        [Required]
        public string SKU { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string Barcode { get; set; }
        public string Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int? CategoryID { get; set; }
        public int? UOMID { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Please select a UOM")]
       
        public int ProductID { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal MRP { get; set; }
        public decimal DiscountPercentage { get; set; }
        public IFormFile? ImageFile { get; set; }
        public byte[]? ExistingImage { get; set; }

        public int ReorderLevel { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }

        public bool IsPerishable { get; set; }
       
        public bool IsSerialized { get; set; }
        public bool IsTaxable { get; set; }

        [ValidateNever]
        public List<SelectListItem> Categories { get; set; }

        [ValidateNever]
        public List<SelectListItem> UOMs { get; set; }
    }
}
