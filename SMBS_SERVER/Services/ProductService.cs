using SMBS_SERVER.Models.Masters;
using SMBS_SERVER.Repositories;

namespace SMBS_SERVER.Services
{
    public class ProductService : IProductService
    {
        private readonly ProductRepository _repo;

        public ProductService(ProductRepository repo)
        {
            _repo = repo;
        }

        public List<ProductMaster> GetAllProducts(int tenantId)
        {
            return _repo.GetAllProducts(tenantId);
        }
        public ProductMaster GetByBarcode(string barcode)
        {
            return _repo.GetByBarcode(barcode);
        }

        public void AddProduct(ProductMaster product)
        {
            _repo.Insert(product);
        }
        public ProductMaster GetById(int id)
        {
            return _repo.GetById(id);
        }

        public void UpdateProduct(ProductMaster product,
            decimal sellingPrice,
            decimal mrp,
            decimal discount)
        {
            _repo.Update(product, sellingPrice, mrp, discount);
        }

    }
}
