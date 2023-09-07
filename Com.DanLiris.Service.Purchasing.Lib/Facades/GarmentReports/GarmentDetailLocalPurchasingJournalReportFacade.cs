using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class GarmentDetailLocalPurchasingJournalReportFacade  : IGarmentDetailLocalPurchasingJournalReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<UnitReceiptNote> dbSet;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IdentityService _identityService;
        private readonly string IDRCurrencyCode = "IDR";
     
        public static readonly string[] MONTH_NAMES = { "JANUARI", "FEBRUARI", "MARET", "APRIL", "MEI", "JUNI", "JULI", "AGUSTUS", "SEPTEMBER", "OKTOBER", "NOVEMBER", "DESEMBER" };

        public GarmentDetailLocalPurchasingJournalReportFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<UnitReceiptNote>();
            _currencyProvider = (ICurrencyProvider)serviceProvider.GetService(typeof(ICurrencyProvider));
            _identityService = serviceProvider.GetService<IdentityService>();
        }

        //public List<GarmentLocalPurchasingJournalReportViewModel> GetReportQuery(int month, int year, int offset)
        public List<GarmentDetailLocalPurchasingJournalReportViewModel> GetReportQuery(DateTime? dateFrom, DateTime? dateTo, int offset)
        {

            //DateTime dateFrom = new DateTime(year, month, 1);
            //int nextYear = month == 12 ? year + 1 : year;
            //int nextMonth = month == 12 ? 1 : month + 1;
            //DateTime dateTo = new DateTime(nextYear, nextMonth, 1);

            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            List<GarmentDetailLocalPurchasingJournalReportViewModel> data = new List<GarmentDetailLocalPurchasingJournalReportViewModel>();

            var Query = (from a in dbContext.GarmentUnitReceiptNotes
                         join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                         join e in dbContext.GarmentDeliveryOrderDetails on b.DODetailId equals e.Id
                         join d in dbContext.GarmentDeliveryOrderItems on e.GarmentDOItemId equals d.Id
                         join c in dbContext.GarmentDeliveryOrders on d.GarmentDOId equals c.Id                         
                         where a.URNType == "PEMBELIAN" && c.SupplierIsImport == false
                               && (c.PaymentType == "T/T AFTER" || c.PaymentType == "T/T BEFORE")
                               && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date 
                         group new { Price = b.PricePerDealUnit, Qty = b.ReceiptQuantity, Rate = c.DOCurrencyRate } by new 
                         { a.URNNo, a.ReceiptDate, a.SupplierCode, a.SupplierName, a.Remark, c.InternNo, c.BillNo, e.CodeRequirment, c.PaymentType, c.DOCurrencyCode, c.DOCurrencyRate, c.UseVat, c.VatRate, c.UseIncomeTax, c.IncomeTaxRate } into G

                         select new GarmentDetailLocalPurchasingJournalReportViewModel
                         {
                             urnno = G.Key.URNNo,
                             urndate = G.Key.ReceiptDate,
                             supplier = G.Key.SupplierCode + "-" + G.Key.SupplierName,
                             remark = G.Key.Remark,
                             inno = G.Key.InternNo == null ? "-" : G.Key.InternNo,
                             billno = G.Key.BillNo == null ? "-" : G.Key.BillNo,
                             code = G.Key.CodeRequirment,
                             paymentyype = G.Key.PaymentType,
                             currencycode = G.Key.DOCurrencyCode,
                             rate = G.Key.DOCurrencyRate,
                             isvat = G.Key.UseVat == true ? "Y" : "N",
                             vatrate = (double)G.Key.VatRate,
                             istax = G.Key.UseIncomeTax == true ? "Y" : "N",
                             taxrate = (double)G.Key.IncomeTaxRate,
                             amount = Math.Round(G.Sum(c => c.Price * c.Qty * (decimal)c.Rate), 2),
                             coaname = "-",
                             account = "-",
                             debit = 0,
                             credit = 0,
                         });
            //
            foreach (GarmentDetailLocalPurchasingJournalReportViewModel x in Query.OrderBy(x => x.urnno))
            {
                var debit1 = new GarmentDetailLocalPurchasingJournalReportViewModel
                {
                    urnno = x.urnno,
                    urndate = x.urndate,
                    supplier = x.supplier,
                    remark = x.remark,
                    inno = x.inno,
                    billno = x.billno,
                    code = x.code,
                    paymentyype = x.paymentyype,
                    currencycode = x.currencycode,
                    rate = x.rate,
                    isvat = x.isvat,
                    vatrate = x.vatrate,
                    istax = x.istax,
                    taxrate = x.taxrate,
                    amount = x.amount,
                    coaname = x.code == "BB" ? "PERSEDIAAN BAHAN BAKU(AG2)" : (x.code == "BP" ? "PERSEDIAAN PEMBANTU(AG2)" : "PERSEDIAAN EMBALANCE(AG2)"),
                    credit = 0,
                    debit = x.amount,
                    account = x.code == "BB" ? "114.03.2.000" : (x.code == "BP" ? "114.04.2.000" : "114.05.2.000")                
                };
                data.Add(debit1);

                var debit2 = new GarmentDetailLocalPurchasingJournalReportViewModel
                {
                    urnno = x.urnno,
                    urndate = x.urndate,
                    supplier = x.supplier,
                    remark = x.remark,
                    inno = x.inno,
                    billno = x.billno,
                    code = x.code,
                    paymentyype = x.paymentyype,
                    currencycode = x.currencycode,
                    rate = x.rate,
                    isvat = x.isvat,
                    vatrate = x.vatrate,
                    istax = x.istax,
                    taxrate = x.taxrate,
                    amount = x.amount,
                    coaname = "PPN MASUKAN (AG2)",
                    debit =  x.amount * Convert.ToDecimal((x.vatrate / 100)),
                    credit = 0,
                    account = "117.01.2.000"
                };
                if (debit2.debit > 0)
                {
                    data.Add(debit2);
                }

                var kredit = new GarmentDetailLocalPurchasingJournalReportViewModel
                {
                    urnno = x.urnno,
                    urndate = x.urndate,
                    supplier = x.supplier,
                    remark = x.remark,
                    inno = x.inno,
                    billno = x.billno,
                    code = x.code,
                    paymentyype = x.paymentyype,
                    currencycode = x.currencycode,
                    rate = x.rate,
                    isvat = x.isvat,
                    vatrate = x.vatrate,
                    istax = x.istax,
                    taxrate = x.taxrate,
                    amount = x.amount,
                    coaname = "       HUTANG USAHA LOKAL(AG2)",
                    debit = 0,
                    credit = debit2.debit > 0 ? x.amount + debit2.debit : x.amount,                   
                    account = "211.00.2.000"
                };

                data.Add(kredit);                
            }

            var total = new GarmentDetailLocalPurchasingJournalReportViewModel
            {
                remark = "",                
                debit = Query.Sum(a => a.amount) + (Query.Sum(a => a.amount * Convert.ToDecimal((a.vatrate / 100)))),
                credit = Query.Sum(a => a.amount) + (Query.Sum(a => a.amount * Convert.ToDecimal((a.vatrate / 100)))),
                account = "J U M L A H"
            };
            if (total.debit > 0)
            {
                data.Add(total);
            }
            else
            {
                var totalx = new GarmentDetailLocalPurchasingJournalReportViewModel
                {
                    remark = "",
                    debit = 0,
                    credit = 0,
                    account = "J U M L A H"
                };
                data.Add(totalx);
            }
            return data;
        }

        //public List<GarmentDetailLocalPurchasingJournalReportViewModel> GetReportData(DateTime? dateFrom, DateTime? dateTo, int offset)
        //{
        //    var Query = GetReportQuery(dateFrom, dateTo, offset);
        //    return Query.ToList();
        //}

        public MemoryStream GenerateExcel(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            var Query = GetReportQuery(dateFrom, dateTo, offset);
            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "NO BON", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "TGL BON", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "NO AKUN ", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA AKUN", DataType = typeof(string) });

            result.Columns.Add(new DataColumn() { ColumnName = "SUPPLIER", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "KETERANGAN", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "NO NOTA INTERN", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "NO BILL", DataType = typeof(string) });

            result.Columns.Add(new DataColumn() { ColumnName = "MATA UANG", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "KURS", DataType = typeof(string) });            
            result.Columns.Add(new DataColumn() { ColumnName = "DEBET", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "KREDIT", DataType = typeof(string) });

            ExcelPackage package = new ExcelPackage();

            //if (Query.ToArray().Count() == 0)
            //    result.Rows.Add("", "", "", "");
            //else

            if (Query.ToArray().Count() == 0)
            {
                result.Rows.Add("", "", 0, 0);
                bool styling = true;

                foreach (KeyValuePair<DataTable, String> item in new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") })
                {
                    var sheet = package.Workbook.Worksheets.Add(item.Value);

                    //string Bln = MONTH_NAMES[month - 1];
                    sheet.Column(1).Width = 15;
                    sheet.Column(2).Width = 15;
                    sheet.Column(3).Width = 15;
                    sheet.Column(4).Width = 50;

                    sheet.Column(5).Width = 30;
                    sheet.Column(6).Width = 50;
                    sheet.Column(7).Width = 20;
                    sheet.Column(8).Width = 20;

                    sheet.Column(9).Width = 15;
                    sheet.Column(10).Width = 15;
                    sheet.Column(11).Width = 20;
                    sheet.Column(12).Width = 20;


                    #region KopTable
                    sheet.Cells[$"A1:D1"].Value = "PT DanLiris GARMINDO";
                    sheet.Cells[$"A1:D1"].Merge = true;
                    sheet.Cells[$"A1:D1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet.Cells[$"A1:D1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A1:D1"].Style.Font.Bold = true;

                    sheet.Cells[$"A2:D2"].Value = "ACCOUNTING DEPT.";
                    sheet.Cells[$"A2:D2"].Merge = true;
                    sheet.Cells[$"A2:D2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet.Cells[$"A2:D2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A2:D2"].Style.Font.Bold = true;

                    sheet.Cells[$"A4:D4"].Value = "RINCIAN JURNAL";
                    sheet.Cells[$"A4:D4"].Merge = true;
                    sheet.Cells[$"A4:D4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells[$"A4:D4"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A4:D4"].Style.Font.Bold = true;

                    sheet.Cells[$"C5"].Value = "BUKU HARIAN";
                    sheet.Cells[$"C5"].Style.Font.Bold = true;
                    sheet.Cells[$"D5"].Value = ": PEMBELIAN LOKAL";
                    sheet.Cells[$"D5"].Style.Font.Bold = true;

                    sheet.Cells[$"C6"].Value = "PERIODE";
                    sheet.Cells[$"C6"].Style.Font.Bold = true;
                    sheet.Cells[$"D6"].Value = ": " + DateFrom.ToString("dd-MM-yyyy") + " S/D " + DateTo.ToString("dd-MM-yyyy");
                    sheet.Cells[$"D6"].Style.Font.Bold = true;

                    #endregion
                    sheet.Cells["A8"].LoadFromDataTable(item.Key, true, (styling == true) ? OfficeOpenXml.Table.TableStyles.Light16 : OfficeOpenXml.Table.TableStyles.None);

                    //sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
                }
            }
            else
            {
                int index = 0;
                foreach (var d in Query)
                {
                    index++;

                    result.Rows.Add(d.urnno, d.urndate, d.account, d.coaname, d.supplier, d.remark, d.inno, d.billno, d.currencycode, d.rate, d.debit, d.credit);
                }
                
                bool styling = true;

                foreach (KeyValuePair<DataTable, String> item in new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") })
                {
                    var sheet = package.Workbook.Worksheets.Add(item.Value);
                    //string Bln = MONTH_NAMES[month - 1];
                    sheet.Column(1).Width = 15;
                    sheet.Column(2).Width = 15;
                    sheet.Column(3).Width = 15;
                    sheet.Column(4).Width = 50;

                    sheet.Column(5).Width = 30;
                    sheet.Column(6).Width = 50;
                    sheet.Column(7).Width = 20;
                    sheet.Column(8).Width = 20;

                    sheet.Column(9).Width = 15;
                    sheet.Column(10).Width = 15;
                    sheet.Column(11).Width = 20;
                    sheet.Column(12).Width = 20;

                    #region KopTable
                    sheet.Cells[$"A1:D1"].Value = "PT DanLiris GARMINDO";
                    sheet.Cells[$"A1:D1"].Merge = true;
                    sheet.Cells[$"A1:D1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet.Cells[$"A1:D1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A1:D1"].Style.Font.Bold = true;

                    sheet.Cells[$"A2:D2"].Value = "ACCOUNTING DEPT.";
                    sheet.Cells[$"A2:D2"].Merge = true;
                    sheet.Cells[$"A2:D2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet.Cells[$"A2:D2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A2:D2"].Style.Font.Bold = true;

                    sheet.Cells[$"A4:D4"].Value = "RINCIAN JURNAL";
                    sheet.Cells[$"A4:D4"].Merge = true;
                    sheet.Cells[$"A4:D4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells[$"A4:D4"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A4:D4"].Style.Font.Bold = true;

                    sheet.Cells[$"C5"].Value = "BUKU HARIAN";
                    sheet.Cells[$"C5"].Style.Font.Bold = true;
                    sheet.Cells[$"D5"].Value = ": PEMBELIAN LOKAL";
                    sheet.Cells[$"D5"].Style.Font.Bold = true;

                    sheet.Cells[$"C6"].Value = "PERIODE";
                    sheet.Cells[$"C6"].Style.Font.Bold = true;
                    sheet.Cells[$"D6"].Value = ": " + DateFrom.ToString("dd-MM-yyyy") + " S/D " + DateTo.ToString("dd-MM-yyyy");
                    sheet.Cells[$"D6"].Style.Font.Bold = true;

                    #endregion
                    sheet.Cells["A8"].LoadFromDataTable(item.Key, true, (styling == true) ? OfficeOpenXml.Table.TableStyles.Light16 : OfficeOpenXml.Table.TableStyles.None);

                    //sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
                }
            }

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return stream;
            }   
    }
}