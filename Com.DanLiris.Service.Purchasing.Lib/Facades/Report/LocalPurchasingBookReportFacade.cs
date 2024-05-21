﻿using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Models.UnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.PurchaseOrder;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNoteViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Report
{
    public class LocalPurchasingBookReportFacade : ILocalPurchasingBookReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<UnitReceiptNote> dbSet;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IdentityService _identityService;
        private readonly string IDRCurrencyCode = "IDR";

        public LocalPurchasingBookReportFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<UnitReceiptNote>();
            _currencyProvider = (ICurrencyProvider)serviceProvider.GetService(typeof(ICurrencyProvider));
            _identityService = serviceProvider.GetService<IdentityService>();
        }
        #region oldQuery

        #endregion
        public async Task<LocalPurchasingBookReportViewModel> GetReportDataV2(string no, int accountingUnitId, int accountingCategoryId, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, bool isValas, int divisionId)
        {
            var dataReceiptNote = await GetReportUnitReceiptNote(no, accountingUnitId, accountingCategoryId, dateFrom, dateTo, inputDateFrom, inputDateTo, isValas, divisionId);
            var dataReceiptNoteCorrection = await GetReportUnitReceiptNoteCorrection(no, accountingUnitId, accountingCategoryId, dateFrom, dateTo, inputDateFrom, inputDateTo, isValas, divisionId);

            var reportReceipt = new List<PurchasingReport>();
            reportReceipt.AddRange(dataReceiptNote);
            reportReceipt.AddRange(dataReceiptNoteCorrection);

            var reportResult = new LocalPurchasingBookReportViewModel();

            reportResult.Reports = reportReceipt.OrderBy(data => data.URNId).OrderBy(data => data.UPONo).ThenBy(data => data.DataSourceSort).ToList();

            reportResult.CategorySummaries = reportResult.Reports
                        .GroupBy(report => new { report.AccountingCategoryName })
                        .Select(report => new Summary()
                        {
                            Category = report.Key.AccountingCategoryName,
                            SubTotal = report.Sum(sum => sum.TotalCurrency),
                            AccountingLayoutIndex = report.Select(item => item.AccountingLayoutIndex).FirstOrDefault()
                        }).OrderBy(order => order.AccountingLayoutIndex).ToList();
            reportResult.CurrencySummaries = reportResult.Reports
                .GroupBy(report => new { report.CurrencyCode })
                .Select(report => new Summary()
                {
                    CurrencyCode = report.Key.CurrencyCode,
                    SubTotal = report.Sum(sum => sum.Total),
                    SubTotalCurrency = report.Sum(sum => sum.TotalCurrency)
                }).OrderBy(order => order.CurrencyCode).ToList();
            return reportResult;

        }

        public async Task<LocalPurchasingBookReportViewModel> GetReportDataV2byDO(string no, int accountingUnitId, int accountingCategoryId, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, bool isValas, int divisionId)
        {
            var dataReceiptNote = await GetReportUnitReceiptNote(no, accountingUnitId, accountingCategoryId, dateFrom, dateTo, inputDateFrom, inputDateTo, isValas, divisionId);
            var dataReceiptNoteCorrection = await GetReportUnitReceiptNoteCorrection(no, accountingUnitId, accountingCategoryId, dateFrom, dateTo, inputDateFrom, inputDateTo, isValas, divisionId);

            var reportReceipt = new List<PurchasingReport>();
            reportReceipt.AddRange(dataReceiptNote);
            reportReceipt.AddRange(dataReceiptNoteCorrection);

            var reportResult = new LocalPurchasingBookReportViewModel();

            reportResult.Reports = reportReceipt.GroupBy(x => new { x.DONo }).Select(s => new PurchasingReport()
            {
                DataSourceSort = 1,
                CategoryName = s.First().CategoryName,
                CategoryCode = s.First().CategoryCode,
                AccountingCategoryName = s.First().AccountingCategoryName,
                AccountingCategoryCode = s.First().AccountingCategoryCode,
                AccountingLayoutIndex = s.First().AccountingLayoutIndex,
                CurrencyRate = (decimal)s.First().CurrencyRate,
                DONo = s.Key.DONo,
                DPP = s.Sum(z =>z.DPP),
                DPPCurrency = s.First().DPPCurrency,
                InvoiceNo = s.First().InvoiceNo,
                VATNo = s.First().VATNo,
                IPONo = s.First().IPONo,
                VAT = s.Sum( z => z.VAT),
                Total = s.Sum( z => z.Total),
                TotalCurrency = s.Sum(z => z.TotalCurrency),
                ProductName = s.First().ProductName,
                ReceiptDate = s.First().ReceiptDate,
                SupplierCode = s.First().SupplierCode,
                SupplierName = s.First().SupplierName,
                UnitName = s.First().UnitName,
                UnitCode = s.First().UnitCode,
                AccountingUnitName = s.First().AccountingUnitName,
                AccountingUnitCode = s.First().AccountingUnitCode,
                UPONo = s.First().UPONo,
                URNNo = s.First().URNNo,
                IsUseVat = s.First().IsUseVat,
                CurrencyCode = s.First().CurrencyCode,
                Quantity = s.Sum( z =>z.Quantity),
                Uom = s.First().Uom,
                Remark = s.First().Remark,
                IncomeTax = s.First().IncomeTax,
                IncomeTaxBy = s.First().IncomeTaxBy,
                URNId = s.First().URNId,
                CorrectionDate = s.First().CorrectionDate,
                CorrectionNo = s.First().CorrectionNo
            }).ToList().OrderBy(data => data.URNId).OrderBy(data => data.UPONo).ThenBy(data => data.DataSourceSort).ToList();


            reportResult.CategorySummaries = reportResult.Reports
                        .GroupBy(report => new { report.AccountingCategoryName })
                        .Select(report => new Summary()
                        {
                            Category = report.Key.AccountingCategoryName,
                            SubTotal = report.Sum(sum => sum.TotalCurrency),
                            AccountingLayoutIndex = report.Select(item => item.AccountingLayoutIndex).FirstOrDefault()
                        }).OrderBy(order => order.AccountingLayoutIndex).ToList();
            reportResult.CurrencySummaries = reportResult.Reports
                .GroupBy(report => new { report.CurrencyCode })
                .Select(report => new Summary()
                {
                    CurrencyCode = report.Key.CurrencyCode,
                    SubTotal = report.Sum(sum => sum.Total),
                    SubTotalCurrency = report.Sum(sum => sum.TotalCurrency)
                }).OrderBy(order => order.CurrencyCode).ToList();
            return reportResult;

        }

        public async Task<List<PurchasingReport>> GetReportUnitReceiptNote(string no, int accountingUnitId, int accountingCategoryId, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, bool isValas, int divisionId)
        {
            var d1 = dateFrom.GetValueOrDefault().ToUniversalTime();
            var d2 = (dateTo.HasValue ? dateTo.Value : DateTime.Now).ToUniversalTime();
            var inputFrom = (inputDateFrom.HasValue ? inputDateFrom.Value : DateTime.MinValue);
            var inputTo = (inputDateTo.HasValue ? inputDateTo.Value : DateTime.Now);

            var query = from urnWithItem in dbContext.UnitReceiptNoteItems
                        join urnHdr in dbContext.UnitReceiptNotes on urnWithItem.URNId equals urnHdr.Id

                        join pr in dbContext.PurchaseRequests on urnWithItem.PRId equals pr.Id into joinPurchaseRequest
                        from urnPR in joinPurchaseRequest.DefaultIfEmpty()

                        join epoDetail in dbContext.ExternalPurchaseOrderDetails on urnWithItem.EPODetailId equals epoDetail.Id into joinExternalPurchaseOrder
                        from urnEPODetail in joinExternalPurchaseOrder.DefaultIfEmpty()

                        join upoItem in dbContext.UnitPaymentOrderDetails on urnWithItem.Id equals upoItem.URNItemId into joinUnitPaymentOrder
                        from urnUPOItem in joinUnitPaymentOrder.DefaultIfEmpty()

                        where urnWithItem.UnitReceiptNote.ReceiptDate.AddHours(7).Date >= d1.Date
                            && urnWithItem.UnitReceiptNote.ReceiptDate.AddHours(7).Date <= d2.Date
                            && !urnWithItem.UnitReceiptNote.SupplierIsImport
                            && urnWithItem.CreatedUtc.Date >= inputFrom.Date
                            && urnWithItem.CreatedUtc.Date <= inputTo.Date

                        select new
                        {
                            // PR Info
                            urnPR.CategoryCode,
                            urnPR.CategoryName,
                            urnPR.CategoryId,

                            urnWithItem.PRId,
                            urnWithItem.UnitReceiptNote.DOId,
                            urnWithItem.UnitReceiptNote.DONo,
                            urnWithItem.UnitReceiptNote.URNNo,
                            URNId = urnWithItem.UnitReceiptNote.Id,
                            urnWithItem.ProductName,
                            urnWithItem.UnitReceiptNote.ReceiptDate,
                            urnWithItem.UnitReceiptNote.SupplierName,
                            urnWithItem.UnitReceiptNote.SupplierCode,
                            urnWithItem.UnitReceiptNote.SupplierIsImport,
                            urnWithItem.UnitReceiptNote.UnitCode,
                            urnWithItem.UnitReceiptNote.UnitName,
                            urnWithItem.UnitReceiptNote.UnitId,
                            urnWithItem.UnitReceiptNote.DivisionId,
                            urnWithItem.EPODetailId,
                            urnWithItem.PricePerDealUnit,
                            urnWithItem.ReceiptQuantity,
                            urnWithItem.Uom,

                            // EPO Info
                            urnEPODetail.ExternalPurchaseOrderItem.PONo,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.UseVat,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.VatRate,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IncomeTaxRate,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.UseIncomeTax,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IncomeTaxBy,
                            EPOPricePerDealUnit = urnEPODetail.PricePerDealUnit,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.CurrencyCode,

                            // UPO Info
                            InvoiceNo = urnUPOItem.UnitPaymentOrderItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrderItem.UnitPaymentOrder.InvoiceNo : "",
                            UPONo = urnUPOItem.UnitPaymentOrderItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrderItem.UnitPaymentOrder.UPONo : urnHdr.URNNo,
                            VatNo = urnUPOItem.UnitPaymentOrderItem.UnitPaymentOrder != null ? urnUPOItem.UnitPaymentOrderItem.UnitPaymentOrder.VatNo : "",
                            urnPR.Remark
                        };

            if (isValas)
            {
                query = query.Where(urn => urn.CurrencyCode != IDRCurrencyCode);
            }
            else
            {
                query = query.Where(urn => urn.CurrencyCode == IDRCurrencyCode);
            }

            if (divisionId > 0)
                query = query.Where(urn => urn.DivisionId == divisionId.ToString());

            if (!string.IsNullOrWhiteSpace(no))
                query = query.Where(urn => urn.URNNo == no);

            var unitFilterIds = await _currencyProvider.GetUnitsIdsByAccountingUnitId(accountingUnitId);
            if (unitFilterIds.Count() > 0)
                query = query.Where(urn => unitFilterIds.Contains(urn.UnitId));

            var categoryFilterIds = await _currencyProvider.GetCategoryIdsByAccountingCategoryId(accountingCategoryId);
            if (categoryFilterIds.Count() > 0)
                query = query.Where(urn => categoryFilterIds.Contains(urn.CategoryId));

            var queryResult = query.OrderByDescending(item => item.ReceiptDate).ToList();
            var currencyTuples = queryResult.Select(item => new Tuple<string, DateTimeOffset>(item.CurrencyCode, item.ReceiptDate));
            var currencies = await _currencyProvider.GetCurrencyByCurrencyCodeDateList(currencyTuples);

            var unitIds = queryResult.Select(item =>
            {
                int.TryParse(item.UnitId, out var unitId);
                return unitId;
            }).Distinct().ToList();
            var units = await _currencyProvider.GetUnitsByUnitIds(unitIds);
            var accountingUnits = await _currencyProvider.GetAccountingUnitsByUnitIds(unitIds);

            var categoryIds = queryResult.Select(item =>
            {
                int.TryParse(item.CategoryId, out var categoryId);
                return categoryId;
            }).Distinct().ToList();
            var categories = await _currencyProvider.GetCategoriesByCategoryIds(categoryIds);
            var accountingCategories = await _currencyProvider.GetAccountingCategoriesByCategoryIds(categoryIds);

            var reportResult = new List<PurchasingReport>();
            foreach (var item in queryResult)
            {
                var currency = currencies.Where(entity => entity.Code == item.CurrencyCode && entity.Date <= item.ReceiptDate).OrderByDescending(entity => entity.Date).FirstOrDefault();

                int.TryParse(item.UnitId, out var unitId);
                var unit = units.FirstOrDefault(element => element.Id == unitId);
                var accountingUnit = new AccountingUnit();
                if (unit != null)
                {
                    accountingUnit = accountingUnits.FirstOrDefault(element => element.Id == unit.AccountingUnitId);
                    if (accountingUnit == null)
                        accountingUnit = new AccountingUnit();
                }

                int.TryParse(item.CategoryId, out var categoryId);
                var category = categories.FirstOrDefault(element => element.Id == categoryId);
                var accountingCategory = new AccountingCategory();
                if (category != null)
                {
                    accountingCategory = accountingCategories.FirstOrDefault(element => element.Id == category.AccountingCategoryId);
                    if (accountingCategory == null)
                        accountingCategory = new AccountingCategory();
                }

                decimal dpp = 0;
                decimal dppCurrency = 0;
                decimal ppn = 0;
                decimal incomeTax = 0;
                decimal total = 0;
                decimal totalCurrency = 0;
                decimal.TryParse(item.IncomeTaxRate, out var incomeTaxRate);

                //default IDR
                double currencyRate = 1;
                var currencyCode = "IDR";
                if (currency != null && !currency.Code.Equals("IDR"))
                {
                    currencyRate = currency.Rate.GetValueOrDefault();
                    dpp = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity);
                    dppCurrency = dpp * (decimal)currencyRate;
                    currencyCode = currency.Code;
                }
                else
                    dpp = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity);

                if (item.UseVat)
                    ppn = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity * ( Convert.ToDouble(item.VatRate) / 100));

                if (item.UseIncomeTax)
                    incomeTax = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity) * incomeTaxRate / 100;

                if (item.IncomeTaxBy == "Supplier")
                {
                    total = dpp + ppn - incomeTax;
                    totalCurrency = (dpp + ppn - incomeTax) * (decimal)currencyRate;
                }
                else
                {
                    total = dpp + ppn;
                    totalCurrency = (dpp + ppn) * (decimal)currencyRate;
                }


                var reportItem = new PurchasingReport()
                {
                    DataSourceSort = 1,
                    CategoryName = item.CategoryName,
                    CategoryCode = item.CategoryCode,
                    AccountingCategoryName = accountingCategory.Name,
                    AccountingCategoryCode = accountingCategory.Code,
                    AccountingLayoutIndex = accountingCategory.AccountingLayoutIndex,
                    CurrencyRate = (decimal)currencyRate,
                    DONo = item.DONo,
                    DPP = dpp,
                    DPPCurrency = dppCurrency,
                    InvoiceNo = item.InvoiceNo,
                    VATNo = item.VatNo,
                    IPONo = item.PONo,
                    VAT = ppn,
                    Total = total,
                    TotalCurrency = totalCurrency,
                    ProductName = item.ProductName,
                    ReceiptDate = item.ReceiptDate,
                    SupplierCode = item.SupplierCode,
                    SupplierName = item.SupplierName,
                    UnitName = item.UnitName,
                    UnitCode = item.UnitCode,
                    AccountingUnitName = accountingUnit.Name,
                    AccountingUnitCode = accountingUnit.Code,
                    UPONo = item.UPONo,
                    URNNo = item.URNNo,
                    IsUseVat = item.UseVat,
                    CurrencyCode = currencyCode,
                    Quantity = item.ReceiptQuantity,
                    Uom = item.Uom,
                    Remark = item.Remark,
                    IncomeTax = incomeTax,
                    IncomeTaxBy = item.IncomeTaxBy,
                    URNId = item.URNId,
                    CorrectionDate = null
                };

                reportResult.Add(reportItem);
            }
            return reportResult;
        }

        public async Task<List<PurchasingReport>> GetReportUnitReceiptNoteCorrection(string no, int accountingUnitId, int accountingCategoryId, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, bool isValas, int divisionId)
        {
            var d1 = dateFrom.GetValueOrDefault().ToUniversalTime();
            var d2 = (dateTo.HasValue ? dateTo.Value : DateTime.Now).ToUniversalTime();
            var inputFrom = (inputDateFrom.HasValue ? inputDateFrom.Value : DateTime.MinValue);
            var inputTo = (inputDateTo.HasValue ? inputDateTo.Value : DateTime.Now);

            var query = from upcCorrectionNoteItem in dbContext.UnitPaymentCorrectionNoteItems

                        join upcCorrectionNote in dbContext.UnitPaymentCorrectionNotes on upcCorrectionNoteItem.UPCId equals upcCorrectionNote.Id into joinupcCorrections
                        from upcCorrection in joinupcCorrections

                        join upoDetailNotes in dbContext.UnitPaymentOrderDetails on upcCorrectionNoteItem.UPODetailId equals upoDetailNotes.Id into joinUpcUpoDetails
                        from joinUpcUpoDetail in joinUpcUpoDetails.DefaultIfEmpty()

                        join pr in dbContext.PurchaseRequests on upcCorrectionNoteItem.PRId equals pr.Id into joinPurchaseRequest
                        from urnPR in joinPurchaseRequest.DefaultIfEmpty()

                        join urnItem in dbContext.UnitReceiptNoteItems on joinUpcUpoDetail.URNItemId equals urnItem.Id into urnItems
                        from urnWithItem in urnItems

                        join epoDetail in dbContext.ExternalPurchaseOrderDetails on urnWithItem.EPODetailId equals epoDetail.Id into joinExternalPurchaseOrder
                        from urnEPODetail in joinExternalPurchaseOrder.DefaultIfEmpty()

                            //join upoItem in dbContext.UnitPaymentOrderDetails on upcCorrection.UPOId equals upoItem.Id  into joinUnitPaymentOrder
                            //from urnUPOItem in joinUnitPaymentOrder.DefaultIfEmpty()

                            //join upoDetail in dbContext.UnitPaymentOrderDetails on urnUPOItem.Id equals upoDetail.URNItemId into joinUnitPaymentOrderDetails
                            //from urnUPODetail in joinUnitPaymentOrderDetails.DefaultIfEmpty()

                        where upcCorrection.CorrectionDate.AddHours(7).Date >= d1.Date 
                            && upcCorrection.CorrectionDate.AddHours(7).Date <= d2.Date 
                            && !urnWithItem.UnitReceiptNote.SupplierIsImport
                            && upcCorrection.CreatedUtc.Date >= inputFrom.Date
                            && upcCorrection.CreatedUtc.Date <= inputTo.Date
                        select new
                        {
                            // PR Info
                            urnPR.CategoryCode,
                            urnPR.CategoryName,
                            urnPR.CategoryId,

                            urnWithItem.PRId,
                            urnWithItem.UnitReceiptNote.DOId,
                            urnWithItem.UnitReceiptNote.DONo,
                            urnWithItem.UnitReceiptNote.URNNo,
                            URNId = urnWithItem.UnitReceiptNote.Id,
                            urnWithItem.ProductName,
                            urnWithItem.UnitReceiptNote.ReceiptDate,
                            urnWithItem.UnitReceiptNote.SupplierName,
                            urnWithItem.UnitReceiptNote.SupplierCode,
                            urnWithItem.UnitReceiptNote.SupplierIsImport,
                            urnWithItem.UnitReceiptNote.UnitCode,
                            urnWithItem.UnitReceiptNote.UnitName,
                            urnWithItem.UnitReceiptNote.UnitId,
                            urnWithItem.UnitReceiptNote.DivisionId,
                            urnWithItem.EPODetailId,
                            urnWithItem.PricePerDealUnit,
                            urnWithItem.ReceiptQuantity,
                            urnWithItem.Uom,

                            // EPO Info
                            urnEPODetail.ExternalPurchaseOrderItem.PONo,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.UseVat,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.VatRate,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IncomeTaxRate,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.UseIncomeTax,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.IncomeTaxBy,
                            EPOPricePerDealUnit = urnEPODetail.PricePerDealUnit,
                            urnEPODetail.ExternalPurchaseOrderItem.ExternalPurchaseOrder.CurrencyCode,

                            // UPO Info
                            InvoiceNo = joinUpcUpoDetail.UnitPaymentOrderItem.UnitPaymentOrder != null ? joinUpcUpoDetail.UnitPaymentOrderItem.UnitPaymentOrder.InvoiceNo : "",
                            UPONo = joinUpcUpoDetail.UnitPaymentOrderItem.UnitPaymentOrder != null ? joinUpcUpoDetail.UnitPaymentOrderItem.UnitPaymentOrder.UPONo : "",
                            VatNo = joinUpcUpoDetail.UnitPaymentOrderItem.UnitPaymentOrder != null ? joinUpcUpoDetail.UnitPaymentOrderItem.UnitPaymentOrder.VatNo : "",
                            urnPR.Remark,

                            //Correction Info
                            CorrectionNo = upcCorrection.UPCNo,
                            upcCorrection.CorrectionDate,
                            upcCorrection.CorrectionType,
                            CorrectionPricePerDealUnitBefore = upcCorrectionNoteItem.PricePerDealUnitBefore,
                            CorrectionPricePerDealUnitAfter = upcCorrectionNoteItem.PricePerDealUnitAfter,
                            CorrectionPriceTotalBefore = upcCorrectionNoteItem.PriceTotalBefore,
                            CorrectionPriceTotalAfter = upcCorrectionNoteItem.PriceTotalAfter,
                            CorrectionQuantity = upcCorrectionNoteItem.Quantity,
                            CorrectionReceiptQuantity = joinUpcUpoDetail.ReceiptQuantity,
                            CorrectionQuantityCorrection = joinUpcUpoDetail.QuantityCorrection,
                            UnitReceiptNoteNo = upcCorrectionNoteItem.URNNo,
                            UPODetailId = upcCorrectionNoteItem.UPODetailId
                        };

            if (isValas)
            {
                query = query.Where(urn => urn.CurrencyCode != IDRCurrencyCode);
            }
            else
            {
                query = query.Where(urn => urn.CurrencyCode == IDRCurrencyCode);
            }

            if (divisionId > 0)
                query = query.Where(urn => urn.DivisionId == divisionId.ToString());

            if (!string.IsNullOrWhiteSpace(no))
                query = query.Where(urn => urn.URNNo == no);

            var unitFilterIds = await _currencyProvider.GetUnitsIdsByAccountingUnitId(accountingUnitId);
            if (unitFilterIds.Count() > 0)
                query = query.Where(urn => unitFilterIds.Contains(urn.UnitId));

            var categoryFilterIds = await _currencyProvider.GetCategoryIdsByAccountingCategoryId(accountingCategoryId);
            if (categoryFilterIds.Count() > 0)
                query = query.Where(urn => categoryFilterIds.Contains(urn.CategoryId));

            var queryResult = query.ToList().OrderByDescending(item => item.CorrectionDate).ToList();
            var currencyTuples = queryResult.Select(item => new Tuple<string, DateTimeOffset>(item.CurrencyCode, item.CorrectionDate));
            var currencies = await _currencyProvider.GetCurrencyByCurrencyCodeDateList(currencyTuples);

            var urnNos = queryResult.Select(element => element.UnitReceiptNoteNo).ToList();
            var correctionItems = dbContext.UnitPaymentCorrectionNoteItems.Where(entity => urnNos.Contains(entity.URNNo)).ToList();
            var correctionIds = correctionItems.Select(element => element.UPCId).ToList();
            var corrections = dbContext.UnitPaymentCorrectionNotes.Where(entity => correctionIds.Contains(entity.Id)).ToList();

            var unitIds = queryResult.Select(item =>
            {
                int.TryParse(item.UnitId, out var unitId);
                return unitId;
            }).Distinct().ToList();
            var units = await _currencyProvider.GetUnitsByUnitIds(unitIds);
            var accountingUnits = await _currencyProvider.GetAccountingUnitsByUnitIds(unitIds);

            var categoryIds = queryResult.Select(item =>
            {
                int.TryParse(item.CategoryId, out var categoryId);
                return categoryId;
            }).Distinct().ToList();
            var categories = await _currencyProvider.GetCategoriesByCategoryIds(categoryIds);
            var accountingCategories = await _currencyProvider.GetAccountingCategoriesByCategoryIds(categoryIds);

            var reportResult = new List<PurchasingReport>();
            foreach (var item in queryResult)
            {
                //var purchaseRequest = purchaseRequests.FirstOrDefault(f => f.Id.Equals(urnItem.PRId));
                //var unitPaymentOrder = unitPaymentOrders.FirstOrDefault(f => f.URNId.Equals(urnItem.URNId));
                //var epoItem = epoItems.FirstOrDefault(f => f.epoDetailIds.Contains(urnItem.EPODetailId));
                //var epoDetail = epoItem.Details.FirstOrDefault(f => f.Id.Equals(urnItem.EPODetailId));
                //var selectedCurrencies = currencies.Where(element => element.Code == item.CurrencyCode).ToList();
                //var currency = selectedCurrencies.OrderBy(entity => (entity.Date - item.ReceiptDate).Duration()).FirstOrDefault();
                var currency = currencies.Where(entity => entity.Code == item.CurrencyCode && entity.Date <= item.ReceiptDate).ToList().Select(o => new { Diffs = Math.Abs((o.Date.Date - item.ReceiptDate.DateTime.Date).Days), o.Date, o.Code, o.Rate }).OrderBy(o => o.Diffs).FirstOrDefault();


                int.TryParse(item.UnitId, out var unitId);
                var unit = units.FirstOrDefault(element => element.Id == unitId);
                var accountingUnit = new AccountingUnit();
                if (unit != null)
                {
                    accountingUnit = accountingUnits.FirstOrDefault(element => element.Id == unit.AccountingUnitId);
                    if (accountingUnit == null)
                        accountingUnit = new AccountingUnit();
                }

                int.TryParse(item.CategoryId, out var categoryId);
                var category = categories.FirstOrDefault(element => element.Id == categoryId);
                var accountingCategory = new AccountingCategory();
                if (category != null)
                {
                    accountingCategory = accountingCategories.FirstOrDefault(element => element.Id == category.AccountingCategoryId);
                    if (accountingCategory == null)
                        accountingCategory = new AccountingCategory();
                }

                decimal dpp = 0;
                decimal dppCurrency = 0;
                decimal ppn = 0;
                decimal incomeTax = 0;
                decimal total = 0;
                decimal totalCurrency = 0;
                double quantity = 0;
                decimal.TryParse(item.IncomeTaxRate, out var incomeTaxRate);

                //default IDR
                double currencyRate = 1;
                var currencyCode = "IDR";
                if (currency != null && !currency.Code.Equals("IDR"))
                {
                    currencyRate = currency.Rate.GetValueOrDefault();
                    //dpp = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity);
                    dpp = (decimal)CalculateCorrectionDpp(item.CorrectionType, item.CorrectionPricePerDealUnitBefore, item.CorrectionPricePerDealUnitAfter, item.CorrectionPriceTotalBefore, item.CorrectionPriceTotalAfter, item.CorrectionQuantity, item.CorrectionReceiptQuantity, item.CorrectionQuantityCorrection, item.UPODetailId, corrections, correctionItems, item.CorrectionDate);

                    dppCurrency = dpp * (decimal)currencyRate;
                    currencyCode = currency.Code;
                }
                else
                    //dpp = (decimal)(item.EPOPricePerDealUnit * item.ReceiptQuantity);
                    dpp = (decimal)CalculateCorrectionDpp(item.CorrectionType, item.CorrectionPricePerDealUnitBefore, item.CorrectionPricePerDealUnitAfter, item.CorrectionPriceTotalBefore, item.CorrectionPriceTotalAfter, item.CorrectionQuantity, item.CorrectionReceiptQuantity, item.CorrectionQuantityCorrection, item.UPODetailId, corrections, correctionItems, item.CorrectionDate);


                if (item.UseVat)
                    ppn = (decimal)(dpp * (Convert.ToDecimal(item.VatRate) / 100));

                if (item.UseIncomeTax && item.IncomeTaxBy == "Supplier")
                    incomeTax = (decimal)(dpp * (incomeTaxRate / 100));

                if (item.IncomeTaxBy == "Supplier")
                {
                    total = dpp + ppn - incomeTax;
                    totalCurrency = (dpp + ppn - incomeTax) * (decimal)currencyRate;
                }
                else
                {
                    total = dpp + ppn;
                    totalCurrency = (dpp + ppn) * (decimal)currencyRate;
                }

                quantity = CalculateCorrectionQuantity(item.CorrectionType, item.CorrectionQuantity, item.CorrectionReceiptQuantity, item.CorrectionQuantityCorrection);

                var reportItem = new PurchasingReport()
                {
                    DataSourceSort = 2,
                    CategoryName = item.CategoryName,
                    CategoryCode = item.CategoryCode,
                    AccountingCategoryName = accountingCategory.Name,
                    AccountingCategoryCode = accountingCategory.Code,
                    AccountingLayoutIndex = accountingCategory.AccountingLayoutIndex,
                    CurrencyRate = (decimal)currencyRate,
                    DONo = item.DONo,
                    DPP = dpp,
                    DPPCurrency = dppCurrency,
                    InvoiceNo = item.InvoiceNo,
                    VATNo = item.VatNo,
                    IPONo = item.PONo,
                    VAT = ppn,
                    Total = total,
                    TotalCurrency = totalCurrency,
                    ProductName = item.ProductName,
                    ReceiptDate = item.ReceiptDate,
                    SupplierCode = item.SupplierCode,
                    SupplierName = item.SupplierName,
                    UnitName = item.UnitName,
                    UnitCode = item.UnitCode,
                    AccountingUnitName = accountingUnit.Name,
                    AccountingUnitCode = accountingUnit.Code,
                    UPONo = item.UPONo,
                    URNNo = item.URNNo,
                    IsUseVat = item.UseVat,
                    CurrencyCode = currencyCode,
                    //Quantity = item.ReceiptQuantity,
                    Quantity = quantity,
                    Uom = item.Uom,
                    Remark = item.Remark,
                    IncomeTax = incomeTax,
                    IncomeTaxBy = item.IncomeTaxBy,
                    CorrectionDate = item.CorrectionDate,
                    CorrectionNo = item.CorrectionNo,
                    URNId = item.URNId
                };

                reportResult.Add(reportItem);
            }
            return reportResult;
        }

        public Task<LocalPurchasingBookReportViewModel> GetReportV2(string no, int accountingUnitId, int accountingCategoryId, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, bool isValas, int divisionId)
        {
            return GetReportDataV2(no, accountingUnitId, accountingCategoryId, dateFrom, dateTo, inputDateFrom, inputDateTo, isValas, divisionId);
        }

        public Task<LocalPurchasingBookReportViewModel> GetReportV2byDO(string no, int accountingUnitId, int accountingCategoryId, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, bool isValas, int divisionId)
        {
            return GetReportDataV2byDO(no, accountingUnitId, accountingCategoryId, dateFrom, dateTo, inputDateFrom, inputDateTo, isValas, divisionId);


        }

        private double CalculateCorrectionDpp(string correctionType, double pricePerDealBefore, double priceperDealAfter, double priceTotalBefore, double priceTotalAfter, double quantity, double receiptQuantity, double quantityCorrection, long uPODetailId, List<Models.UnitPaymentCorrectionNoteModel.UnitPaymentCorrectionNote> corrections, List<Models.UnitPaymentCorrectionNoteModel.UnitPaymentCorrectionNoteItem> correctionItems, DateTimeOffset correctionDate)
        {
            var previousCorrection = new PreviousCorrection();

            var upcIds = correctionItems.Where(element => element.UPODetailId == uPODetailId).Select(element => element.UPCId).ToList();
            var previousCorrectionNotes = corrections.Where(element => upcIds.Contains(element.Id) && element.CorrectionDate < correctionDate).OrderBy(element => element.UPCNo).ToList();

            foreach (var previousCorrectionNote in previousCorrectionNotes)
            {
                var previousCorrectionNoteItem = correctionItems.FirstOrDefault(element => element.UPCId == previousCorrectionNote.Id && element.UPODetailId == uPODetailId);

                if (previousCorrectionNote.CorrectionType == "Harga Satuan")
                {
                    previousCorrection.PricePerDealCorrection = previousCorrectionNoteItem.PricePerDealUnitAfter;
                }
                else if (previousCorrectionNote.CorrectionType == "Harga Total")
                {
                    previousCorrection.TotalCorrection = previousCorrectionNoteItem.PriceTotalAfter;
                }
            }

            switch (correctionType)
            {
                case "Harga Satuan":
                    if (previousCorrection.PricePerDealCorrection == 0)
                        return ((priceperDealAfter - pricePerDealBefore) * quantity);
                    else
                        return ((priceperDealAfter - previousCorrection.PricePerDealCorrection) * quantity);
                case "Harga Total":
                    if (previousCorrection.TotalCorrection == 0)
                        return priceTotalAfter - priceTotalBefore;
                    else
                        return priceTotalAfter - previousCorrection.TotalCorrection;
                case "Jumlah":
                    //return (priceperDealAfter * (Math.Abs(quantityCorrection-quantity)))*-1 ;
                    return priceTotalAfter * -1;
                default:
                    return 0;
            }
        }

        private double CalculateCorrectionQuantity(string correctionType, double quantity, double receiptQuantity, double quantityCorrection)
        {
            switch (correctionType)
            {
                case "Harga Satuan":
                    return receiptQuantity;
                case "Harga Total":
                    return receiptQuantity;
                case "Jumlah":
                    return quantity * -1;
                default:
                    return 0;
                    break;
            }
        }

        private DataTable GetFormatReportExcel(bool isValas)
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No PO", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Bon Penerimaan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Faktur Pajak", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No SPB/NI", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No. Nota Koreksi", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Tanggal Nota Koreksi", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Kategori Pembelian", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Kategori Pembukuan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Unit Pembelian", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Unit Pembukuan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Quantity", DataType = typeof(decimal) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Satuan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });

            if (isValas)
            {
                dt.Columns.Add(new DataColumn() { ColumnName = "Kurs", DataType = typeof(string) });
                dt.Columns.Add(new DataColumn() { ColumnName = "DPP Valas", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "DPP (IDR)", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "PPN (IDR)", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "PPH (IDR)", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });
            }
            else
            {
                dt.Columns.Add(new DataColumn() { ColumnName = "DPP", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "PPN", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "PPH", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });
            }

            return dt;
        }
        private DataTable GetFormatReportExcelMII(bool isValas)
        {
            var dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
            //dt.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            //dt.Columns.Add(new DataColumn() { ColumnName = "No PO", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Bon Penerimaan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No Faktur Pajak", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No SPB/NI", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "No. Nota Koreksi", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Tanggal Nota Koreksi", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Kategori Pembelian", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Kategori Pembukuan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Unit Pembelian", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Unit Pembukuan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Quantity", DataType = typeof(decimal) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Satuan", DataType = typeof(string) });
            dt.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });

            if (isValas)
            {
                dt.Columns.Add(new DataColumn() { ColumnName = "Kurs", DataType = typeof(string) });
                dt.Columns.Add(new DataColumn() { ColumnName = "DPP Valas", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "DPP (IDR)", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "PPN (IDR)", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "PPH (IDR)", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });
            }
            else
            {
                dt.Columns.Add(new DataColumn() { ColumnName = "DPP", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "PPN", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "PPH", DataType = typeof(decimal) });
                dt.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });
            }

            return dt;
        }

        //public async Task<MemoryStream> GenerateExcel(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo, bool isValas)
        //{
        //    var result = await GetReport(no, unit, category, dateFrom, dateTo, isValas);
        //    //var Data = reportResult.Reports;
        //    var reportDataTable = GetFormatReportExcel(isValas);
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(string) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No PO", DataType = typeof(string) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(string) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Bon Penerimaan", DataType = typeof(string) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Faktur Pajak", DataType = typeof(string) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No SPB/NI", DataType = typeof(string) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(string) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "DPP", DataType = typeof(decimal) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "DPP Valas", DataType = typeof(decimal) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "PPN", DataType = typeof(decimal) });
        //    //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

        //    var categoryDataTable = new DataTable();
        //    categoryDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
        //    categoryDataTable.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });

        //    var currencyDataTable = new DataTable();
        //    currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
        //    currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

        //    if (result.Reports.Count > 0)
        //    {
        //        foreach (var report in result.Reports)
        //        {
        //            if (isValas)
        //            {
        //                reportDataTable.Rows.Add(report.ReceiptDate.ToString("MM/dd/yyyy"), report.SupplierName, report.ProductName, report.IPONo, report.DONo, report.URNNo, report.InvoiceNo, report.VATNo, report.UPONo, report.CategoryCode + " - " + report.CategoryName, report.UnitName, report.Quantity, report.Uom, report.CurrencyCode, report.CurrencyRate, report.DPP, report.DPPCurrency, report.VAT * report.CurrencyRate, report.IncomeTax * report.CurrencyRate, report.Total);
        //            }
        //            else
        //            {
        //                reportDataTable.Rows.Add(report.ReceiptDate.ToString("MM/dd/yyyy"), report.SupplierName, report.ProductName, report.IPONo, report.DONo, report.URNNo, report.InvoiceNo, report.VATNo, report.UPONo, report.CategoryCode + " - " + report.CategoryName, report.UnitName, report.Quantity, report.Uom, report.CurrencyCode, report.DPP, report.VAT, report.Total);
        //            }
        //        }
        //        foreach (var categorySummary in result.CategorySummaries)
        //            categoryDataTable.Rows.Add(categorySummary.Category, categorySummary.SubTotal);

        //        foreach (var currencySummary in result.CurrencySummaries)
        //            currencyDataTable.Rows.Add(currencySummary.CurrencyCode, currencySummary.SubTotal);
        //    }

        //    using (var package = new ExcelPackage())
        //    {
        //        var company = "PT DAN LIRIS";
        //        var title = "BUKU PEMBELIAN LOKAL";
        //        if (isValas)
        //            title = "BUKU PEMBELIAN LOKAL VALAS";
        //        var period = $"Dari {dateFrom.GetValueOrDefault().AddHours(_identityService.TimezoneOffset):MM/dd/yyyy} Sampai {dateTo.GetValueOrDefault().AddHours(_identityService.TimezoneOffset):MM/dd/yyyy}";

        //        var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
        //        worksheet.Cells["A1"].Value = company;
        //        worksheet.Cells["A2"].Value = title;
        //        worksheet.Cells["A3"].Value = period;
        //        worksheet.Cells["A4"].LoadFromDataTable(reportDataTable, true);
        //        worksheet.Cells[$"A{4 + 3 + result.Reports.Count}"].LoadFromDataTable(categoryDataTable, true);
        //        worksheet.Cells[$"A{4 + result.Reports.Count + 3 + result.CategorySummaries.Count + 3}"].LoadFromDataTable(currencyDataTable, true);

        //        var stream = new MemoryStream();
        //        package.SaveAs(stream);

        //        return stream;
        //    }
        //}

        public async Task<MemoryStream> GenerateExcel(string no, int accountingUnitId, int accountingCategoryId, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, bool isValas, int divisionId)
        {
            var result = await GetReportV2(no, accountingUnitId, accountingCategoryId, dateFrom, dateTo, inputDateFrom, inputDateTo, isValas, divisionId);
            //var Data = reportResult.Reports;
            var reportDataTable = GetFormatReportExcel(isValas);
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No PO", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Bon Penerimaan", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Faktur Pajak", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No SPB/NI", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "DPP", DataType = typeof(decimal) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "DPP Valas", DataType = typeof(decimal) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "PPN", DataType = typeof(decimal) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

            var categoryDataTable = new DataTable();
            categoryDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            categoryDataTable.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });

            var currencyDataTable = new DataTable();
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

            if (result.Reports.Count > 0)
            {
                foreach (var report in result.Reports)
                {
                    var dateReceipt = report.ReceiptDate.HasValue ? report.ReceiptDate.GetValueOrDefault().ToString("MM/dd/yyyy") : string.Empty;
                    var dateCorrection = report.CorrectionDate.HasValue ? report.ReceiptDate.GetValueOrDefault().ToString("MM/dd/yyyy") : string.Empty;
                    if (isValas)
                    {
                        reportDataTable.Rows.Add(dateReceipt, report.SupplierName, report.ProductName, report.IPONo, report.DONo, report.URNNo, report.InvoiceNo, report.VATNo, report.UPONo, report.CorrectionNo, dateCorrection, report.AccountingCategoryName, report.CategoryName, report.AccountingUnitName, report.UnitName, report.Quantity, report.Uom, report.CurrencyCode, report.CurrencyRate, report.DPP, report.DPP * report.CurrencyRate, report.VAT * report.CurrencyRate, report.IncomeTax * report.CurrencyRate, (report.DPP + report.VAT - report.IncomeTax) * report.CurrencyRate);
                    }
                    else
                    {
                        reportDataTable.Rows.Add(dateReceipt, report.SupplierName, report.ProductName, report.IPONo, report.DONo, report.URNNo, report.InvoiceNo, report.VATNo, report.UPONo, report.CorrectionNo, dateCorrection, report.AccountingCategoryName, report.CategoryName, report.AccountingUnitName, report.UnitName, report.Quantity, report.Uom, report.CurrencyCode, report.DPP, report.VAT, report.IncomeTax, report.Total);
                    }
                }
                foreach (var categorySummary in result.CategorySummaries)
                    categoryDataTable.Rows.Add(categorySummary.Category, categorySummary.SubTotal);

                foreach (var currencySummary in result.CurrencySummaries)
                    currencyDataTable.Rows.Add(currencySummary.CurrencyCode, currencySummary.SubTotal);
            }

            using (var package = new ExcelPackage())
            {
                var company = "PT DAN LIRIS";
                var title = "BUKU PEMBELIAN LOKAL";
                if (isValas)
                    title = "BUKU PEMBELIAN LOKAL VALAS";
                var period = $"Dari {dateFrom.GetValueOrDefault().AddHours(_identityService.TimezoneOffset):MM/dd/yyyy} Sampai {dateTo.GetValueOrDefault().AddHours(_identityService.TimezoneOffset):MM/dd/yyyy}";

                var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
                worksheet.Cells["A1"].Value = company;
                worksheet.Cells["A2"].Value = title;
                worksheet.Cells["A3"].Value = period;
                #region PrintHeader
                var rowStartHeader = 4;
                var colStartHeader = 1;

                foreach (var columns in reportDataTable.Columns)
                {
                    DataColumn column = (DataColumn)columns;
                    if (column.ColumnName == "DPP Valas")
                    {
                        var rowStartHeaderSpan = rowStartHeader + 1;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Value = column.ColumnName;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Style.Font.Bold = true;

                        worksheet.Cells[rowStartHeader, colStartHeader].Value = "Pembelian";
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 3].Merge = true;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 3].Style.Font.Bold = true;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 3].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }
                    else if (column.ColumnName == "DPP" && !isValas)
                    {
                        var rowStartHeaderSpan = rowStartHeader + 1;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Value = column.ColumnName;
                        worksheet.Cells[rowStartHeader, colStartHeader].Value = "Pembelian";
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 2].Merge = true;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 2].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 2].Style.Font.Bold = true;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    }
                    else if (column.ColumnName == "DPP (IDR)" || column.ColumnName == "PPN (IDR)" || column.ColumnName == "PPH (IDR)" || column.ColumnName == "PPN" || column.ColumnName == "PPH")
                    {
                        var rowStartHeaderSpan = rowStartHeader + 1;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Value = column.ColumnName;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Style.Font.Bold = true;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    }
                    else
                    {
                        worksheet.Cells[rowStartHeader, colStartHeader].Value = column.ColumnName;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader + 1, colStartHeader].Merge = true;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader + 1, colStartHeader].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader + 1, colStartHeader].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader + 1, colStartHeader].Style.Font.Bold = true;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader + 1, colStartHeader].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);



                    }
                    colStartHeader += 1;
                }
                #endregion
                //worksheet.Cells["A6"].LoadFromDataTable(reportDataTable, false,OfficeOpenXml.Table.TableStyles.Light18);
                worksheet.Cells["A6"].LoadFromDataTable(reportDataTable, false);
                for (int i = 6; i < result.Reports.Count + 6; i++)
                {
                    for (int j = 1; j <= reportDataTable.Columns.Count; j++)
                    {
                        worksheet.Cells[i, j].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }
                }
                //worksheet.Cells[4, 1, 6 + result.Reports.Count, reportDataTable.Columns.Count].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                worksheet.Cells[$"A{6 + 3 + result.Reports.Count}"].LoadFromDataTable(categoryDataTable, true, OfficeOpenXml.Table.TableStyles.Light18);
                worksheet.Cells[$"A{6 + result.Reports.Count + 3 + result.CategorySummaries.Count + 3}"].LoadFromDataTable(currencyDataTable, true, OfficeOpenXml.Table.TableStyles.Light18);

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return stream;
            }
        }


        public async Task<MemoryStream> GenerateExcelMII(string no, int accountingUnitId, int accountingCategoryId, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, bool isValas, int divisionId)
        {
            var result = await GetReportDataV2byDO(no, accountingUnitId, accountingCategoryId, dateFrom, dateTo, inputDateFrom, inputDateTo, isValas, divisionId);
            //var Data = reportResult.Reports;
            var reportDataTable = GetFormatReportExcelMII(isValas);
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Tanggal", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No PO", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Bon Penerimaan", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No Faktur Pajak", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "No SPB/NI", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(string) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "DPP", DataType = typeof(decimal) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "DPP Valas", DataType = typeof(decimal) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "PPN", DataType = typeof(decimal) });
            //reportDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

            var categoryDataTable = new DataTable();
            categoryDataTable.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(string) });
            categoryDataTable.Columns.Add(new DataColumn() { ColumnName = "Total (IDR)", DataType = typeof(decimal) });

            var currencyDataTable = new DataTable();
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(string) });
            currencyDataTable.Columns.Add(new DataColumn() { ColumnName = "Total", DataType = typeof(decimal) });

            if (result.Reports.Count > 0)
            {
                foreach (var report in result.Reports)
                {
                    var dateReceipt = report.ReceiptDate.HasValue ? report.ReceiptDate.GetValueOrDefault().ToString("MM/dd/yyyy") : string.Empty;
                    var dateCorrection = report.CorrectionDate.HasValue ? report.ReceiptDate.GetValueOrDefault().ToString("MM/dd/yyyy") : string.Empty;
                    if (isValas)
                    {
                        reportDataTable.Rows.Add(dateReceipt, report.SupplierName,/* report.ProductName, report.IPONo,*/ report.DONo, report.URNNo, report.InvoiceNo, report.VATNo, report.UPONo, report.CorrectionNo, dateCorrection, report.AccountingCategoryName, report.CategoryName, report.AccountingUnitName, report.UnitName, report.Quantity, report.Uom, report.CurrencyCode, report.CurrencyRate, report.DPP, report.DPP * report.CurrencyRate, report.VAT * report.CurrencyRate, report.IncomeTax * report.CurrencyRate, (report.DPP + report.VAT - report.IncomeTax) * report.CurrencyRate);
                    }
                    else
                    {
                        reportDataTable.Rows.Add(dateReceipt, report.SupplierName, /*report.ProductName, report.IPONo,*/ report.DONo, report.URNNo, report.InvoiceNo, report.VATNo, report.UPONo, report.CorrectionNo, dateCorrection, report.AccountingCategoryName, report.CategoryName, report.AccountingUnitName, report.UnitName, report.Quantity, report.Uom, report.CurrencyCode, report.DPP, report.VAT, report.IncomeTax, report.Total);
                    }
                }
                foreach (var categorySummary in result.CategorySummaries)
                    categoryDataTable.Rows.Add(categorySummary.Category, categorySummary.SubTotal);

                foreach (var currencySummary in result.CurrencySummaries)
                    currencyDataTable.Rows.Add(currencySummary.CurrencyCode, currencySummary.SubTotal);
            }

            using (var package = new ExcelPackage())
            {
                var company = "PT DAN LIRIS";
                var title = "BUKU PEMBELIAN LOKAL";
                if (isValas)
                    title = "BUKU PEMBELIAN LOKAL VALAS";
                var period = $"Dari {dateFrom.GetValueOrDefault().AddHours(_identityService.TimezoneOffset):MM/dd/yyyy} Sampai {dateTo.GetValueOrDefault().AddHours(_identityService.TimezoneOffset):MM/dd/yyyy}";

                var worksheet = package.Workbook.Worksheets.Add("Sheet 1");
                worksheet.Cells["A1"].Value = company;
                worksheet.Cells["A2"].Value = title;
                worksheet.Cells["A3"].Value = period;
                #region PrintHeader
                var rowStartHeader = 4;
                var colStartHeader = 1;

                foreach (var columns in reportDataTable.Columns)
                {
                    DataColumn column = (DataColumn)columns;
                    if (column.ColumnName == "DPP Valas")
                    {
                        var rowStartHeaderSpan = rowStartHeader + 1;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Value = column.ColumnName;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Style.Font.Bold = true;

                        worksheet.Cells[rowStartHeader, colStartHeader].Value = "Pembelian";
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 3].Merge = true;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 3].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 3].Style.Font.Bold = true;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 3].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }
                    else if (column.ColumnName == "DPP" && !isValas)
                    {
                        var rowStartHeaderSpan = rowStartHeader + 1;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Value = column.ColumnName;
                        worksheet.Cells[rowStartHeader, colStartHeader].Value = "Pembelian";
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 2].Merge = true;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 2].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 2].Style.Font.Bold = true;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader, colStartHeader + 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    }
                    else if (column.ColumnName == "DPP (IDR)" || column.ColumnName == "PPN (IDR)" || column.ColumnName == "PPH (IDR)" || column.ColumnName == "PPN" || column.ColumnName == "PPH")
                    {
                        var rowStartHeaderSpan = rowStartHeader + 1;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Value = column.ColumnName;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Style.Font.Bold = true;
                        worksheet.Cells[rowStartHeaderSpan, colStartHeader].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                    }
                    else
                    {
                        worksheet.Cells[rowStartHeader, colStartHeader].Value = column.ColumnName;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader + 1, colStartHeader].Merge = true;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader + 1, colStartHeader].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader + 1, colStartHeader].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader + 1, colStartHeader].Style.Font.Bold = true;
                        worksheet.Cells[rowStartHeader, colStartHeader, rowStartHeader + 1, colStartHeader].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);



                    }
                    colStartHeader += 1;
                }
                #endregion
                //worksheet.Cells["A6"].LoadFromDataTable(reportDataTable, false,OfficeOpenXml.Table.TableStyles.Light18);
                worksheet.Cells["A6"].LoadFromDataTable(reportDataTable, false);
                for (int i = 6; i < result.Reports.Count + 6; i++)
                {
                    for (int j = 1; j <= reportDataTable.Columns.Count; j++)
                    {
                        worksheet.Cells[i, j].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }
                }
                //worksheet.Cells[4, 1, 6 + result.Reports.Count, reportDataTable.Columns.Count].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                worksheet.Cells[$"A{6 + 3 + result.Reports.Count}"].LoadFromDataTable(categoryDataTable, true, OfficeOpenXml.Table.TableStyles.Light18);
                worksheet.Cells[$"A{6 + result.Reports.Count + 3 + result.CategorySummaries.Count + 3}"].LoadFromDataTable(currencyDataTable, true, OfficeOpenXml.Table.TableStyles.Light18);

                var stream = new MemoryStream();
                package.SaveAs(stream);

                return stream;
            }
        }
    }

    public interface ILocalPurchasingBookReportFacade
    {
        //Task<LocalPurchasingBookReportViewModel> GetReport(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo, bool isValas);
        //Task<LocalPurchasingBookReportViewModel> GetReport(string no, int accountingUnitId, int accountingCategoryId, DateTime? dateFrom, DateTime? dateTo, bool isValas, int divisionId);
        Task<LocalPurchasingBookReportViewModel> GetReportV2(string no, int accountingUnitId, int accountingCategoryId, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, bool isValas, int divisionId);

        //Task<MemoryStream> GenerateExcel(string no, string unit, string category, DateTime? dateFrom, DateTime? dateTo, bool isValas);
        Task<MemoryStream> GenerateExcel(string no, int accountingUnitId, int accountingCategoryId, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, bool isValas, int divisionId);

        Task<MemoryStream> GenerateExcelMII(string no, int accountingUnitId, int accountingCategoryId, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, bool isValas, int divisionId);
    }

    public class PreviousCorrection
    {
        public int UPODetailId { get; set; }
        public double TotalCorrection { get; set; }
        public double PricePerDealCorrection { get; set; }
    }
}