using Dapper;
using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.Transactions;
using SMBS_SERVER.Models.ViewModels;

namespace SMBS_SERVER.Repositories
{
    public class StockRepository
    {
        private readonly IConfiguration _config;

        public StockRepository(IConfiguration config)
        {
            _config = config;
        }

        private MySqlConnection GetConnection()
            => new MySqlConnection(_config.GetConnectionString("DefaultConnection"));

        public List<InventoryListViewModel> GetInventory(int tenantId, int branchId)
        {
            using var con = GetConnection();
            string sql = @"
SELECT 
    s.StockID,
    s.ProductID,
    p.ProductCode,
    p.ProductName,
    s.CurrentStock,
    s.ReservedStock,
    s.AvailableStock,
    COALESCE(pm.SellingPrice, 0) AS UnitPrice
FROM stockmaster s
INNER JOIN productmaster p 
    ON p.ProductID = s.ProductID
LEFT JOIN pricemaster pm 
    ON pm.ProductID = s.ProductID
    AND pm.BranchID = s.BranchID
    AND pm.TenantID = s.TenantID
    AND pm.IsActive = 1
WHERE s.TenantID = @TenantID
  AND s.BranchID = @BranchID
ORDER BY p.ProductName;
";

            return con.Query<InventoryListViewModel>(
                sql,
                new { TenantID = tenantId, BranchID = branchId }
            ).ToList();
        }
        // ✅ GET STOCK BY PRODUCT
        public StockMaster GetByProduct(int productId, int branchId, int tenantId)
        {
            using var con = GetConnection();

            return con.QueryFirstOrDefault<StockMaster>(@"SELECT * FROM stockmaster
                  WHERE ProductID=@productId
                    AND BranchID=@branchId
                    AND TenantID=@tenantId",
                new { productId, branchId, tenantId });
        }

        // ✅ UPDATE STOCK
        public void Update(StockMaster stock)
        {
            using var con = GetConnection();

            con.Execute(@"
UPDATE stockmaster
SET 
    CurrentStock = @CurrentStock,
    ReservedStock = @ReservedStock,
    AvailableStock = @AvailableStock,
    LastUpdatedDate = NOW()
WHERE StockID = @StockID", stock);
        }
        public void Insert(StockMaster stock)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"
        INSERT INTO stockmaster
        (TenantID, ProductID, BranchID,
         CurrentStock, ReservedStock, AvailableStock,
         LastUpdatedDate)
        VALUES
        (@TenantID, @ProductID, @BranchID,
         @CurrentStock, @ReservedStock, @AvailableStock,
         NOW())";

            using var cmd = new MySqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@TenantID", stock.TenantID);
            cmd.Parameters.AddWithValue("@ProductID", stock.ProductID);
            cmd.Parameters.AddWithValue("@BranchID", stock.BranchID);
            cmd.Parameters.AddWithValue("@CurrentStock", stock.CurrentStock);
            cmd.Parameters.AddWithValue("@ReservedStock", stock.ReservedStock);
            cmd.Parameters.AddWithValue("@AvailableStock", stock.AvailableStock);

            cmd.ExecuteNonQuery();
        }

    }
}

