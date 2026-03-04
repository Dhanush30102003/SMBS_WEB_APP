using SMBS_SERVER.Models.Masters;

public interface IProductService
{
    ProductMaster GetByBarcode(string barcode);
}
