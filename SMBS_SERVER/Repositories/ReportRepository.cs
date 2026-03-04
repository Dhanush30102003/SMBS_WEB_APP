using MySql.Data.MySqlClient;
using SMBS_SERVER.Models.ViewModels;
using System.Data;

namespace SMBS_SERVER.Repositories
{
    public class ReportRepository
    {
        private readonly string _conStr;

        public ReportRepository(IConfiguration config)
        {
            _conStr = config.GetConnectionString("DefaultConnection");
        }

        private MySqlConnection GetConnection()
            => new MySqlConnection(_conStr);

        public SalesReportViewModel GetSalesSummary(DateTime from, DateTime to)
        {
            var vm = new SalesReportViewModel
            {
                FromDate = from,
                ToDate = to
            };

            using var con = GetConnection();
            con.Open();

            string sql = @"
                SELECT
                    IFNULL(SUM(TotalAmount),0) AS TotalSales,
                    COUNT(*) AS TotalInvoices,
                    IFNULL(AVG(TotalAmount),0) AS AvgInvoice,
                    IFNULL(SUM(BalanceAmount),0) AS Pending
                FROM salesinvoice
                WHERE InvoiceDate BETWEEN @from AND @to
                  AND InvoiceStatus = 'Active'";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);

            using var rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                vm.TotalSales = rd.GetDecimal("TotalSales");
                vm.TotalInvoices = rd.GetInt32("TotalInvoices");
                vm.AvgInvoiceValue = rd.GetDecimal("AvgInvoice");
                vm.TotalPending = rd.GetDecimal("Pending");
            }

            return vm;
        }
        public DataTable GetSalesInvoices(DateTime from, DateTime to)
        {
            var dt = new DataTable();

            using var con = GetConnection();
            con.Open();

            string sql = @"
        SELECT 
            InvoiceNumber,
            InvoiceDate,
            CustomerName,
            CustomerPhone,
            TotalAmount,
            PaidAmount,
            BalanceAmount,
            PaymentStatus
        FROM salesinvoice
        WHERE InvoiceDate BETWEEN @from AND @to
          AND InvoiceStatus = 'Active'
        ORDER BY InvoiceDate";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);

            using var da = new MySqlDataAdapter(cmd);
            da.Fill(dt);

            return dt;
        }

    }
}
