
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentCorrectionNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentCorrectionNoteViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentCorrectionNoteFacades
{
    public class GarmentCorrectionNotePriceFacade : IGarmentCorrectionNotePriceFacade
    {
        private string USER_AGENT = "Facade";

        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentCorrectionNote> dbSet;
        private readonly ILogHistoryFacades logHistoryFacades;
        public GarmentCorrectionNotePriceFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));

            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentCorrectionNote>();
            logHistoryFacades = serviceProvider.GetService<ILogHistoryFacades>();
        }

        public Tuple<List<GarmentCorrectionNote>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentCorrectionNote> Query = dbSet;

            Query = Query.Where(m => (m.CorrectionType ?? "").ToUpper().StartsWith("HARGA"));

            Query = Query.Select(m => new GarmentCorrectionNote
            {
                Id = m.Id,
                CorrectionNo = m.CorrectionNo,
                CorrectionType = m.CorrectionType,
                CorrectionDate = m.CorrectionDate,
                SupplierName = m.SupplierName,
                DONo = m.DONo,
                UseIncomeTax = m.UseIncomeTax,
                UseVat = m.UseVat,
                CreatedBy = m.CreatedBy,
                LastModifiedUtc = m.LastModifiedUtc
            });

            List<string> searchAttributes = new List<string>()
            {
                "CorrectionNo", "CorrectionType", "SupplierName", "DONo"
            };

            Query = QueryHelper<GarmentCorrectionNote>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentCorrectionNote>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentCorrectionNote>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentCorrectionNote> pageable = new Pageable<GarmentCorrectionNote>(Query, Page - 1, Size);
            List<GarmentCorrectionNote> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentCorrectionNote ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                .FirstOrDefault();
            return model;
        }

        public async Task<int> Create(GarmentCorrectionNote garmentCorrectionNote)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(garmentCorrectionNote, identityService.Username, USER_AGENT);
                    var supplier = GetSupplier(garmentCorrectionNote.SupplierId);
                    garmentCorrectionNote.CorrectionNo = GenerateNo("NK", garmentCorrectionNote, supplier.Import ? "I" : "L");
                    if (garmentCorrectionNote.UseVat)
                    {
                        garmentCorrectionNote.NKPN = GenerateNKPN("NKPN", garmentCorrectionNote);
                    }
                    if (garmentCorrectionNote.UseIncomeTax)
                    {
                        garmentCorrectionNote.NKPH = GenerateNKPH("NKPH", garmentCorrectionNote);
                    }

                    if (((garmentCorrectionNote.CorrectionType ?? "").ToUpper() == "HARGA SATUAN"))
                    {
                        garmentCorrectionNote.TotalCorrection = garmentCorrectionNote.Items.Sum(i => (i.PricePerDealUnitAfter - i.PricePerDealUnitBefore) * i.Quantity);
                    }
                    else if ((garmentCorrectionNote.CorrectionType ?? "").ToUpper() == "HARGA TOTAL")
                    {
                        garmentCorrectionNote.TotalCorrection = garmentCorrectionNote.Items.Sum(i => i.PriceTotalAfter - i.PriceTotalBefore);
                    }

                    var garmentDeliveryOrder = dbContext.GarmentDeliveryOrders.First(d => d.Id == garmentCorrectionNote.DOId);
                    garmentDeliveryOrder.IsCorrection = true;
                    EntityExtension.FlagForUpdate(garmentDeliveryOrder, identityService.Username, USER_AGENT);

                    foreach (var item in garmentCorrectionNote.Items)
                    {
                        EntityExtension.FlagForCreate(item, identityService.Username, USER_AGENT);

                        var garmentDeliveryOrderDetail = dbContext.GarmentDeliveryOrderDetails.First(d => d.Id == item.DODetailId);
                        if ((garmentCorrectionNote.CorrectionType ?? "").ToUpper() == "HARGA SATUAN")
                        {
                            garmentDeliveryOrderDetail.PricePerDealUnitCorrection = (double)item.PricePerDealUnitAfter;
                            //garmentDeliveryOrderDetail.PriceTotalCorrection = (double)item.PriceTotalAfter;
                            garmentDeliveryOrderDetail.PriceTotalCorrection = (garmentDeliveryOrderDetail.QuantityCorrection - garmentDeliveryOrderDetail.ReturQuantity) * garmentDeliveryOrderDetail.PricePerDealUnitCorrection;

                        }
                        else if ((garmentCorrectionNote.CorrectionType ?? "").ToUpper() == "HARGA TOTAL")
                        {
                            garmentDeliveryOrderDetail.PriceTotalCorrection = (double)item.PriceTotalAfter;
                        }
                        EntityExtension.FlagForUpdate(garmentDeliveryOrderDetail, identityService.Username, USER_AGENT);
                    }

                    dbSet.Add(garmentCorrectionNote);

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Create Koreksi Harga Pembelian - " + garmentCorrectionNote.CorrectionNo);

                    Created = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Created;
        }

        private string GenerateNo(string code, GarmentCorrectionNote garmentCorrectionNote, string suffix = "")
        {
            string Year = garmentCorrectionNote.CorrectionDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("yy");
            string Month = garmentCorrectionNote.CorrectionDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("MM");

            string no = string.Concat(code, Year, Month);
            int Padding = 4;

            var lastNo = dbSet.Where(w => (w.CorrectionNo ?? "").StartsWith(no) && (w.CorrectionNo ?? "").EndsWith(suffix) && !w.IsDeleted).OrderByDescending(o => o.CorrectionNo).FirstOrDefaultAsync().Result;

            int lastNoNumber = 0;
            if (lastNo != null)
            {
                int.TryParse(lastNo.CorrectionNo.Substring(no.Length, lastNo.CorrectionNo.Length - no.Length - suffix.Length), out lastNoNumber);
            }
            return no + (lastNoNumber + 1).ToString().PadLeft(Padding, '0') + suffix;
        }

        private string GenerateNKPN(string code, GarmentCorrectionNote garmentCorrectionNote)
        {
            string Year = garmentCorrectionNote.CorrectionDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("yy");
            string Month = garmentCorrectionNote.CorrectionDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("MM");

            string no = string.Concat(code, Year, Month);
            int Padding = 4;

            var lastData = dbSet.Where(w => (w.NKPN ?? "").StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.NKPN).FirstOrDefaultAsync().Result;

            int lastNoNumber = 0;
            if (lastData != null)
            {
                int.TryParse(lastData.NKPN.Substring(no.Length, lastData.NKPN.Length - no.Length), out lastNoNumber);
            }
            return no + (lastNoNumber + 1).ToString().PadLeft(Padding, '0');
        }

        private string GenerateNKPH(string code, GarmentCorrectionNote garmentCorrectionNote)
        {
            string Year = garmentCorrectionNote.CorrectionDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("yy");
            string Month = garmentCorrectionNote.CorrectionDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("MM");

            string no = string.Concat(code, Year, Month);
            int Padding = 4;

            var lastData = dbSet.Where(w => (w.NKPH ?? "").StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.NKPH).FirstOrDefaultAsync().Result;

            int lastNoNumber = 0;
            if (lastData != null)
            {
                int.TryParse(lastData.NKPH.Substring(no.Length, lastData.NKPH.Length - no.Length), out lastNoNumber);
            }
            return no + (lastNoNumber + 1).ToString().PadLeft(Padding, '0');
        }

        private SupplierViewModel GetSupplier(long supplierId)
        {
            string supplierUri = "master/garment-suppliers";
            IHttpClientService httpClient = (IHttpClientService)serviceProvider.GetService(typeof(IHttpClientService));

            var response = httpClient.GetAsync($"{APIEndpoint.Core}{supplierUri}/{supplierId}").Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                SupplierViewModel viewModel = JsonConvert.DeserializeObject<SupplierViewModel>(result.GetValueOrDefault("data").ToString());
                return viewModel;
            }
            else
            {
                return null;
            }
        }

        //
        public IQueryable<GarmentCorrectionNoteGenerateDataViewModel> GetReportQuery(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;
            var Query1 = (from a in dbContext.GarmentCorrectionNotes 
                         join b in dbContext.GarmentCorrectionNoteItems on a.Id equals b.GCorrectionId
                         join c in dbContext.GarmentDeliveryOrders on a.DOId equals c.Id
                         join d in dbContext.GarmentExternalPurchaseOrders on b.EPOId equals d.Id
                         where a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false && d.IsDeleted == false &&
                               (a.CorrectionType == "Harga Total" || a.CorrectionType == "Harga Satuan") &&
                               a.CorrectionDate.AddHours(offset).Date >= DateFrom.Date && a.CorrectionDate.AddHours(offset).Date <= DateTo.Date

                         select new GarmentCorrectionNoteGenerateDataViewModel
                         {
                             CorrectionNo = a.CorrectionNo,
                             CorrectionDate = a.CorrectionDate,
                             CorrectionType = a.CorrectionType,
                             SupplierCode = a.SupplierCode,
                             SupplierName = a.SupplierName,
                             DONo = a.DONo,
                             DODate = c.DODate,
                             PaymentBill = c.PaymentBill,
                             BillNo = c.BillNo,
                             NKPN = a.NKPN == null ? "-" : a.NKPN,
                             DueDate = c.ArrivalDate.AddDays(d.PaymentDueDays),
                             PaymentDueDays = d.PaymentDueDays == 0 ? "D000" : (d.PaymentDueDays >= 1 && d.PaymentDueDays < 10 ? "D00" + d.PaymentDueDays.ToString() : (d.PaymentDueDays >= 10 && d.PaymentDueDays < 100 ? "D0" + d.PaymentDueDays.ToString() : "D" + d.PaymentDueDays.ToString())),                             
                             UseVat = a.UseVat ? "YA " : "TIDAK",
                             VatRate = a.VatRate,
                             UseIncomeTax = a.UseIncomeTax ? "YA " : "TIDAK",
                             IncomeTaxName = a.IncomeTaxName == null ? "-" : a.IncomeTaxName,
                             IncomeTaxRate = a.IncomeTaxRate,
                             CurrencyCode = c.DOCurrencyCode,
                             CurrencyRate = c.DOCurrencyRate,
                             Amount = b.PriceTotalAfter - b.PriceTotalBefore,
                         }
                         );

            var Query2 = (from a in dbContext.GarmentCorrectionNotes
                          join b in dbContext.GarmentCorrectionNoteItems on a.Id equals b.GCorrectionId
                          join c in dbContext.GarmentDeliveryOrders on a.DOId equals c.Id
                          join d in dbContext.GarmentExternalPurchaseOrders on b.EPOId equals d.Id
                          where a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false && d.IsDeleted == false &&
                                (a.CorrectionType == "Retur" || a.CorrectionType == "Jumlah") &&
                                a.CorrectionDate.AddHours(offset).Date >= DateFrom.Date && a.CorrectionDate.AddHours(offset).Date <= DateTo.Date

                          select new GarmentCorrectionNoteGenerateDataViewModel
                          {
                              CorrectionNo = a.CorrectionNo,
                              CorrectionDate = a.CorrectionDate,
                              CorrectionType = a.CorrectionType,
                              SupplierCode = a.SupplierCode,
                              SupplierName = a.SupplierName,
                              DONo = a.DONo,
                              DODate = c.DODate,
                              PaymentBill = c.PaymentBill,
                              BillNo = c.BillNo,
                              NKPN = a.NKPN == null ? "-" : a.NKPN,
                              DueDate = c.ArrivalDate.AddDays(d.PaymentDueDays),
                              PaymentDueDays = d.PaymentDueDays == 0 ? "D000" : (d.PaymentDueDays >= 1 && d.PaymentDueDays < 10 ? "D00" + d.PaymentDueDays.ToString() : (d.PaymentDueDays >= 10 && d.PaymentDueDays < 100 ? "D0" + d.PaymentDueDays.ToString() : "D" + d.PaymentDueDays.ToString())),
                              UseVat = a.UseVat ? "YA " : "TIDAK",
                              VatRate = a.VatRate,
                              UseIncomeTax = a.UseIncomeTax ? "YA " : "TIDAK",
                              IncomeTaxName = a.IncomeTaxName == null ? "-" : a.IncomeTaxName,
                              IncomeTaxRate = a.IncomeTaxRate,
                              CurrencyCode = c.DOCurrencyCode,
                              CurrencyRate = c.DOCurrencyRate,
                              Amount = b.Quantity * b.PricePerDealUnitAfter,
                          }
                         );

            //

            List<GarmentCorrectionNoteGenerateDataViewModel> CombineData = Query1.Union(Query2).ToList();

            //
            var Query = from a in CombineData

                        group new { TotalAmount = a.Amount } by new
                         {
                             a.CorrectionNo,
                             a.CorrectionDate,
                             a.CorrectionType,
                             a.SupplierCode,
                             a.SupplierName,
                             a.DONo,
                             a.DODate,
                             a.PaymentBill,
                             a.BillNo,
                             a.DueDate,
                             a.PaymentDueDays,
                             a.UseVat,
                             a.VatRate,
                             a.UseIncomeTax,
                             a.IncomeTaxName,
                             a.IncomeTaxRate,
                             a.CurrencyCode,
                             a.CurrencyRate,
                         } into G

                         select new GarmentCorrectionNoteGenerateDataViewModel
                         {
                             CorrectionNo = G.Key.CorrectionNo,
                             CorrectionDate = G.Key.CorrectionDate,
                             CorrectionType = G.Key.CorrectionType,
                             SupplierCode = G.Key.SupplierCode,
                             SupplierName = G.Key.SupplierName,
                             DONo = G.Key.DONo,
                             DODate = G.Key.DODate,
                             PaymentBill = G.Key.PaymentBill,
                             BillNo = G.Key.BillNo,
                             DueDate = G.Key.DueDate,
                             PaymentDueDays = G.Key.PaymentDueDays,
                             UseVat = G.Key.UseVat,
                             VatRate = G.Key.VatRate,
                             UseIncomeTax = G.Key.UseIncomeTax,
                             IncomeTaxName = G.Key.IncomeTaxName,
                             IncomeTaxRate = G.Key.IncomeTaxRate,
                             CurrencyCode = G.Key.CurrencyCode,
                             CurrencyRate = G.Key.CurrencyRate,
                             Amount = Math.Round(G.Sum(m => m.TotalAmount), 2),
                         };
            return Query.AsQueryable();
        }

        public MemoryStream GenerateDataExcel(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetReportQuery(dateFrom, dateTo, offset);
            Query = Query.OrderBy(b => b.CorrectionNo).ThenBy(b => b.DONo);
            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "NOMOR NOTA KOREKSI", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL NOTA KOREKSI", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "JENIS KOREKSI", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "KODE SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NO SURAT JALAN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL SURAT JALAN", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "NO BON KECIL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NO BON PUSAT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL JATUH TEMPO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TEMPO", DataType = typeof(String) });
          
            result.Columns.Add(new DataColumn() { ColumnName = "PPN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "% PPN", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "PPH", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "JENIS PAJAK", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "% PPH", DataType = typeof(Double) });

            result.Columns.Add(new DataColumn() { ColumnName = "MATA UANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "RATE", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "JUMLAH NOMINAL", DataType = typeof(Double) });

            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 0, "", 0, 0); // to allow column name to be generated properly for empty data as template
            else
            {
                var index = 0;
                foreach (var item in Query)
                {
                    index++;
                    string CDate = item.CorrectionDate == new DateTime(1970, 1, 1) ? "-" : item.CorrectionDate.GetValueOrDefault().ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd/MM/yyyy", new CultureInfo("id-ID"));
                    string DODate = item.DODate == null ? "-" : item.DODate.GetValueOrDefault().ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd/MM/yyyy", new CultureInfo("id-ID"));
                    string DueDate = item.DueDate == new DateTime(1970, 1, 1) ? "-" : item.DueDate.GetValueOrDefault().ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd/MM/yyyy", new CultureInfo("id-ID"));
                    
                    result.Rows.Add(item.CorrectionNo, CDate, item.CorrectionType, item.SupplierCode, item.SupplierName, item.DONo, DODate, item.PaymentBill, item.BillNo, DueDate,
                                    item.PaymentDueDays, item.UseVat, item.VatRate, item.UseIncomeTax, item.IncomeTaxName, item.IncomeTaxRate, item.CurrencyCode, item.CurrencyRate, item.Amount);
                }
            }
            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Sheet1") }, true);
        }
    }
}
