using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Com.Moonlay.NetCore.Lib;
using Microsoft.Extensions.DependencyInjection;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Data;
using OfficeOpenXml;
using System.Globalization;
using OfficeOpenXml.Style;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSubcon.Report.GarmentReceiptSubconStockReport
{
    public class GarmentReceiptSubconStockReportFacade : IGarmentReceiptSubconStockReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        public GarmentReceiptSubconStockReportFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
        }

        public List<GarmentStockReportViewModel> GetStockQuery(string ctg, string unitcode, DateTime? datefrom, DateTime? dateto, int offset)
        {
            DateTime DateFrom = datefrom == null ? new DateTime(1970, 1, 1) : (DateTime)datefrom;
            DateTime DateTo = dateto == null ? DateTime.Now : (DateTime)dateto;

            string filter = (string.IsNullOrWhiteSpace(ctg) ? "{}" : "{" + "'" + "CodeRequirement" + "'" + ":" + "'" + ctg + "'" + "}");

            var categories = GetProductCategories(1, int.MaxValue, "{}", filter);
            var categories1 = categories.Select(x => x.Name).ToArray();

            List<GarmentStockReportViewModel> stock1 = new List<GarmentStockReportViewModel>();

            var SATerima = (from a in dbContext.GarmentSubconUnitReceiptNoteItems
                            join b in dbContext.GarmentSubconUnitReceiptNotes on a.URNId equals b.Id

                            where
                            a.IsDeleted == false && b.IsDeleted == false
                              //&& b.ReceiptDate.AddHours(offset).Date >= lastdate.Date
                              && b.ReceiptDate.AddHours(offset).Date < DateFrom.Date
                              && b.UnitCode == (string.IsNullOrWhiteSpace(unitcode) ? b.UnitCode : unitcode)
                              && categories1.Contains(a.ProductName)
                            select new GarmentStockReportViewModelTemp
                            {
                                BeginningBalanceQty = Math.Round(a.ReceiptQuantity * a.Conversion, 2, MidpointRounding.AwayFromZero),
                                BeginningBalanceUom = a.SmallUomUnit.Trim(),
                                Buyer = b.ProductOwnerCode,
                                EndingBalanceQty = 0,
                                EndingUom = a.SmallUomUnit.Trim(),
                                ExpandUom = a.SmallUomUnit.Trim(),
                                ExpendQty = 0,
                                NoArticle = b.Article.TrimEnd(),
                                PlanPo = a.POSerialNumber.Trim(),
                                ProductCode = a.ProductCode.Trim(),
                                ReceiptCorrectionQty = 0,
                                ReceiptQty = 0,
                                ReceiptUom = a.SmallUomUnit.Trim(),
                                RO = b.RONo
                            }).GroupBy(x => new { x.BeginningBalanceUom, x.Buyer, x.EndingUom, x.ExpandUom, x.NoArticle, x.PaymentMethod, x.PlanPo, x.ProductCode, /*x.ProductName,*/ x.ReceiptUom, x.RO }, (key, group) => new GarmentStockReportViewModelTemp
                            {
                                BeginningBalanceQty = Math.Round(group.Sum(x => x.BeginningBalanceQty), 2, MidpointRounding.AwayFromZero),
                                BeginningBalanceUom = key.BeginningBalanceUom,
                                Buyer = key.Buyer,
                                EndingBalanceQty = Math.Round(group.Sum(x => x.EndingBalanceQty), 2, MidpointRounding.AwayFromZero),
                                EndingUom = key.EndingUom,
                                ExpandUom = key.ExpandUom,
                                ExpendQty = Math.Round(group.Sum(x => x.ExpendQty), 2, MidpointRounding.AwayFromZero),
                                NoArticle = key.NoArticle,
                                PaymentMethod = key.PaymentMethod,
                                PlanPo = key.PlanPo,
                                ProductCode = key.ProductCode,

                                ReceiptCorrectionQty = Math.Round(group.Sum(x => x.ReceiptCorrectionQty), 2, MidpointRounding.AwayFromZero),
                                ReceiptQty = Math.Round(group.Sum(x => x.ReceiptQty), 2, MidpointRounding.AwayFromZero),
                                ReceiptUom = key.ReceiptUom,
                                RO = key.RO
                            });

            var SAKeluar = (from a in dbContext.GarmentSubconUnitExpenditureNoteItems
                            join b in dbContext.GarmentSubconUnitExpenditureNotes on a.UENId equals b.Id
                            join f in dbContext.GarmentSubconUnitReceiptNoteItems on a.URNItemId equals f.Id into urnitems
                            from urnitem in urnitems.DefaultIfEmpty()
                            join g in dbContext.GarmentSubconUnitReceiptNotes on urnitem.URNId equals g.Id into urns
                            from urn in urns.DefaultIfEmpty()
                            where
                            a.IsDeleted == false && b.IsDeleted == false

                               //&&
                               //b.ExpenditureDate.AddHours(offset).Date >= lastdate.Date
                               && b.ExpenditureDate.AddHours(offset).Date < DateFrom.Date
                               && b.UnitSenderCode == (string.IsNullOrWhiteSpace(unitcode) ? b.UnitSenderCode : unitcode)
                               && categories1.Contains(a.ProductName)
                            select new GarmentStockReportViewModelTemp
                            {
                                //BeginningBalanceQty = Convert.ToDecimal(a.UomUnit == "YARD" && ctg == "BB" ? a.Quantity * -1 * 0.9144 : b.ExpenditureType == "EXTERNAL" ? Convert.ToDouble(urnitem == null ? 0 : urnitem.SmallQuantity) * -1 : -1 * a.Quantity),
                                BeginningBalanceQty = a.UomUnit == "YARD" && ctg == "BB" ? Convert.ToDecimal(a.Quantity * -1 * 0.9144) : (b.ExpenditureType == "EXTERNAL" && a.UomUnit != "PCS" && ctg != "BB") ? Convert.ToDecimal(a.Quantity) * urnitem.Conversion * -1 : -1 * Convert.ToDecimal(a.Quantity),
                                BeginningBalanceUom = a.UomUnit == "YARD" && ctg == "BB" ? "MT" : b.ExpenditureType == "EXTERNAL" ? urnitem.SmallUomUnit : a.UomUnit.Trim(),
                                Buyer = urn == null ? "-" : urn.ProductOwnerCode,
                                EndingBalanceQty = 0,
                                EndingUom = a.UomUnit == "YARD" && ctg == "BB" ? "MT" : b.ExpenditureType == "EXTERNAL" ? urnitem.SmallUomUnit : a.UomUnit.Trim(),
                                ExpandUom = a.UomUnit == "YARD" && ctg == "BB" ? "MT" : b.ExpenditureType == "EXTERNAL" ? urnitem.SmallUomUnit : a.UomUnit.Trim(),
                                ExpendQty = 0,
                                NoArticle = urn == null ? "-" : urn.Article.TrimEnd(),
                                PlanPo = a.POSerialNumber.Trim(),
                                ProductCode = a.ProductCode.Trim(),
                                ReceiptCorrectionQty = 0,
                                ReceiptQty = 0,
                                ReceiptUom = a.UomUnit == "YARD" && ctg == "BB" ? "MT" : b.ExpenditureType == "EXTERNAL" ? urnitem.SmallUomUnit : a.UomUnit.Trim(),
                                RO = a.RONo
                            }).GroupBy(x => new { x.BeginningBalanceUom, x.Buyer, x.EndingUom, x.ExpandUom, x.NoArticle, x.PaymentMethod, x.PlanPo, x.ProductCode, /*x.ProductName,*/ x.ReceiptUom, x.RO }, (key, group) => new GarmentStockReportViewModelTemp
                            {
                                BeginningBalanceQty = Math.Round(group.Sum(x => x.BeginningBalanceQty), 2, MidpointRounding.AwayFromZero),
                                BeginningBalanceUom = key.BeginningBalanceUom,
                                Buyer = key.Buyer,
                                EndingBalanceQty = Math.Round(group.Sum(x => x.EndingBalanceQty), 2, MidpointRounding.AwayFromZero),
                                EndingUom = key.EndingUom,
                                ExpandUom = key.ExpandUom,
                                ExpendQty = Math.Round(group.Sum(x => x.ExpendQty), 2, MidpointRounding.AwayFromZero),
                                NoArticle = key.NoArticle,
                                PaymentMethod = key.PaymentMethod,
                                PlanPo = key.PlanPo,
                                ProductCode = key.ProductCode,
                                ReceiptCorrectionQty = Math.Round(group.Sum(x => x.ReceiptCorrectionQty), 2, MidpointRounding.AwayFromZero),
                                ReceiptQty = Math.Round(group.Sum(x => x.ReceiptQty), 2, MidpointRounding.AwayFromZero),
                                ReceiptUom = key.ReceiptUom,
                                RO = key.RO
                            });

            var SaldoAwal1 = SATerima.Concat(SAKeluar).AsEnumerable();
            var SaldoAwal12 = SaldoAwal1.GroupBy(x => new { x.BeginningBalanceUom, /*x.Buyer,*/ x.EndingUom, x.ExpandUom, x.NoArticle, x.PaymentMethod, x.PlanPo, x.ProductCode, /*x.ProductName,*/ x.ReceiptUom, x.RO }, (key, group) => new GarmentStockReportViewModelTemp
            {
                BeginningBalanceQty = Math.Round(group.Sum(x => x.BeginningBalanceQty), 2, MidpointRounding.AwayFromZero),
                BeginningBalanceUom = key.BeginningBalanceUom,
                Buyer = group.FirstOrDefault().Buyer,
                EndingBalanceQty = Math.Round(group.Sum(x => x.BeginningBalanceQty), 2, MidpointRounding.AwayFromZero),
                EndingUom = key.EndingUom,
                ExpandUom = key.ExpandUom,
                ExpendQty = Math.Round(group.Sum(x => x.ExpendQty), 2, MidpointRounding.AwayFromZero),
                NoArticle = key.NoArticle,
                PaymentMethod = key.PaymentMethod,
                PlanPo = key.PlanPo,
                ProductCode = key.ProductCode,
                ReceiptCorrectionQty = Math.Round(group.Sum(x => x.ReceiptCorrectionQty), 2, MidpointRounding.AwayFromZero),
                ReceiptQty = Math.Round(group.Sum(x => x.ReceiptQty), 2, MidpointRounding.AwayFromZero),
                ReceiptUom = key.ReceiptUom,
                RO = key.RO
            }).ToList();

            var Terima = (from a in dbContext.GarmentSubconUnitReceiptNoteItems
                          join b in dbContext.GarmentSubconUnitReceiptNotes on a.URNId equals b.Id
                          where a.IsDeleted == false && b.IsDeleted == false &&
                              b.ReceiptDate.AddHours(offset).Date >= DateFrom.Date
                              && b.ReceiptDate.AddHours(offset).Date <= DateTo.Date
                              && b.UnitCode == (string.IsNullOrWhiteSpace(unitcode) ? b.UnitCode : unitcode)
                              && categories1.Contains(a.ProductName)
                          select new GarmentStockReportViewModelTemp
                          {
                              BeginningBalanceQty = 0,
                              BeginningBalanceUom = a.SmallUomUnit.Trim(),
                              Buyer = b.ProductOwnerCode,
                              EndingBalanceQty = 0,
                              EndingUom = a.SmallUomUnit.Trim(),
                              ExpandUom = a.SmallUomUnit.Trim(),
                              ExpendQty = 0,
                              NoArticle = b.Article.TrimEnd(),
                              PlanPo = a.POSerialNumber.Trim(),
                              ProductCode = a.ProductCode.Trim(),
                              ReceiptCorrectionQty = 0,
                              ReceiptQty = Math.Round(a.ReceiptQuantity * a.Conversion, 2, MidpointRounding.AwayFromZero),
                              ReceiptUom = a.SmallUomUnit.Trim(),
                              RO = b.RONo
                          }).GroupBy(x => new { x.BeginningBalanceUom, x.Buyer, x.EndingUom, x.ExpandUom, x.NoArticle, x.PaymentMethod, x.PlanPo, x.ProductCode, /*x.ProductName,*/ x.ReceiptUom, x.RO }, (key, group) => new GarmentStockReportViewModelTemp
                          {
                              BeginningBalanceQty = Math.Round(group.Sum(x => x.BeginningBalanceQty), 2, MidpointRounding.AwayFromZero),
                              BeginningBalanceUom = key.BeginningBalanceUom,
                              Buyer = key.Buyer,
                              EndingBalanceQty = Math.Round(group.Sum(x => x.EndingBalanceQty), 2, MidpointRounding.AwayFromZero),
                              EndingUom = key.EndingUom,
                              ExpandUom = key.ExpandUom,
                              ExpendQty = Math.Round(group.Sum(x => x.ExpendQty), 2, MidpointRounding.AwayFromZero),
                              NoArticle = key.NoArticle,
                              PaymentMethod = key.PaymentMethod,
                              PlanPo = key.PlanPo,
                              ProductCode = key.ProductCode,
                              ReceiptCorrectionQty = Math.Round(group.Sum(x => x.ReceiptCorrectionQty), 2, MidpointRounding.AwayFromZero),
                              ReceiptQty = Math.Round(group.Sum(x => x.ReceiptQty), 2, MidpointRounding.AwayFromZero),
                              ReceiptUom = key.ReceiptUom,
                              RO = key.RO
                          }).ToList();

            var Keluar = (from a in dbContext.GarmentSubconUnitExpenditureNoteItems
                          join b in dbContext.GarmentSubconUnitExpenditureNotes on a.UENId equals b.Id
                          join f in dbContext.GarmentSubconUnitReceiptNoteItems on a.URNItemId equals f.Id into urnitems
                          from urnitem in urnitems.DefaultIfEmpty()
                          join g in dbContext.GarmentSubconUnitReceiptNotes on urnitem.URNId equals g.Id into urns
                          from urn in urns.DefaultIfEmpty()
                          where a.IsDeleted == false && b.IsDeleted == false &&
                               b.ExpenditureDate.AddHours(offset).Date >= DateFrom.Date
                               && b.ExpenditureDate.AddHours(offset).Date <= DateTo.Date
                               && b.UnitSenderCode == (string.IsNullOrWhiteSpace(unitcode) ? b.UnitSenderCode : unitcode)
                               && categories1.Contains(a.ProductName)
                          select new GarmentStockReportViewModelTemp
                          {
                              BeginningBalanceQty = 0,
                              BeginningBalanceUom = a.UomUnit == "YARD" && ctg == "BB" ? "MT" : b.ExpenditureType == "EXTERNAL" ? urnitem.SmallUomUnit : a.UomUnit.Trim(),
                              Buyer = urn == null ? "-" : urn.ProductOwnerCode,
                              EndingBalanceQty = 0,
                              EndingUom = a.UomUnit == "YARD" && ctg == "BB" ? "MT" : b.ExpenditureType == "EXTERNAL" ? urnitem.SmallUomUnit : a.UomUnit.Trim(),
                              ExpandUom = a.UomUnit == "YARD" && ctg == "BB" ? "MT" : b.ExpenditureType == "EXTERNAL" ? urnitem.SmallUomUnit : a.UomUnit.Trim(),
                              ExpendQty = a.UomUnit == "YARD" && ctg == "BB" ? Convert.ToDecimal(a.Quantity * 0.9144) : (b.ExpenditureType == "EXTERNAL" && a.UomUnit != "PCS" && ctg != "BB") ? Convert.ToDecimal(a.Quantity) * urnitem.Conversion : Convert.ToDecimal(a.Quantity),
                              //NoArticle = prs != null ? prs.Article.TrimEnd() : "-",
                              NoArticle = urn == null ? "-" : urn.Article.TrimEnd(),
                              PlanPo = a.POSerialNumber.Trim(),
                              ProductCode = a.ProductCode.Trim(),
                              ReceiptCorrectionQty = 0,
                              ReceiptQty = 0,
                              ReceiptUom = a.UomUnit == "YARD" && ctg == "BB" ? "MT" : a.UomUnit.Trim(),
                              RO = a.RONo
                          }).GroupBy(x => new { x.BeginningBalanceUom, x.Buyer, x.EndingUom, x.ExpandUom, x.NoArticle, x.PaymentMethod, x.PlanPo, x.ProductCode,/* x.ProductName,*/ x.ReceiptUom, x.RO }, (key, group) => new GarmentStockReportViewModelTemp
                          {
                              BeginningBalanceQty = Math.Round(group.Sum(x => x.BeginningBalanceQty), 2, MidpointRounding.AwayFromZero),
                              BeginningBalanceUom = key.BeginningBalanceUom,
                              Buyer = key.Buyer,
                              EndingBalanceQty = Math.Round(group.Sum(x => x.EndingBalanceQty), 2, MidpointRounding.AwayFromZero),
                              EndingUom = key.EndingUom,
                              ExpandUom = key.ExpandUom,
                              ExpendQty = Math.Round(group.Sum(x => x.ExpendQty), 2, MidpointRounding.AwayFromZero),
                              NoArticle = key.NoArticle,
                              PaymentMethod = key.PaymentMethod,
                              PlanPo = key.PlanPo,
                              ProductCode = key.ProductCode,
                              //ProductName = key.ProductName,
                              ReceiptCorrectionQty = Math.Round(group.Sum(x => x.ReceiptCorrectionQty), 2, MidpointRounding.AwayFromZero),
                              ReceiptQty = Math.Round(group.Sum(x => x.ReceiptQty), 2, MidpointRounding.AwayFromZero),
                              ReceiptUom = key.ReceiptUom,
                              RO = key.RO
                          }).ToList();

            var SaldoFiltered = Terima.Concat(Keluar).AsEnumerable();
            var SaldoFiltered1 = SaldoFiltered.GroupBy(x => new { x.BeginningBalanceUom, /*x.Buyer,*/ x.EndingUom, x.ExpandUom, x.NoArticle, x.PaymentMethod, x.PlanPo, x.ProductCode, /*x.ProductName,*/ x.ReceiptUom, x.RO }, (key, group) => new GarmentStockReportViewModelTemp
            {
                BeginningBalanceQty = Math.Round(group.Sum(x => x.BeginningBalanceQty), 2, MidpointRounding.AwayFromZero),
                BeginningBalanceUom = key.BeginningBalanceUom,
                Buyer = group.FirstOrDefault().Buyer,
                EndingBalanceQty = Math.Round(group.Sum(x => x.EndingBalanceQty), 2, MidpointRounding.AwayFromZero),
                EndingUom = key.EndingUom,
                ExpandUom = key.ExpandUom,
                ExpendQty = Math.Round(group.Sum(x => x.ExpendQty), 2, MidpointRounding.AwayFromZero),
                NoArticle = key.NoArticle,
                PlanPo = key.PlanPo,
                ProductCode = key.ProductCode,
                //ProductName = key.ProductName,
                ReceiptCorrectionQty = Math.Round(group.Sum(x => x.ReceiptCorrectionQty), 2, MidpointRounding.AwayFromZero),
                ReceiptQty = Math.Round(group.Sum(x => x.ReceiptQty), 2, MidpointRounding.AwayFromZero),
                ReceiptUom = key.ReceiptUom,
                RO = key.RO
            }).ToList();

            var SaldoAkhir1 = SaldoAwal12.Concat(SaldoFiltered1).AsEnumerable();
            var stock = SaldoAkhir1.GroupBy(x => new { x.BeginningBalanceUom, /*x.Buyer,*/ x.EndingUom, x.ExpandUom, x.PaymentMethod, x.PlanPo, x.ProductCode, /*x.ProductName,*/ x.ReceiptUom, x.RO, x.NoArticle }, (key, group) => new GarmentStockReportViewModelTemp
            {
                BeginningBalanceQty = Math.Round(group.Sum(x => x.BeginningBalanceQty), 2, MidpointRounding.AwayFromZero),
                BeginningBalanceUom = key.BeginningBalanceUom,
                Buyer = group.FirstOrDefault().Buyer,
                EndingBalanceQty = Math.Round(group.Sum(x => x.BeginningBalanceQty + x.ReceiptQty + x.ReceiptCorrectionQty - (decimal)x.ExpendQty), 2, MidpointRounding.AwayFromZero),
                EndingUom = key.EndingUom,
                ExpandUom = key.ExpandUom,
                ExpendQty = Math.Round(group.Sum(x => x.ExpendQty), 2, MidpointRounding.AwayFromZero),
                NoArticle = key.NoArticle,
                PlanPo = key.PlanPo,
                ProductCode = key.ProductCode,
                ReceiptCorrectionQty = Math.Round(group.Sum(x => x.ReceiptCorrectionQty), 2, MidpointRounding.AwayFromZero),
                ReceiptQty = Math.Round(group.Sum(x => x.ReceiptQty), 2, MidpointRounding.AwayFromZero),
                ReceiptUom = key.ReceiptUom,
                RO = key.RO
            }).ToList();


            var PrdoctCodes = string.Join(",", stock.Select(x => x.ProductCode).Distinct().ToList());

            var Codes = GetProductCode(PrdoctCodes);

            stock1 = (from i in stock
                      join b in Codes on i.ProductCode.Trim() equals b.Code.Trim() into produtcodes
                      from bb in produtcodes.DefaultIfEmpty()
                      select new GarmentStockReportViewModel
                      {
                          BeginningBalanceQty = i.BeginningBalanceQty,
                          BeginningBalanceUom = i.BeginningBalanceUom,
                          Buyer = i.Buyer,
                          EndingBalanceQty = i.EndingBalanceQty,
                          EndingUom = i.EndingUom,
                          ExpandUom = i.ExpandUom,
                          ExpendQty = decimal.ToDouble(i.ExpendQty),
                          NoArticle = i.NoArticle,
                          PaymentMethod = i.PaymentMethod,
                          PlanPo = i.PlanPo,
                          ProductCode = i.ProductCode,
                          ProductRemark = bb != null ? (ctg == "BB" ? string.Concat((bb == null ? "-" : bb.Composition), "", (bb == null ? "-" : bb.Width), "", (bb == null ? "-" : bb.Const), "", (bb == null ? "-" : bb.Yarn)) : bb.Name) : "-",
                          ReceiptCorrectionQty = i.ReceiptCorrectionQty,
                          ReceiptQty = i.ReceiptQty,
                          ReceiptUom = i.ReceiptUom,
                          RO = i.RO
                      }).ToList();

            stock1 = stock1.Where(x => (x.BeginningBalanceQty != 0) || (x.EndingBalanceQty != 0) || (x.ReceiptCorrectionQty != 0) || (x.ReceiptQty != 0) || (x.ExpendQty != 0)).ToList();

            decimal TotalReceiptQty = 0;
            decimal TotalCorrectionQty = 0;
            decimal TotalBeginningBalanceQty = 0;
            decimal TotalEndingBalanceQty = 0;
            double TotalExpendQty = 0;

            TotalReceiptQty = stock1.Sum(x => x.ReceiptQty);
            TotalCorrectionQty = stock1.Sum(x => x.ReceiptCorrectionQty);
            TotalBeginningBalanceQty = stock1.Sum(x => x.BeginningBalanceQty);
            TotalEndingBalanceQty = stock1.Sum(x => x.EndingBalanceQty);
            TotalExpendQty = stock1.Sum(x => x.ExpendQty);

            var stocks = new GarmentStockReportViewModel
            {
                BeginningBalanceQty = Math.Round(TotalBeginningBalanceQty, 2),
                BeginningBalanceUom = "",
                Buyer = "",
                EndingBalanceQty = Math.Round(TotalEndingBalanceQty, 2),
                EndingUom = "",
                ExpandUom = "",
                ExpendQty = Math.Round(TotalExpendQty, 2),
                NoArticle = "",
                PlanPo = "",
                ProductCode = "TOTAL",
                ProductRemark = "",
                ReceiptCorrectionQty = Math.Round(TotalCorrectionQty, 2),
                ReceiptQty = Math.Round(TotalReceiptQty, 2),
                ReceiptUom = "",
                RO = ""
            };

            stock1 = stock1.OrderBy(x => x.ProductCode).ThenBy(x => x.PlanPo).ToList();

            stock1.Add(stocks);

            return stock1;
        }

        public Tuple<List<GarmentStockReportViewModel>, int> GetStockReport(int offset, string unitcode, string tipebarang, int page, int size, string Order, DateTime? dateFrom, DateTime? dateTo)
        {
            //var Query = GetStockQuery(tipebarang, unitcode, dateFrom, dateTo, offset);
            //Query = Query.OrderByDescending(x => x.SupplierName).ThenBy(x => x.Dono);
            List<GarmentStockReportViewModel> Query = GetStockQuery(tipebarang, unitcode, dateFrom, dateTo, offset).ToList();
            //Data = Data.Where(x => (x.BeginningBalanceQty > 0) || (x.EndingBalanceQty > 0) || (x.ReceiptCorrectionQty > 0) || (x.ReceiptQty > 0) || (x.ExpendQty > 0)).ToList();
            //Data = Data.OrderBy(x => x.ProductCode).ThenBy(x => x.PlanPo).ToList();

            Pageable<GarmentStockReportViewModel> pageable = new Pageable<GarmentStockReportViewModel>(Query, page - 1, size);
            List<GarmentStockReportViewModel> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;
            //int TotalData = Data.Count();
            return Tuple.Create(Data, TotalData);
        }

        public MemoryStream GenerateExcelStockReport(string ctg, string categoryname, string unitname, string unitcode, DateTime? datefrom, DateTime? dateto, int offset)
        {
            var Query = GetStockQuery(ctg, unitcode, datefrom, dateto, offset);
            Query.RemoveAt(Query.Count() - 1);

            DataTable result = new DataTable();
            var headers = new string[] { "No", "Kode Barang", "No RO", "Plan PO", "Artikel", "Nama Barang", "Buyer", "Saldo Awal", "Saldo Awal2", "Penerimaan", "Penerimaan1", "Penerimaan2", "Pengeluaran", "Pengeluaran1", "Saldo Akhir", "Saldo Akhir1", "Asal" };
            var subheaders = new string[] { "Jumlah", "Sat", "Jumlah", "Koreksi", "Sat", "Jumlah", "Sat", "Jumlah", "Sat" };
            for (int i = 0; i < 7; i++)
            {
                result.Columns.Add(new DataColumn() { ColumnName = headers[i], DataType = typeof(string) });
            }

            result.Columns.Add(new DataColumn() { ColumnName = headers[7], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[8], DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[9], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[10], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[11], DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[12], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[13], DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[14], DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[15], DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = headers[16], DataType = typeof(String) });

            var index = 1;
            decimal BeginningQtyTotal = 0;
            decimal ReceiptQtyTotal = 0;
            decimal CorrQtyTotal = 0;
            double ExpendQtyTotal = 0;
            decimal EndingQtyTotal = 0;

            foreach (var item in Query)
            {
                BeginningQtyTotal += item.BeginningBalanceQty;
                ReceiptQtyTotal += item.ReceiptQty;
                ExpendQtyTotal += item.ExpendQty;
                EndingQtyTotal += item.EndingBalanceQty;
                CorrQtyTotal += item.ReceiptCorrectionQty;

                result.Rows.Add(index++, item.ProductCode, item.RO, item.PlanPo, item.NoArticle, /*item.ProductName,*/ item.ProductRemark, item.Buyer,

                    Convert.ToDouble(item.BeginningBalanceQty), item.BeginningBalanceUom, Convert.ToDouble(item.ReceiptQty), Convert.ToDouble(item.ReceiptCorrectionQty), item.ReceiptUom,
                    item.ExpendQty,
                    item.ExpandUom, Convert.ToDouble(item.EndingBalanceQty), item.EndingUom,
                    item.PaymentMethod /*== "FREE FROM BUYER" || item.PaymentMethod == "CMT" || item.PaymentMethod == "CMT/IMPORT" ? "BY" : "BL"*/);

            }

            ExcelPackage package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Data");


            DateTime DateFrom = datefrom == null ? new DateTime(1970, 1, 1) : (DateTime)datefrom;
            DateTime DateTo = dateto == null ? DateTime.Now : (DateTime)dateto;

            var col = (char)('A' + result.Columns.Count);
            string tglawal = new DateTimeOffset(DateFrom).ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
            string tglakhir = new DateTimeOffset(DateTo).ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
            sheet.Cells[$"A1:{col}1"].Value = string.Format("LAPORAN STOCK BARANG TERIMA SUBCON {0}", categoryname);
            sheet.Cells[$"A1:{col}1"].Merge = true;
            sheet.Cells[$"A1:{col}1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A1:{col}1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A1:{col}1"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Value = string.Format("Periode {0} - {1}", tglawal, tglakhir);
            sheet.Cells[$"A2:{col}2"].Merge = true;
            sheet.Cells[$"A2:{col}2"].Style.Font.Bold = true;
            sheet.Cells[$"A2:{col}2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A2:{col}2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            sheet.Cells[$"A3:{col}3"].Value = string.Format("KONFEKSI : {0}", unitname);
            sheet.Cells[$"A3:{col}3"].Merge = true;
            sheet.Cells[$"A3:{col}3"].Style.Font.Bold = true;
            sheet.Cells[$"A3:{col}3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            sheet.Cells[$"A3:{col}3"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;


            sheet.Cells["A7"].LoadFromDataTable(result, false, OfficeOpenXml.Table.TableStyles.Light16);
            sheet.Cells["H5"].Value = headers[7];
            sheet.Cells["H5:I5"].Merge = true;

            sheet.Cells["J5"].Value = headers[9];
            sheet.Cells["J5:L5"].Merge = true;
            sheet.Cells["M5"].Value = headers[12];
            sheet.Cells["M5:N5"].Merge = true;
            sheet.Cells["O5"].Value = headers[14];
            sheet.Cells["O5:P5"].Merge = true;

            foreach (var i in Enumerable.Range(0, 7))
            {
                col = (char)('A' + i);
                sheet.Cells[$"{col}5"].Value = headers[i];
                sheet.Cells[$"{col}5:{col}6"].Merge = true;
            }

            for (var i = 0; i < 9; i++)
            {
                col = (char)('H' + i);
                sheet.Cells[$"{col}6"].Value = subheaders[i];

            }

            foreach (var i in Enumerable.Range(0, 1))
            {
                col = (char)('Q' + i);
                sheet.Cells[$"{col}5"].Value = headers[i + 16];
                sheet.Cells[$"{col}5:{col}6"].Merge = true;
            }

            sheet.Cells["A5:Q6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells["A5:Q6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet.Cells["A5:Q6"].Style.Font.Bold = true;
            var widths = new int[] { 10, 15, 15, 20, 20, 15, 20, 15, 10, 10, 10, 10, 10, 10, 10, 10, 10, 15 };
            foreach (var i in Enumerable.Range(0, headers.Length))
            {
                sheet.Column(i + 1).Width = widths[i];
            }

            var a = Query.Count();
            sheet.Cells[$"A{7 + a}"].Value = "T O T A L  . . . . . . . . . . . . . . .";
            sheet.Cells[$"A{7 + a}:G{7 + a}"].Merge = true;
            sheet.Cells[$"A{7 + a}:G{7 + a}"].Style.Font.Bold = true;
            sheet.Cells[$"A{7 + a}:G{7 + a}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[$"A{7 + a}:G{7 + a}"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            sheet.Cells[$"H{7 + a}"].Value = BeginningQtyTotal;
            sheet.Cells[$"J{7 + a}"].Value = ReceiptQtyTotal;
            sheet.Cells[$"K{7 + a}"].Value = CorrQtyTotal;
            sheet.Cells[$"M{7 + a}"].Value = ExpendQtyTotal;
            sheet.Cells[$"O{7 + a}"].Value = EndingQtyTotal;

            MemoryStream stream = new MemoryStream();
            package.SaveAs(stream);
            return stream;
        }

        private List<GarmentCategoryViewModel> GetProductCategories(int page, int size, string order, string filter)
        {
            IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));


            var garmentSupplierUri = APIEndpoint.Core + $"master/garment-categories";
            string queryUri = "?page=" + page + "&size=" + size + "&order=" + order + "&filter=" + filter;
            string uri = garmentSupplierUri + queryUri;
            var httpResponse = httpClient.GetAsync($"{uri}").Result;

            if (httpResponse.IsSuccessStatusCode)
            {
                var content = httpResponse.Content.ReadAsStringAsync().Result;
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);

                List<GarmentCategoryViewModel> viewModel;
                if (result.GetValueOrDefault("data") == null)
                {
                    viewModel = new List<GarmentCategoryViewModel>();
                }
                else
                {
                    viewModel = JsonConvert.DeserializeObject<List<GarmentCategoryViewModel>>(result.GetValueOrDefault("data").ToString());

                }
                return viewModel;
            }
            else
            {
                List<GarmentCategoryViewModel> viewModel = new List<GarmentCategoryViewModel>();
                return viewModel;
            }

            //if (httpClient != null)
            //{
            //    var garmentSupplierUri = APIEndpoint.Core + $"master/garment-categories";
            //    string queryUri = "?page=" + page + "&size=" + size + "&order=" + order + "&filter=" + filter;
            //    string uri = garmentSupplierUri + queryUri;
            //    var response = httpClient.GetAsync($"{uri}").Result.Content.ReadAsStringAsync();
            //    Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
            //    List<GarmentCategoryViewModel> viewModel = JsonConvert.DeserializeObject<List<GarmentCategoryViewModel>>(result.GetValueOrDefault("data").ToString());
            //    return viewModel;
            //}
            //else
            //{
            //    List<GarmentCategoryViewModel> viewModel = null;
            //    return viewModel;
            //}
        }

        private List<GarmentProductViewModel> GetProductCode(string codes)
        {

            IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));

            var httpContent = new StringContent(JsonConvert.SerializeObject(codes), Encoding.UTF8, "application/json");

            string garmentProductionUri = APIEndpoint.Core + $"master/garmentProducts/byCodes";
            var httpResponse = httpClient.SendAsync(HttpMethod.Get, garmentProductionUri, httpContent).Result;

            if (httpResponse.IsSuccessStatusCode)
            {
                var content = httpResponse.Content.ReadAsStringAsync().Result;
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);

                List<GarmentProductViewModel> viewModel;
                if (result.GetValueOrDefault("data") == null)
                {
                    viewModel = new List<GarmentProductViewModel>();
                }
                else
                {
                    viewModel = JsonConvert.DeserializeObject<List<GarmentProductViewModel>>(result.GetValueOrDefault("data").ToString());

                }
                return viewModel;
            }
            else
            {
                List<GarmentProductViewModel> viewModel = new List<GarmentProductViewModel>();
                return viewModel;
            }
        }
    }
}
