using SMBS_SERVER.Models.ViewModels;
using SMBS_SERVER.Repositories;
using System.Data;

namespace SMBS_SERVER.Services
{
    public class ReportService
    {
        private readonly ReportRepository _repo;

        public ReportService(ReportRepository repo)
        {
            _repo = repo;
        }

        public SalesReportViewModel GetSalesReport(DateTime from, DateTime to)
        {
            return _repo.GetSalesSummary(from, to);
        }
        public DataTable GetSalesInvoices(DateTime from, DateTime to)
        {
            return _repo.GetSalesInvoices(from, to);
        }

    }
}
