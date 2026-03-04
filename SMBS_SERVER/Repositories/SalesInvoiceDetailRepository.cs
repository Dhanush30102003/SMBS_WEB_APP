using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.Transactions;

public class SalesInvoiceDetailRepository
{
    private readonly string _connStr;

    public SalesInvoiceDetailRepository(IConfiguration cfg)
    {
        _connStr = cfg.GetConnectionString("DefaultConnection"); // ✅ FIX
    }
    public List<SalesInvoiceDetails> GetByInvoiceId(int invoiceId)
    {
        var list = new List<SalesInvoiceDetails>();

        using var conn = new MySqlConnection(_connStr);
        conn.Open();

        var cmd = new MySqlCommand(@"
        SELECT d.*, p.ProductName
        FROM salesinvoicedetails d
        LEFT JOIN productmaster p ON d.ProductID = p.ProductID
        WHERE d.InvoiceID = @InvoiceID
    ", conn);

        cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new SalesInvoiceDetails
            {
                InvoiceID = Convert.ToInt32(reader["InvoiceID"]),
                ProductID = Convert.ToInt32(reader["ProductID"]),
                Quantity = Convert.ToDecimal(reader["Quantity"]),
                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                LineTotal = Convert.ToDecimal(reader["LineTotal"]),
                Remarks = reader["ProductName"]?.ToString() // reuse for name
            });
        }

        return list;
    }

    public void Insert(SalesInvoiceDetails d)
    {
        using var conn = new MySqlConnection(_connStr);
        conn.Open();

        var cmd = new MySqlCommand(@"
INSERT INTO SalesInvoiceDetails
(
    InvoiceID, ProductID, VariantID,
    Quantity, UOMID,
    UnitPrice, MRP,
    DiscountPercentage, DiscountAmount,
    TaxPercentage, TaxAmount,
    LineTotal,
    BatchNumber, ExpiryDate, SerialNumber,
    Remarks,
    IsActive, CreatedDate, IsSynced
)
VALUES
(
    @InvoiceID, @ProductID, @VariantID,
    @Quantity, @UOMID,
    @UnitPrice, @MRP,
    @DiscountPercentage, @DiscountAmount,
    @TaxPercentage, @TaxAmount,
    @LineTotal,
    @BatchNumber, @ExpiryDate, @SerialNumber,
    @Remarks,
    @IsActive, @CreatedDate, @IsSynced
)
", conn);

        cmd.Parameters.AddWithValue("@InvoiceID", d.InvoiceID);
        cmd.Parameters.AddWithValue("@ProductID", d.ProductID);

        cmd.Parameters.AddWithValue(
            "@VariantID",
            d.VariantID == 0 ? DBNull.Value : d.VariantID
        );

        cmd.Parameters.AddWithValue("@Quantity", d.Quantity);
        cmd.Parameters.AddWithValue("@UOMID", d.UOMID);

        cmd.Parameters.AddWithValue("@UnitPrice", d.UnitPrice);
        cmd.Parameters.AddWithValue("@MRP", d.MRP);

        cmd.Parameters.AddWithValue("@DiscountPercentage", d.DiscountPercentage);
        cmd.Parameters.AddWithValue("@DiscountAmount", d.DiscountAmount);

        cmd.Parameters.AddWithValue("@TaxPercentage", d.TaxPercentage);
        cmd.Parameters.AddWithValue("@TaxAmount", d.TaxAmount);

        cmd.Parameters.AddWithValue("@LineTotal", d.LineTotal);

        cmd.Parameters.AddWithValue(
            "@BatchNumber",
            string.IsNullOrWhiteSpace(d.BatchNumber) ? DBNull.Value : d.BatchNumber
        );

        cmd.Parameters.AddWithValue(
            "@ExpiryDate",
            d.ExpiryDate.HasValue ? d.ExpiryDate.Value : DBNull.Value
        );

        cmd.Parameters.AddWithValue(
            "@SerialNumber",
            string.IsNullOrWhiteSpace(d.SerialNumber) ? DBNull.Value : d.SerialNumber
        );

        cmd.Parameters.AddWithValue("@Remarks", d.Remarks ?? "");

        cmd.Parameters.AddWithValue("@IsActive", d.IsActive);
        cmd.Parameters.AddWithValue("@CreatedDate", d.CreatedDate);
        cmd.Parameters.AddWithValue("@IsSynced", d.IsSynced);


        cmd.ExecuteNonQuery();
    }
}