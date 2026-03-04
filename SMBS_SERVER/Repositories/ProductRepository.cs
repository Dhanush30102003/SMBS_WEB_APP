using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.Masters;
using Microsoft.Extensions.Logging;


namespace SMBS_SERVER.Repositories
{
    public class ProductRepository
    {
        private readonly IConfiguration _config;


        public ProductRepository(IConfiguration config)
        {
            _config = config;
        }

        private MySqlConnection GetConnection()
            => new MySqlConnection(_config.GetConnectionString("DefaultConnection"));

        public List<ProductMaster> GetAllProducts(int tenantId)
        {
            var list = new List<ProductMaster>();

            using var con = GetConnection();
            con.Open();
            string sql = @"SELECT ProductID, ProductCode, ProductName, Barcode, ProductImage
               FROM productmaster
               WHERE TenantID=@TenantID AND IsActive=1";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@TenantID", tenantId);

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new ProductMaster
                {
                    ProductID = rd.GetInt32("ProductID"),
                    ProductCode = rd.GetString("ProductCode"),
                    ProductName = rd.GetString("ProductName"),
                    Barcode = rd["Barcode"]?.ToString(),
                     ProductImage = rd["ProductImage"] == DBNull.Value
                   ? null
                   : (byte[])rd["ProductImage"]
                });
            }

            return list;
        }
        public ProductMaster GetById(int id)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"
    SELECT p.*, pr.SellingPrice, pr.MRP, pr.DiscountPercentage
    FROM productmaster p
    LEFT JOIN pricemaster pr ON p.ProductID = pr.ProductID
    WHERE p.ProductID = @Id
    LIMIT 1";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Id", id);

            using var rd = cmd.ExecuteReader();

            if (rd.Read())
            {
                return new ProductMaster
                {
                    ProductID = rd.GetInt32("ProductID"),
                    ProductCode = rd.GetString("ProductCode"),
                    SKU = rd["SKU"]?.ToString(),
                    ProductName = rd.GetString("ProductName"),
                    Barcode = rd["Barcode"]?.ToString(),
                    Description = rd["Description"]?.ToString(),
                    CategoryID = rd["CategoryID"] as int?,
                    UOMID = rd["UOMID"] as int?,
                    ProductImage = rd["ProductImage"] == DBNull.Value
                        ? null
                        : (byte[])rd["ProductImage"],

                    SellingPrice = rd["SellingPrice"] == DBNull.Value ? 0 : rd.GetDecimal("SellingPrice"),
                    MRP = rd["MRP"] == DBNull.Value ? 0 : rd.GetDecimal("MRP"),
                    DiscountPercentage = rd["DiscountPercentage"] == DBNull.Value ? 0 : rd.GetDecimal("DiscountPercentage")
                };
            }

            return null;
        }
        public void Update(ProductMaster product, decimal sellingPrice, decimal mrp, decimal discount)
        {
            using var con = GetConnection();
            con.Open();

            var transaction = con.BeginTransaction();

            try
            {
                string updateProduct = @"
        UPDATE productmaster SET
            ProductCode=@ProductCode,
            SKU=@SKU,
            ProductName=@ProductName,
            Barcode=@Barcode,
            Description=@Description,
            CategoryID=@CategoryID,
            UOMID=@UOMID,
            ProductImage=IFNULL(@ProductImage, ProductImage),
            ModifiedDate=@ModifiedDate
        WHERE ProductID=@ProductID";

                using var cmd = new MySqlCommand(updateProduct, con, transaction);

                cmd.Parameters.AddWithValue("@ProductID", product.ProductID);
                cmd.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                cmd.Parameters.AddWithValue("@SKU", product.SKU);
                cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                cmd.Parameters.AddWithValue("@Barcode", product.Barcode);
                cmd.Parameters.AddWithValue("@Description", product.Description);
                cmd.Parameters.AddWithValue("@CategoryID", product.CategoryID);
                cmd.Parameters.AddWithValue("@UOMID", product.UOMID);
                cmd.Parameters.Add("@ProductImage", MySqlDbType.LongBlob)
                    .Value = product.ProductImage ?? (object)DBNull.Value;
                cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                cmd.ExecuteNonQuery();

                string updatePrice = @"
        UPDATE pricemaster SET
            SellingPrice=@SellingPrice,
            MRP=@MRP,
            DiscountPercentage=@Discount
        WHERE ProductID=@ProductID";

                using var cmd2 = new MySqlCommand(updatePrice, con, transaction);

                cmd2.Parameters.AddWithValue("@ProductID", product.ProductID);
                cmd2.Parameters.AddWithValue("@SellingPrice", sellingPrice);
                cmd2.Parameters.AddWithValue("@MRP", mrp);
                cmd2.Parameters.AddWithValue("@Discount", discount);

                cmd2.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public int GetProductIdByCode(string productCode, int tenantId)
        {
            using var conn = GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(@"
        SELECT ProductID
        FROM productmaster
        WHERE ProductCode = @Code
          AND TenantID = @TenantID
        LIMIT 1
    ", conn);

            cmd.Parameters.AddWithValue("@Code", productCode);
            cmd.Parameters.AddWithValue("@TenantID", tenantId);

            var result = cmd.ExecuteScalar();

            if (result == null)
                return 0;

            return Convert.ToInt32(result);
        }
        public ProductMaster GetByBarcode(string barcode)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"
       SELECT 
    p.ProductID,
    p.ProductCode,
    p.ProductName,
    p.Barcode,
    p.IsActive,
    pr.SellingPrice,
    pr.MRP,
    pr.DiscountPercentage
FROM productmaster p
INNER JOIN pricemaster pr 
    ON p.ProductID = pr.ProductID
WHERE TRIM(p.Barcode) = TRIM(@Barcode)
AND p.IsActive = 1
AND pr.IsActive = 1
LIMIT 1";


            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Barcode", barcode);

            using var rd = cmd.ExecuteReader();

            if (rd.Read())
            {
                return new ProductMaster
                {
                    ProductID = rd.GetInt32("ProductID"),
                    ProductCode = rd.GetString("ProductCode"),
                    ProductName = rd.GetString("ProductName"),
                    Barcode = rd["Barcode"]?.ToString(),
                    IsActive = rd.GetBoolean("IsActive"),

                    // 🔥 ADD THESE FIELDS TO MODEL
                    SellingPrice = rd.GetDecimal("SellingPrice"),
                    MRP = rd.GetDecimal("MRP"),
                    DiscountPercentage = rd.GetDecimal("DiscountPercentage")
                };
            }

            return null;
        }



        public void Insert(ProductMaster product)
        {
            using var con = GetConnection();
            con.Open();

            string sql = @"
INSERT INTO productmaster
(
    TenantID,
    ProductCode,
    SKU,
    ProductName,
    Barcode,
    Description,
    CategoryID,
    UOMID,
 ProductImage,  
    ReorderLevel,
    MinStockLevel,
    MaxStockLevel,
    IsPerishable,
    IsSerialized,
    IsTaxable,
    IsActive,
    CreatedBy,
    CreatedDate,
    ModifiedBy,
    ModifiedDate
)
VALUES
(
    @TenantID,
    @ProductCode,
    @SKU,
    @ProductName,
    @Barcode,
    @Description,
    @CategoryID,
    @UOMID,
 @ProductImage, 
    @ReorderLevel,
    @MinStockLevel,
    @MaxStockLevel,
    @IsPerishable,
    @IsSerialized,
    @IsTaxable,
    @IsActive,
    @CreatedBy,
    @CreatedDate,
    @ModifiedBy,
    @ModifiedDate
)";
            try
            {
                using var cmd = new MySqlCommand(sql, con);

                cmd.Parameters.AddWithValue("@TenantID", product.TenantID);
                cmd.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                cmd.Parameters.AddWithValue("@SKU", product.SKU);
                cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                cmd.Parameters.AddWithValue("@Barcode", product.Barcode);
                cmd.Parameters.AddWithValue("@Description", product.Description);

                cmd.Parameters.AddWithValue("@CategoryID", product.CategoryID);
                cmd.Parameters.AddWithValue("@UOMID", product.UOMID);
                cmd.Parameters.Add("@ProductImage", MySqlDbType.LongBlob)
                 .Value = product.ProductImage ?? (object)DBNull.Value;

                cmd.Parameters.AddWithValue("@ReorderLevel", product.ReorderLevel);
                cmd.Parameters.AddWithValue("@MinStockLevel", product.MinStockLevel);
                cmd.Parameters.AddWithValue("@MaxStockLevel", product.MaxStockLevel);

                cmd.Parameters.AddWithValue("@IsPerishable", product.IsPerishable);
                cmd.Parameters.AddWithValue("@IsSerialized", product.IsSerialized);
                cmd.Parameters.AddWithValue("@IsTaxable", product.IsTaxable);
                cmd.Parameters.AddWithValue("@IsActive", product.IsActive);

                cmd.Parameters.AddWithValue("@CreatedBy", 1);
                cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@ModifiedBy", 1);
                cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                int rows = cmd.ExecuteNonQuery();
                Console.WriteLine($"ROWS INSERTED = {rows}");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"MYSQL ERROR {ex.Number}: {ex.Message}");
                throw;
            }
        }

    }
}
