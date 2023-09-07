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
    public class GarmentImportPurchasingJournalReportFacade : IGarmentImportPurchasingJournalReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<UnitReceiptNote> dbSet;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IdentityService _identityService;
        private readonly string IDRCurrencyCode = "IDR";

        public static readonly string[] MONTH_NAMES = { "JANUARI", "FEBRUARI", "MARET", "APRIL", "MEI", "JUNI", "JULI", "AGUSTUS", "SEPTEMBER", "OKTOBER", "NOVEMBER", "DESEMBER" };

        public GarmentImportPurchasingJournalReportFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<UnitReceiptNote>();
            _currencyProvider = (ICurrencyProvider)serviceProvider.GetService(typeof(ICurrencyProvider));
            _identityService = serviceProvider.GetService<IdentityService>();
        }


        //public List<GarmentLocalPurchasingJournalReportViewModel> GetReportQuery(int month, int year, int offset)
        public List<GarmentImportPurchasingJournalReportViewModel> GetReportQuery(DateTime? dateFrom, DateTime? dateTo, int offset)
        {

            //DateTime dateFrom = new DateTime(year, month, 1);
            //int nextYear = month == 12 ? year + 1 : year;
            //int nextMonth = month == 12 ? 1 : month + 1;
            //DateTime dateTo = new DateTime(nextYear, nextMonth, 1);

            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            List<GarmentImportPurchasingJournalReportViewModel> data = new List<GarmentImportPurchasingJournalReportViewModel>();

            var Querya = (from a in dbContext.GarmentUnitReceiptNotes
                         join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                         join e in dbContext.GarmentDeliveryOrderDetails on b.DODetailId equals e.Id
                         join d in dbContext.GarmentDeliveryOrderItems on e.GarmentDOItemId equals d.Id
                         join c in dbContext.GarmentDeliveryOrders on d.GarmentDOId equals c.Id
                         where a.URNType == "PEMBELIAN" && c.SupplierIsImport == true
                               && (c.PaymentType == "T/T AFTER" || c.PaymentType == "T/T BEFORE")
                               && c.DOCurrencyCode != "IDR" && a.UnitCode != "SMP1" && e.CodeRequirment == "BB"
                               && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
                         group new { Price = b.PricePerDealUnit, Qty = b.ReceiptQuantity, Rate = c.DOCurrencyRate } by new { a.UnitCode, c.UseVat, c.VatRate } into G

                         select new GarmentImportPurchasingJournalReportTempViewModel
                         {
                             UnitCode = G.Key.UnitCode,
                             //Code = G.Key.CodeRequirment,
                             //PaymentType = G.Key.PaymentType,
                             IsVat = G.Key.UseVat == true ? "Y" : "N",
                             VatRate = (double)G.Key.VatRate,
                             //IsTax = G.Key.UseIncomeTax == true ? "Y" : "N",
                             //TaxRate = (double)G.Key.IncomeTaxRate,
                             Amount = Math.Round(G.Sum(c => c.Price * c.Qty * (decimal)c.Rate), 2)
                         });

            var Query1 = (from x in Querya
                          group new { Amt = x.Amount } by new { x.UnitCode } into G

                          select new GarmentImportPurchasingJournalReportTemp1ViewModel
                          {
                              UnitCode = G.Key.UnitCode,
                              Amount = Math.Round(G.Sum(c => c.Amt), 2)
                          });

            var NewQuery1 = from a in Query1
                           select new GarmentImportPurchasingJournalReportViewModel
                           {

                               //remark = a.Code == "BB" && a.UnitCode == "C1A" ? "PERSEDIAAN BHN BAKU - K1A" : (a.Code == "BB" && a.UnitCode == "C1B" ? "PERSEDIAAN BHN BAKU - K1B" : (a.Code == "BB" && a.UnitCode == "C2A" ? "PERSEDIAAN BHN BAKU - K2A" : (a.Code == "BB" && a.UnitCode == "C2B" ? "PERSEDIAAN BHN BAKU - K2B" : "PERSEDIAAN BHN BAKU - K2C"))),
                               remark = a.UnitCode == "C1A" ? "PERSEDIAAN BHN BAKU - K1A" : (a.UnitCode == "C1B" ? "PERSEDIAAN BHN BAKU - K1B" : (a.UnitCode == "C2A" ? "PERSEDIAAN BHN BAKU - K2A" : (a.UnitCode == "C2B" ? "PERSEDIAAN BHN BAKU - K2B" : "PERSEDIAAN BHN BAKU - K2C"))),
                               credit = 0,
                               debit = a.Amount,
                               account = a.UnitCode == "C1A" ? "11507.41" : (a.UnitCode == "C1B" ? "11507.42" : (a.UnitCode == "C2A" ? "11507.43" : (a.UnitCode == "C2B" ? "11507.44" : "11507.45")))
                           };
            //
            var Queryb = (from a in dbContext.GarmentUnitReceiptNotes
                          join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                          join e in dbContext.GarmentDeliveryOrderDetails on b.DODetailId equals e.Id
                          join d in dbContext.GarmentDeliveryOrderItems on e.GarmentDOItemId equals d.Id
                          join c in dbContext.GarmentDeliveryOrders on d.GarmentDOId equals c.Id
                          where a.URNType == "PEMBELIAN" && c.SupplierIsImport == true
                                && (c.PaymentType == "T/T AFTER" || c.PaymentType == "T/T BEFORE")
                                && c.DOCurrencyCode != "IDR" && a.UnitCode != "SMP1" && e.CodeRequirment == "BP"
                                && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
                          group new { Price = b.PricePerDealUnit, Qty = b.ReceiptQuantity, Rate = c.DOCurrencyRate } by new { a.UnitCode, c.UseVat, c.VatRate } into G

                          select new GarmentImportPurchasingJournalReportTempViewModel
                          {
                              UnitCode = G.Key.UnitCode,
                              //Code = G.Key.CodeRequirment,
                              //PaymentType = G.Key.PaymentType,
                              IsVat = G.Key.UseVat == true ? "Y" : "N",
                              VatRate = (double)G.Key.VatRate,
                              //IsTax = G.Key.UseIncomeTax == true ? "Y" : "N",
                              //TaxRate = (double)G.Key.IncomeTaxRate,
                              Amount = Math.Round(G.Sum(c => c.Price * c.Qty * (decimal)c.Rate), 2)
                          });

            var Query2 = (from x in Queryb
                          group new { Amt = x.Amount } by new { x.UnitCode } into G

                          select new GarmentImportPurchasingJournalReportTemp1ViewModel
                          {
                              UnitCode = G.Key.UnitCode,
                              Amount = Math.Round(G.Sum(c => c.Amt), 2)
                          });

            var NewQuery2 = from a in Query2
                            select new GarmentImportPurchasingJournalReportViewModel
                            {
                                remark = a.UnitCode == "C1A" ? "PERSEDIAAN B PBT  - K1A" : (a.UnitCode == "C1B" ? "PERSEDIAAN B PBT  - K1B" : (a.UnitCode == "C2A" ? "PERSEDIAAN B PBT  - K2A" : (a.UnitCode == "C2B" ? "PERSEDIAAN B PBT  - K2B" : "PERSEDIAAN B PBT  - K2C"))),
                                credit = 0,
                                debit = a.Amount,
                                account = a.UnitCode == "C1A" ? "11521.41" : (a.UnitCode == "C1B" ? "11521.42" : (a.UnitCode == "C2A" ? "11521.43" : (a.UnitCode == "C2B" ? "11521.44" : "11521.45")))
                            };
            //

            //
            var Queryc = (from a in dbContext.GarmentUnitReceiptNotes
                          join b in dbContext.GarmentUnitReceiptNoteItems on a.Id equals b.URNId
                          join e in dbContext.GarmentDeliveryOrderDetails on b.DODetailId equals e.Id
                          join d in dbContext.GarmentDeliveryOrderItems on e.GarmentDOItemId equals d.Id
                          join c in dbContext.GarmentDeliveryOrders on d.GarmentDOId equals c.Id
                          where a.URNType == "PEMBELIAN" && c.SupplierIsImport == true
                                && (c.PaymentType == "T/T AFTER" || c.PaymentType == "T/T BEFORE")
                                && c.DOCurrencyCode != "IDR" && a.UnitCode != "SMP1" && e.CodeRequirment == "BE"
                                && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
                          group new { Price = b.PricePerDealUnit, Qty = b.ReceiptQuantity, Rate = c.DOCurrencyRate } by new { a.UnitCode, c.UseVat, c.VatRate } into G

                          select new GarmentImportPurchasingJournalReportTempViewModel
                          {
                              UnitCode = G.Key.UnitCode,
                              //Code = G.Key.CodeRequirment,
                              //PaymentType = G.Key.PaymentType,
                              IsVat = G.Key.UseVat == true ? "Y" : "N",
                              VatRate = (double)G.Key.VatRate,
                              //IsTax = G.Key.UseIncomeTax == true ? "Y" : "N",
                              //TaxRate = (double)G.Key.IncomeTaxRate,
                              Amount = Math.Round(G.Sum(c => c.Price * c.Qty * (decimal)c.Rate), 2)
                          });

            var Query3 = (from x in Queryc
                          group new { Amt = x.Amount } by new { x.UnitCode } into G

                          select new GarmentImportPurchasingJournalReportTemp1ViewModel
                          {
                              UnitCode = G.Key.UnitCode,
                              Amount = Math.Round(G.Sum(c => c.Amt), 2)
                          });

            var NewQuery3 = from a in Query3
                            select new GarmentImportPurchasingJournalReportViewModel
                            {
                                remark = a.UnitCode == "C1A" ? "PERSEDIAAN PEMBUNGKUS -K1A" : (a.UnitCode == "C1B" ? "PERSEDIAAN PEMBUNGKUS -K1B" : (a.UnitCode == "C2A" ? "PERSEDIAAN PEMBUNGKUS -K2A" : (a.UnitCode == "C2B" ? "PERSEDIAAN PEMBUNGKUS -K2B" : "PERSEDIAAN PEMBUNGKUS -K2C"))),
                                credit = 0,
                                debit = a.Amount,
                                account = a.UnitCode == "C1A" ? "11519.41" : (a.UnitCode == "C1B" ? "11519.42" : (a.UnitCode == "C2A" ? "11519.43" : (a.UnitCode == "C2B" ? "11519.44" : "11519.45")))
                            };
            //         
            List<GarmentImportPurchasingJournalReportViewModel> CombineData = NewQuery1.Union(NewQuery2).Union(NewQuery3).ToList();

            var sumquery = CombineData.ToList()
                   .GroupBy(x => new { x.remark, x.account }, (key, group) => new
                   {
                       Remark = key.remark,
                       Account = key.account,
                       Debit = group.Sum(s => s.debit)
                   }).OrderBy(a => a.Remark);
            foreach (var item in sumquery)
            {
                var result = new GarmentImportPurchasingJournalReportViewModel
                {
                    remark = item.Remark,
                    debit = item.Debit,
                    credit = 0,
                    account = item.Account
                };

                data.Add(result);
            }

            //if (NewQuery1.ToList().Count == 0)
            //{
            //    var stock1 = new GarmentImportPurchasingJournalReportViewModel
            //    {
            //        remark = "PERSEDIAAN BAHAN BAKU(AG2)",
            //        debit = 0,
            //        credit = 0,
            //        account = "114.03.2.000"
            //    };
            //    data.Add(stock1);

            //    var stock2 = new GarmentImportPurchasingJournalReportViewModel
            //    {
            //        remark = "PERSEDIAAN PEMBANTU(AG2)",
            //        debit = 0,
            //        credit = 0,
            //        account = "114.04.2.000"
            //    };
            //    data.Add(stock2);

            //    var stock3 = new GarmentImportPurchasingJournalReportViewModel
            //    {
            //        remark = "PERSEDIAAN EMBALANCE(AG2)",
            //        debit = 0,
            //        credit = 0,
            //        account = "114.05.2.000"
            //    };
            //    data.Add(stock3);
            //}

            //var PPNMsk = new GarmentImportPurchasingJournalReportViewModel
            //{
            //    remark = "PPN MASUKAN (AG2)",
            //    debit = Query.Where(a => a.IsVat == "Y").Sum(a => a.Amount * (decimal)(a.VatRate / 100)),
            //    credit = 0,
            //    account = "117.01.2.000"
            //};

            ////if (PPNMsk.debit > 0)
            ////{
            //data.Add(PPNMsk);
            //}

            //var PPH = new GarmentImportPurchasingJournalReportViewModel
            //{
            //    remark = "       PPH  23   YMH DIBAYAR(AG2)",
            //    debit = 0,
            //    credit = Query.Where(a => a.IsTax == "Y").Sum(a => a.Amount * (decimal)(a.TaxRate / 100)),
            //    account = "217.03.2.000"
            //};

            //if (PPH.credit > 0)
            //{
            //data.Add(PPH);
            //}

            //var Credit1 = new GarmentImportPurchasingJournalReportViewModel
            //{
            //    remark = "       KAS  DITANGAN VALAS (AG2)",
            //    credit = Query.Where(a => a.PaymentType == "CASH").Sum(a => a.Amount),
            //    debit = 0,
            //    account = "111.01.2.002"
            //};

            ////if (Credit1.credit > 0)
            ////{
            //data.Add(Credit1);
            ////}

            var Credit = new GarmentImportPurchasingJournalReportViewModel
            {
                remark = "        UTANG USAHA IMPOR - OPERASIONAL",
                debit = 0,
                //credit = Query1.Sum(a => a.Amount) + PPNMsk.debit - (PPH.credit + Credit1.credit),
                credit = Query1.Sum(a => a.Amount),
                //credit = Query.Where(a => a.PaymentType == "T/T AFTER" || a.PaymentType == "T/T BEFORE").Sum(a => a.Amount),
                account = "21203.01"
            };    


            if (Credit.credit > 0)
            {
                data.Add(Credit);
            }
            //else
            //{
            //    var hutang = new GarmentImportPurchasingJournalReportViewModel
            //    {
            //        remark = "       HUTANG USAHA IMPOR(AG2)",
            //        debit = 0,
            //        credit = 0,
            //        account = "211.00.3.000"
            //    };
            //    data.Add(hutang);
            //}

            var total = new GarmentImportPurchasingJournalReportViewModel
            {
                remark = "",
                //debit = Query1.Sum(a => a.Amount) + PPNMsk.debit,
                debit = Query1.Sum(a => a.Amount),
                //credit = Credit.credit + Credit1.credit + PPH.credit,
                credit = Query1.Sum(a => a.Amount),
                account = "J U M L A H"
            };
            if (total.credit > 0)
            {
                data.Add(total);
            }
            //else
            //{
            //    var jumlah = new GarmentImportPurchasingJournalReportViewModel
            //    {
            //        remark = "",
            //        debit = 0,
            //        credit = 0,
            //        account = "J U M L A H"
            //    };
            //    data.Add(jumlah);
            //}

            return data;
        }

        public List<GarmentImportPurchasingJournalReportViewModel> GetReportData(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetReportQuery(dateFrom, dateTo, offset);
            return Query.ToList();
        }

        public MemoryStream GenerateExcel(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            var Query = GetReportQuery(dateFrom, dateTo, offset);
            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "AKUN DAN KETERANGAN", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "AKUN", DataType = typeof(string) });
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

                    sheet.Column(1).Width = 50;
                    sheet.Column(2).Width = 15;
                    sheet.Column(3).Width = 20;
                    sheet.Column(4).Width = 20;

                    #region KopTable
                    sheet.Cells[$"A1:D1"].Value = "PT. DAN LIRIS";
                    sheet.Cells[$"A1:D1"].Merge = true;
                    sheet.Cells[$"A1:D1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet.Cells[$"A1:D1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A1:D1"].Style.Font.Bold = true;

                    sheet.Cells[$"A2:D2"].Value = "ACCOUNTING DEPT.";
                    sheet.Cells[$"A2:D2"].Merge = true;
                    sheet.Cells[$"A2:D2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet.Cells[$"A2:D2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A2:D2"].Style.Font.Bold = true;

                    sheet.Cells[$"A4:D4"].Value = "IKHTISAR JURNAL";
                    sheet.Cells[$"A4:D4"].Merge = true;
                    sheet.Cells[$"A4:D4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells[$"A4:D4"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A4:D4"].Style.Font.Bold = true;

                    sheet.Cells[$"C5"].Value = "BUKU HARIAN";
                    sheet.Cells[$"C5"].Style.Font.Bold = true;
                    sheet.Cells[$"D5"].Value = ": PEMBELIAN IMPORT";
                    sheet.Cells[$"D5"].Style.Font.Bold = true;

                    sheet.Cells[$"C6"].Value = "PERIODE";
                    sheet.Cells[$"C6"].Style.Font.Bold = true;
                    sheet.Cells[$"D6"].Value = ": " + DateFrom.ToString("ddMMyyyy") + " S/D " + DateTo.ToString("ddMMyyyy");
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

                    result.Rows.Add(d.remark, d.account, d.debit, d.credit);
                }

                bool styling = true;

                foreach (KeyValuePair<DataTable, String> item in new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") })
                {
                    var sheet = package.Workbook.Worksheets.Add(item.Value);
                    //string Bln = MONTH_NAMES[month - 1];

                    sheet.Column(1).Width = 50;
                    sheet.Column(2).Width = 15;
                    sheet.Column(3).Width = 20;
                    sheet.Column(4).Width = 20;

                    #region KopTable
                    sheet.Cells[$"A1:D1"].Value = "PT. DAN LIRIS";
                    sheet.Cells[$"A1:D1"].Merge = true;
                    sheet.Cells[$"A1:D1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet.Cells[$"A1:D1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A1:D1"].Style.Font.Bold = true;

                    sheet.Cells[$"A2:D2"].Value = "ACCOUNTING DEPT.";
                    sheet.Cells[$"A2:D2"].Merge = true;
                    sheet.Cells[$"A2:D2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet.Cells[$"A2:D2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A2:D2"].Style.Font.Bold = true;

                    sheet.Cells[$"A4:D4"].Value = "IKHTISAR JURNAL";
                    sheet.Cells[$"A4:D4"].Merge = true;
                    sheet.Cells[$"A4:D4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells[$"A4:D4"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A4:D4"].Style.Font.Bold = true;

                    sheet.Cells[$"C5"].Value = "BUKU HARIAN";
                    sheet.Cells[$"C5"].Style.Font.Bold = true;
                    sheet.Cells[$"D5"].Value = ": PEMBELIAN IMPORT";
                    sheet.Cells[$"D5"].Style.Font.Bold = true;

                    sheet.Cells[$"C6"].Value = "PERIODE";
                    sheet.Cells[$"C6"].Style.Font.Bold = true;
                    sheet.Cells[$"D6"].Value = ": " + DateFrom.ToString("ddMMyyyy") + " S/D " + DateTo.ToString("ddMMyyyy");
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