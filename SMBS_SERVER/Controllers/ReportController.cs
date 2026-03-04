using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SMBS_SERVER.Services;
using System.ComponentModel;
using SMBS_SERVER.Models.ViewModels;
using OfficeOpenXml.Drawing.Chart;
using System.Data;

[Authorize]
public class ReportsController : Controller
{
    private readonly ReportService _service;

    public ReportsController(ReportService service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        var model = new SalesReportViewModel(); // all zero
        return View(model);
    }


    [HttpPost]
    public IActionResult LoadReport(DateTime fromDate, DateTime toDate)
    {
        var model = _service.GetSalesReport(fromDate, toDate);
        model.FromDate = fromDate;
        model.ToDate = toDate;

        return View("Index", model);
    }

    [HttpPost]
    public IActionResult Export(DateTime fromDate, DateTime toDate)
    {
        var summary = _service.GetSalesReport(fromDate, toDate);
        var invoices = _service.GetSalesInvoices(fromDate, toDate);

        using var package = new ExcelPackage();

        // ==========================
        // SHEET 1: SUMMARY
        // ==========================
        var summarySheet = package.Workbook.Worksheets.Add("Summary");

        summarySheet.Cells["A1"].Value = "Sales Report";
        summarySheet.Cells["A3"].Value = "From Date";
        summarySheet.Cells["B3"].Value = fromDate.ToShortDateString();
        summarySheet.Cells["A4"].Value = "To Date";
        summarySheet.Cells["B4"].Value = toDate.ToShortDateString();

        summarySheet.Cells["A6"].Value = "Total Sales";
        summarySheet.Cells["B6"].Value = summary.TotalSales;

        summarySheet.Cells["A7"].Value = "Total Invoices";
        summarySheet.Cells["B7"].Value = summary.TotalInvoices;

        summarySheet.Cells["A8"].Value = "Avg Invoice Value";
        summarySheet.Cells["B8"].Value = summary.AvgInvoiceValue;

        summarySheet.Cells["A9"].Value = "Pending Amount";
        summarySheet.Cells["B9"].Value = summary.TotalPending;

        summarySheet.Cells["A6:A9"].Style.Font.Bold = true;
        summarySheet.Cells["B6:B9"].Style.Numberformat.Format = "₹#,##0.00";

        // ==========================
        // DAILY SALES DATA (FOR CHART)
        // ==========================
        summarySheet.Cells["D6"].Value = "Date";
        summarySheet.Cells["E6"].Value = "Sales";

        var daily = invoices.AsEnumerable()
            .GroupBy(r => Convert.ToDateTime(r["InvoiceDate"]).Date)
            .Select(g => new
            {
                Date = g.Key,
                Total = g.Sum(x => Convert.ToDecimal(x["TotalAmount"]))
            })
            .OrderBy(x => x.Date)
            .ToList();

        int row = 7;
        foreach (var d in daily)
        {
            summarySheet.Cells[row, 4].Value = d.Date.ToShortDateString();
            summarySheet.Cells[row, 5].Value = d.Total;
            row++;
        }

        // ==========================
        // CHART
        // ==========================
        var chart = summarySheet.Drawings.AddChart("SalesChart", eChartType.ColumnClustered);
        chart.Title.Text = "Daily Sales";
        chart.SetPosition(2, 0, 6, 0);
        chart.SetSize(700, 350);

        var xRange = summarySheet.Cells[$"D7:D{row - 1}"];
        var yRange = summarySheet.Cells[$"E7:E{row - 1}"];

        chart.Series.Add(yRange, xRange);

        // ==========================
        // SHEET 2: INVOICES
        // ==========================
        var invoiceSheet = package.Workbook.Worksheets.Add("Invoices");

        invoiceSheet.Cells["A1"].LoadFromDataTable(invoices, true);
        invoiceSheet.Cells.AutoFitColumns();

        invoiceSheet.Cells["E:E"].Style.Numberformat.Format = "₹#,##0.00";
        invoiceSheet.Cells["F:F"].Style.Numberformat.Format = "₹#,##0.00";
        invoiceSheet.Cells["G:G"].Style.Numberformat.Format = "₹#,##0.00";

        var bytes = package.GetAsByteArray();

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"SalesReport_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx"
        );
    }

}
