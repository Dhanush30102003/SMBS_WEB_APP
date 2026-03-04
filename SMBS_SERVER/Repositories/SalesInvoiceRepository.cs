using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using SMBS_SERVER.Models;
using SMBS_SERVER.Models.Transactions;

namespace SMBS_SERVER.Repositories
{
    public class SalesInvoiceRepository
    {
        private readonly string _connectionString;

        public SalesInvoiceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new Exception("Database connection string is EMPTY");

            }
        }
        // 🔹 Check if invoice already exists (IDEMPOTENT SYNC)
        public bool ExistsByInvoiceNumber(string invoiceNumber, int tenantId)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"
                SELECT COUNT(*) 
                FROM SalesInvoice 
                WHERE InvoiceNumber = @InvoiceNumber 
                  AND TenantID = @TenantID
            ", conn);

            cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
            cmd.Parameters.AddWithValue("@TenantID", tenantId);

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
        public string GenerateInvoiceNumber()
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var year = DateTime.Now.Year;

            var cmd = new MySqlCommand(@"
        SELECT COUNT(*) 
        FROM salesinvoice 
        WHERE YEAR(InvoiceDate) = @Year
    ", conn);

            cmd.Parameters.AddWithValue("@Year", year);

            int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;

            return $"INV-MOB-{DateTime.Now:yyyyMMddHHmmssfff}";
        }
        public void InsertDetail(
    int invoiceId,
    int productId,
    decimal qty,
    decimal price)
        {

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"
        INSERT INTO salesinvoicedetails
        (
            InvoiceID,
            ProductID,
            Quantity,
            UnitPrice,
            LineTotal,
            IsActive,
            IsSynced,
            CreatedDate
        )
        VALUES
        (
            @InvoiceID,
            @ProductID,
            @Quantity,
            @UnitPrice,
            @LineTotal,
            1,
            0,
            NOW()
        )
    ", conn);

            cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
            cmd.Parameters.AddWithValue("@ProductID", productId);
            cmd.Parameters.AddWithValue("@Quantity", qty);
            cmd.Parameters.AddWithValue("@UnitPrice", price);
            cmd.Parameters.AddWithValue("@LineTotal", qty * price);

            cmd.ExecuteNonQuery();

        }
        public int GetInvoiceIdByNumber(string invoiceNumber, int tenantId)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"
        SELECT InvoiceID
        FROM salesinvoice
        WHERE InvoiceNumber = @InvoiceNumber
          AND TenantID = @TenantID
        LIMIT 1
    ", conn);

            cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
            cmd.Parameters.AddWithValue("@TenantID", tenantId);

            var result = cmd.ExecuteScalar();

            if (result == null)
                throw new Exception($"Invoice not found: {invoiceNumber}");

            return Convert.ToInt32(result);
        }

        public List<SalesInvoice> GetByCustomerId(int customerId)
        {
            var list = new List<SalesInvoice>();

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"
        SELECT *
        FROM salesinvoice
        WHERE CustomerID = @CustomerID
        ORDER BY InvoiceDate DESC
    ", conn);

            cmd.Parameters.AddWithValue("@CustomerID", customerId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new SalesInvoice
                {
                    InvoiceID = reader["InvoiceID"] != DBNull.Value ? Convert.ToInt32(reader["InvoiceID"]) : 0,
                    TenantID = reader["TenantID"] != DBNull.Value ? Convert.ToInt32(reader["TenantID"]) : 0,
                    BranchID = reader["BranchID"] != DBNull.Value ? Convert.ToInt32(reader["BranchID"]) : 0,

                    InvoiceNumber = reader["InvoiceNumber"]?.ToString(),

                    InvoiceDate = reader["InvoiceDate"] != DBNull.Value
        ? Convert.ToDateTime(reader["InvoiceDate"])
        : DateTime.MinValue,

                    InvoiceTime = reader["InvoiceTime"] != DBNull.Value
        ? (TimeSpan)reader["InvoiceTime"]
        : TimeSpan.Zero,

                    CustomerID = reader["CustomerID"] != DBNull.Value
        ? Convert.ToInt32(reader["CustomerID"])
        : null,

                    CustomerName = reader["CustomerName"]?.ToString(),
                    CustomerPhone = reader["CustomerPhone"]?.ToString(),

                    CashierUserID = reader["CashierUserID"] != DBNull.Value ? Convert.ToInt32(reader["CashierUserID"]) : 0,
                    POSTerminalID = reader["POSTerminalID"] != DBNull.Value ? Convert.ToInt32(reader["POSTerminalID"]) : 0,
                    ShiftTypeID = reader["ShiftTypeID"] != DBNull.Value ? Convert.ToInt32(reader["ShiftTypeID"]) : 0,

                    Subtotal = reader["Subtotal"] != DBNull.Value ? Convert.ToDecimal(reader["Subtotal"]) : 0,
                    DiscountPercentage = reader["DiscountPercentage"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountPercentage"]) : 0,
                    TotalTaxAmount = reader["TotalTaxAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalTaxAmount"]) : 0,
                    RoundOff = reader["RoundOff"] != DBNull.Value ? Convert.ToDecimal(reader["RoundOff"]) : 0,

                    PromotionID = reader["PromotionID"] != DBNull.Value
        ? Convert.ToInt32(reader["PromotionID"])
        : null,

                    LoyaltyPointsEarned = reader["LoyaltyPointsEarned"] != DBNull.Value ? Convert.ToInt32(reader["LoyaltyPointsEarned"]) : 0,
                    LoyaltyPointsRedeemed = reader["LoyaltyPointsRedeemed"] != DBNull.Value ? Convert.ToInt32(reader["LoyaltyPointsRedeemed"]) : 0,

                    TotalDiscountAmount = reader["TotalDiscountAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalDiscountAmount"]) : 0,
                    TotalAmount = reader["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalAmount"]) : 0,
                    PaidAmount = reader["PaidAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PaidAmount"]) : 0,
                    BalanceAmount = reader["BalanceAmount"] != DBNull.Value ? Convert.ToDecimal(reader["BalanceAmount"]) : 0,

                    PaymentStatus = reader["PaymentStatus"]?.ToString(),

                    IsPaused = reader["IsPaused"] != DBNull.Value && Convert.ToBoolean(reader["IsPaused"]),
                    IsCompleted = reader["IsCompleted"] != DBNull.Value && Convert.ToBoolean(reader["IsCompleted"]),

                    InvoiceStatus = reader["InvoiceStatus"]?.ToString(),
                    Remarks = reader["Remarks"]?.ToString(),

                    CreatedDate = reader["CreatedDate"] != DBNull.Value
        ? Convert.ToDateTime(reader["CreatedDate"])
        : DateTime.MinValue,

                    ModifiedDate = reader["ModifiedDate"] != DBNull.Value
        ? Convert.ToDateTime(reader["ModifiedDate"])
        : null
                });

            }


            return list;
        }
        public void MarkAsPaid(int invoiceId, decimal paidAmount, string referenceId)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"
        UPDATE salesinvoice
        SET 
            PaymentStatus = 'Paid',
            PaymentReference = @ReferenceId,
            PaidAmount = @PaidAmount,
            BalanceAmount = 0,
            IsCompleted = 1,
            ModifiedDate = NOW()
        WHERE InvoiceID = @InvoiceID
    ", conn);

            cmd.Parameters.AddWithValue("@InvoiceID", invoiceId);
            cmd.Parameters.AddWithValue("@PaidAmount", paidAmount);
            cmd.Parameters.AddWithValue("@ReferenceId", referenceId);

            int rows=cmd.ExecuteNonQuery();
            Console.WriteLine($"Rows affected: {rows}");
        }

        // 🔹 Insert Sales Invoice (HEADER ONLY)
        public void Insert(SalesInvoice invoice)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                var cmd = new MySqlCommand(@"
        INSERT INTO salesinvoice
        (
            TenantID, BranchID,
            InvoiceNumber, InvoiceDate, InvoiceTime,
            CustomerID, CustomerName, CustomerPhone,
            CashierUserID, POSTerminalID, ShiftTypeID,
            Subtotal, DiscountPercentage,
            TotalTaxAmount, TotalDiscountAmount,
            RoundOff, TotalAmount,
            PaidAmount, BalanceAmount,
            PaymentStatus,
            PromotionID,
            LoyaltyPointsEarned,
            LoyaltyPointsRedeemed,
            InvoiceStatus,
            IsPaused, IsCompleted,
            Remarks, CreatedDate
        )
        VALUES
        (
            @TenantID, @BranchID,
            @InvoiceNumber, @InvoiceDate, @InvoiceTime,
            @CustomerID, @CustomerName, @CustomerPhone,
            @CashierUserID, @POSTerminalID, @ShiftTypeID,
            @Subtotal, @DiscountPercentage,
            @TotalTaxAmount, @TotalDiscountAmount,
            @RoundOff, @TotalAmount,
            @PaidAmount, @BalanceAmount,
            @PaymentStatus,
            @PromotionID,
            @LoyaltyPointsEarned,
            @LoyaltyPointsRedeemed,
            'Active',
            0, 0,
            @Remarks, NOW()
        )
        ", conn, transaction);

                cmd.Parameters.AddWithValue("@TenantID", invoice.TenantID);
                cmd.Parameters.AddWithValue("@BranchID", invoice.BranchID);
                cmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
                cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
                cmd.Parameters.AddWithValue("@InvoiceTime", invoice.InvoiceTime);
                cmd.Parameters.AddWithValue("@CustomerID", invoice.CustomerID);
                cmd.Parameters.AddWithValue("@CustomerName", invoice.CustomerName ?? "");
                cmd.Parameters.AddWithValue("@CustomerPhone", invoice.CustomerPhone ?? "");
                cmd.Parameters.AddWithValue("@CashierUserID", invoice.CashierUserID);
                cmd.Parameters.AddWithValue("@POSTerminalID", invoice.POSTerminalID);
                cmd.Parameters.AddWithValue("@ShiftTypeID", invoice.ShiftTypeID);
                cmd.Parameters.AddWithValue("@Subtotal", invoice.Subtotal);
                cmd.Parameters.AddWithValue("@DiscountPercentage", invoice.DiscountPercentage);
                cmd.Parameters.AddWithValue("@TotalTaxAmount", invoice.TotalTaxAmount);
                cmd.Parameters.AddWithValue("@TotalDiscountAmount", invoice.TotalDiscountAmount);
                cmd.Parameters.AddWithValue("@RoundOff", invoice.RoundOff);
                cmd.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);
                cmd.Parameters.AddWithValue("@PaidAmount", invoice.PaidAmount);
                cmd.Parameters.AddWithValue("@BalanceAmount", invoice.BalanceAmount);
                cmd.Parameters.AddWithValue("@PaymentStatus", invoice.PaymentStatus);
                cmd.Parameters.AddWithValue("@PromotionID", invoice.PromotionID);
                cmd.Parameters.AddWithValue("@LoyaltyPointsEarned", invoice.LoyaltyPointsEarned);
                cmd.Parameters.AddWithValue("@LoyaltyPointsRedeemed", invoice.LoyaltyPointsRedeemed);
                cmd.Parameters.AddWithValue("@Remarks", invoice.Remarks ?? "");

                cmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch (MySqlException ex)
            {
                transaction.Rollback();

                if (ex.Number == 1062) // Duplicate entry
                {
                    throw new Exception("Invoice number already exists. Please retry.");
                }

                throw;
            }
        }

    }
}
