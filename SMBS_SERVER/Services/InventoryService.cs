using SMBS_SERVER.Models.Transactions;
using SMBS_SERVER.Models.ViewModels;
using SMBS_SERVER.Repositories;

namespace SMBS_SERVER.Services
{
    public class InventoryService
    {
        private readonly StockRepository _stockRepo;

        public InventoryService(StockRepository stockRepo)
        {
            _stockRepo = stockRepo;
        }

        public List<InventoryListViewModel> GetInventory(int tenantId, int branchId)
        {
            return _stockRepo.GetInventory(tenantId, branchId);
        }

        // ✅ ADD STOCK
        public void AddStock(int productId, int branchId, decimal quantity, string reason)
        {
            var stock = _stockRepo.GetByProduct(productId, branchId, tenantId: 1);

            if (stock == null)
                throw new Exception("Stock not found");

            stock.CurrentStock += quantity;
            stock.AvailableStock = stock.CurrentStock - stock.ReservedStock;

            _stockRepo.Update(stock);
        }

        // ✅ REDUCE STOCK
        public void ReduceStock(int productId, int branchId, decimal quantity, string reason)
        {
            var stock = _stockRepo.GetByProduct(productId, branchId, tenantId: 1);

            if (stock == null)
                throw new Exception("Stock not found");

            if (stock.CurrentStock < quantity)
                throw new Exception("Insufficient stock");

            stock.CurrentStock -= quantity;
            stock.AvailableStock = stock.CurrentStock - stock.ReservedStock;

            _stockRepo.Update(stock);
        }
    }
}
