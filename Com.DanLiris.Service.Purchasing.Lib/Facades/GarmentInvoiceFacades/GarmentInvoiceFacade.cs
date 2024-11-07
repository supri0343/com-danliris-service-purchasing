using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.Services.GarmentDebtBalance;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Lib.Facades.LogHistoryFacade;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using static Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports.BCForAvalFacade;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using static iTextSharp.text.pdf.AcroFields;
using Org.BouncyCastle.Utilities;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInvoiceFacades
{
    public class GarmentInvoiceFacade : IGarmentInvoice
    {
        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentInvoice> dbSet;
        private readonly DbSet<GarmentDeliveryOrder> dbSetDeliveryOrder;
        public readonly IServiceProvider serviceProvider;
        private readonly IGarmentDebtBalanceService _garmentDebtBalanceService;
        private readonly IGarmentInternNoteFacade _facadeInternNote;
        private string USER_AGENT = "Facade";
        private readonly ILogHistoryFacades logHistoryFacades;

        public GarmentInvoiceFacade(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentInvoice>();
            this.dbSetDeliveryOrder = dbContext.Set<GarmentDeliveryOrder>();
            this.serviceProvider = serviceProvider;
            _garmentDebtBalanceService = serviceProvider.GetService<IGarmentDebtBalanceService>();
            _facadeInternNote = serviceProvider.GetService<IGarmentInternNoteFacade>();
            logHistoryFacades = serviceProvider.GetService<ILogHistoryFacades>();
        }
        public Tuple<List<GarmentInvoice>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentInvoice> Query = this.dbSet.Include(m => m.Items).ThenInclude(i => i.Details);

            List<string> searchAttributes = new List<string>()
            {
                "InvoiceNo", "SupplierName","Items.DeliveryOrderNo","NPN","NPH"
            };

            Query = QueryHelper<GarmentInvoice>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentInvoice>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentInvoice>.ConfigureOrder(Query, OrderDictionary);




            Pageable<GarmentInvoice> pageable = new Pageable<GarmentInvoice>(Query, Page - 1, Size);
            List<GarmentInvoice> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public Tuple<List<GarmentInvoiceIndexDto>, int, Dictionary<string, string>> ReadMerge(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentInvoice> Query = this.dbSet.Include(m => m.Items).ThenInclude(i => i.Details);

            List<string> searchAttributes = new List<string>()
            {
                "invoiceNo", "supplierName","items.deliveryOrderNo","npn","nph", "internNoteNo"
            };


            //var result = (from a in Query
            //              join b in dbContext.GarmentInternNoteItems on a.Id equals b.InvoiceId into r
            //              from b in r.DefaultIfEmpty()
            //              join c in dbContext.GarmentInternNotes on b.GarmentINId equals c.Id into s
            //              from c in s.DefaultIfEmpty()
            //              //where 
            //              //a.InvoiceNo.Contains(Keyword) && c.INNo.Contains(Keyword)
                          
            //              select new GarmentInvoiceIndexDto
            //              {
            //                  Id = a.Id,
            //                  LastModifiedUtc = a.LastModifiedUtc,
            //                  CreatedUtc = a.CreatedUtc,
            //                  invoiceNo = a.InvoiceNo,
            //                  invoiceDate = a.InvoiceDate,
            //                  internNoteNo = c.INNo,
            //                  internNoteId =c != null?c.Id : 0,
            //                  supplierName = a.SupplierName,
            //                  supplier = new SupplierViewModel
            //                  {
            //                      Code = a.SupplierCode,
            //                      Name = a.SupplierName
            //                  },
            //                  CreatedBy = a.CreatedBy,
            //                  npn = a.NPN,
            //                  nph = a.NPH,
            //                  items = a.Items.Select(x => new GarmentInvoiceItemIndexDto()
            //                  {

            //                      deliveryOrderId = x.DeliveryOrderId,
            //                      deliveryOrderNo = x.DeliveryOrderNo,
            //                      deliveryOrder = new GarmentDeliveryOrderViewModel(){ 
            //                        doNo = x.DeliveryOrderNo,
                                        
            //                      },
                                   
            //                  }).ToList()

            //              }).AsQueryable();

            var result = (from a in Query
                          join b in dbContext.GarmentInternNoteItems on a.Id equals b.InvoiceId into r
                          from b in r.DefaultIfEmpty()
                          join c in dbContext.GarmentInternNotes on b.GarmentINId equals c.Id into s
                          from c in s.DefaultIfEmpty()
                              // where kondisi dapat ditambahkan di sini jika perlu
                          select new
                          {
                              Id = a.Id,
                              LastModifiedUtc = a.LastModifiedUtc,
                              CreatedUtc = a.CreatedUtc,
                              InvoiceNo = a.InvoiceNo,
                              InvoiceDate = a.InvoiceDate,
                              InternNoteNo = c.INNo,
                              InternNoteId = c != null ? c.Id : 0,
                              SupplierName = a.SupplierName,
                              Supplier = new SupplierViewModel
                              {
                                  Code = a.SupplierCode,
                                  Name = a.SupplierName
                              },
                              CreatedBy = a.CreatedBy,
                              Npn = a.NPN,
                              Nph = a.NPH,
                             

                              Items = a.Items.Select(x => new GarmentInvoiceItemIndexDto
                              {
                                  deliveryOrderId = x.DeliveryOrderId,
                                  deliveryOrderNo = x.DeliveryOrderNo,
                                  deliveryOrder = new GarmentDeliveryOrderViewModel()
                                  {
                                      doNo = x.DeliveryOrderNo,

                                  },
                              }).ToList()
                          })
              .GroupBy(x => x.InvoiceNo) // Mengelompokkan berdasarkan InvoiceNo
              .Select(group => new GarmentInvoiceIndexDto
              {
                  Id = group.First().Id,
                  LastModifiedUtc = group.First().LastModifiedUtc,
                  CreatedUtc = group.First().CreatedUtc,
                  invoiceNo = group.Key, // Nilai GroupBy menjadi Key di sini
                  invoiceDate = group.First().InvoiceDate,
                  internNoteNo = group.First().InternNoteNo,
                  internNoteId = group.First().InternNoteId,
                  supplierName = group.First().SupplierName,
                  supplier = group.First().Supplier,
                  CreatedBy = group.First().CreatedBy,
                  npn = group.First().Npn,
                  nph = group.First().Nph,
                  //items = group.SelectMany(x => x.Items).ToList(),
                  //items = group.SelectMany(x => x.Items.Select( s => new GarmentInvoiceItemIndexDto
                  //                        { 
                  //                              deliveryOrderId = s.deliveryOrderId,
                  //                              deliveryOrderNo = s.deliveryOrderNo,
                  //                              deliveryOrder = new GarmentDeliveryOrderViewModel()
                  //                              { 
                  //                                  doNo = s.deliveryOrderNo
                  //                              }


                  //                        })
                  //                          .GroupBy(r => r.deliveryOrderId) // Mengelompokkan untuk menghapus duplikat berdasarkan DeliveryOrderId
                  //                          .Select(g => g.First()) // Memilih item pertama dari setiap grup untuk menghapus duplikat
                  //
                  //     .ToList()
                 // ).ToList()

                items = group.SelectMany(x => x.Items)
                               .GroupBy(i => i.deliveryOrderId) // Grupkan berdasarkan DeliveryOrderId
                               .Select(g => g.First()) // Ambil item pertama dari setiap grup untuk menghilangkan duplikat
                               .ToList()





                  // Gabungkan semua item dari setiap grup
              })
              .AsQueryable();


            if (Keyword != null) {
                result = result.Where(x =>
                                           x.invoiceNo.Contains(Keyword)
                                           || (x.CreatedBy != null && x.CreatedBy.Contains(Keyword))
                                           || (x.internNoteNo != null && x.internNoteNo.Contains(Keyword))  // Null check pada internNoteNo
                                           || (x.npn != null && x.npn.Contains(Keyword))  // Null check pada npn
                                           || (x.nph != null && x.nph.Contains(Keyword))  // Null check pada nph
                                           || (x.supplierName != null && x.supplierName.Contains(Keyword)) // Null check pada supplierName
                                           || x.items.Any(i => i.deliveryOrderNo != null && i.deliveryOrderNo.Contains(Keyword))
                                        );
                // result = result.Select(s => s.items.Where(r => r.deliveryOrderNo.Contains(Keyword)).ToList());

            }

                

            //result = QueryHelper<GarmentInvoiceIndexDto>.ConfigureSearch(result, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            result = QueryHelper<GarmentInvoiceIndexDto>.ConfigureFilter(result, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            result = QueryHelper<GarmentInvoiceIndexDto>.ConfigureOrder(result, OrderDictionary);

            Pageable<GarmentInvoiceIndexDto> pageable = new Pageable<GarmentInvoiceIndexDto>(result, Page - 1, Size);
            List<GarmentInvoiceIndexDto> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentInvoice ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                 .Include(m => m.Items)
                     .ThenInclude(i => i.Details)
                 .FirstOrDefault();
            return model;
        }

        public async Task<int> Create(GarmentInvoice model, string username, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    double _total = 0;
                    EntityExtension.FlagForCreate(model, username, USER_AGENT);
                    if (model.UseIncomeTax)
                    {
                        model.NPH = GenerateNPH();
                    }
                    if (model.UseVat)
                    {
                        model.NPN = GenerateNPN();
                    }
                    foreach (var item in model.Items)
                    {
                        _total += item.TotalAmount;
                        GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.DeliveryOrderId);
                        if (deliveryOrder != null)
                            deliveryOrder.IsInvoice = true;
                        EntityExtension.FlagForCreate(item, username, USER_AGENT);

                        foreach (var detail in item.Details)
                        {
                            EntityExtension.FlagForCreate(detail, username, USER_AGENT);
                        }
                    }
                    model.TotalAmount = _total;

                    this.dbSet.Add(model);

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Create Garment Invoice - " + model.InvoiceNo);

                    Created = await dbContext.SaveChangesAsync();

                    foreach (var item in model.Items)
                    {
                        var deliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.DeliveryOrderId);
                        if (deliveryOrder != null)
                        {
                            var amount = 0.0;
                            var currencyAmount = 0.0;
                            var vatAmount = 0.0;
                            var currencyVATAmount = 0.0;
                            var incomeTaxAmount = 0.0;
                            var currencyIncomeTaxAmount = 0.0;

                            if (model.CurrencyCode == "IDR")
                            {
                                amount = item.TotalAmount;
                                if (model.IsPayVat)
                                {
                                    vatAmount = item.TotalAmount * 0.1;
                                    //vatAmount = item.TotalAmount * (model.VatRate / 100);
                                }

                                if (model.IsPayTax)
                                {
                                    incomeTaxAmount = item.TotalAmount * model.IncomeTaxRate / 100;
                                }
                            }
                            else
                            {
                                amount = item.TotalAmount * deliveryOrder.DOCurrencyRate.GetValueOrDefault();
                                currencyAmount = item.TotalAmount;
                                if (model.IsPayVat)
                                {
                                    vatAmount = amount * 0.1;
                                    //vatAmount = amount * (model.VatRate / 100);
                                    currencyVATAmount = item.TotalAmount * 0.1;
                                    //currencyVATAmount = item.TotalAmount * (model.VatRate / 100);
                                }

                                if (model.IsPayTax)
                                {
                                    incomeTaxAmount = amount * model.IncomeTaxRate / 100;
                                    currencyIncomeTaxAmount = item.TotalAmount * model.IncomeTaxRate / 100;
                                }
                            }

                            await _garmentDebtBalanceService.UpdateFromInvoice((int)deliveryOrder.Id, new InvoiceFormDto((int)model.Id, model.InvoiceDate, model.InvoiceNo, amount, currencyAmount, vatAmount, incomeTaxAmount, model.IsPayVat, model.IsPayTax, currencyVATAmount, currencyIncomeTaxAmount, model.VatNo));
                        }
                    }

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


        public async Task<int> CreateMerge(GarmentInvoice model, GarmentInvoiceViewModel viewModel, string username, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    double _total = 0;
                    EntityExtension.FlagForCreate(model, username, USER_AGENT);
                    if (model.UseIncomeTax)
                    {
                        model.NPH = GenerateNPH();
                    }
                    if (model.UseVat)
                    {
                        model.NPN = GenerateNPN();
                    }
                    foreach (var item in model.Items)
                    {
                        _total += item.TotalAmount;
                        GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.DeliveryOrderId);
                        if (deliveryOrder != null)
                            deliveryOrder.IsInvoice = true;
                        EntityExtension.FlagForCreate(item, username, USER_AGENT);

                        foreach (var detail in item.Details)
                        {
                            EntityExtension.FlagForCreate(detail, username, USER_AGENT);
                        }
                    }
                    model.TotalAmount = _total;

                    this.dbSet.Add(model);

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Create Garment Invoice - " + model.InvoiceNo);

                    Created = await dbContext.SaveChangesAsync();

                    foreach (var item in model.Items)
                    {
                        var deliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.DeliveryOrderId);
                        if (deliveryOrder != null)
                        {
                            var amount = 0.0;
                            var currencyAmount = 0.0;
                            var vatAmount = 0.0;
                            var currencyVATAmount = 0.0;
                            var incomeTaxAmount = 0.0;
                            var currencyIncomeTaxAmount = 0.0;

                            if (model.CurrencyCode == "IDR")
                            {
                                amount = item.TotalAmount;
                                if (model.IsPayVat)
                                {
                                    vatAmount = item.TotalAmount * 0.1;
                                    //vatAmount = item.TotalAmount * (model.VatRate / 100);
                                }

                                if (model.IsPayTax)
                                {
                                    incomeTaxAmount = item.TotalAmount * model.IncomeTaxRate / 100;
                                }
                            }
                            else
                            {
                                amount = item.TotalAmount * deliveryOrder.DOCurrencyRate.GetValueOrDefault();
                                currencyAmount = item.TotalAmount;
                                if (model.IsPayVat)
                                {
                                    vatAmount = amount * 0.1;
                                    //vatAmount = amount * (model.VatRate / 100);
                                    currencyVATAmount = item.TotalAmount * 0.1;
                                    //currencyVATAmount = item.TotalAmount * (model.VatRate / 100);
                                }

                                if (model.IsPayTax)
                                {
                                    incomeTaxAmount = amount * model.IncomeTaxRate / 100;
                                    currencyIncomeTaxAmount = item.TotalAmount * model.IncomeTaxRate / 100;
                                }
                            }

                            await _garmentDebtBalanceService.UpdateFromInvoice((int)deliveryOrder.Id, new InvoiceFormDto((int)model.Id, model.InvoiceDate, model.InvoiceNo, amount, currencyAmount, vatAmount, incomeTaxAmount, model.IsPayVat, model.IsPayTax, currencyVATAmount, currencyIncomeTaxAmount, model.VatNo));
                        }
                    }


                    GarmentInternNote internNote = await MapToModelInvoice(model, viewModel);

                    await _facadeInternNote.CreateWithInvoice(internNote, viewModel.supplier.Import, username, 7);


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

        private string GenerateNPN()
        {
            string NPN = null;
            GarmentInvoice garmentInvoice = (from data in dbSet
                                             where data.NPN != null && data.NPN.StartsWith("NPN")
                                             orderby data.NPN descending
                                             select data).FirstOrDefault();
            string year = DateTime.Now.Year.ToString().Substring(2, 2);
            string month = DateTime.Now.Month.ToString("D2");
            string day = DateTime.Now.Day.ToString("D2");
            string formatDate = year + month;
            int counterId = 0;
            if (garmentInvoice != null)
            {
                NPN = garmentInvoice.NPN;
                string months = NPN.Substring(5, 2);
                string number = NPN.Substring(7);
                if (months == DateTime.Now.Month.ToString("D2"))
                {
                    counterId = Convert.ToInt32(number) + 1;
                }
                else
                {
                    counterId = 1;
                }
            }
            else
            {
                counterId = 1;
            }
            NPN = "NPN" + formatDate + counterId.ToString("D4");

            return NPN;
        }
        private string GenerateNPH()
        {
            string NPH = null;
            GarmentInvoice garmentInvoice = (from data in dbSet
                                             where data.NPH != null && data.NPH.StartsWith("NPH")
                                             orderby data.NPH descending
                                             select data).FirstOrDefault();
            string year = DateTime.Now.Year.ToString().Substring(2, 2);
            string month = DateTime.Now.Month.ToString("D2");
            string day = DateTime.Now.Day.ToString("D2");
            string formatDate = year + month;
            int counterId = 0;
            if (garmentInvoice != null)
            {
                NPH = garmentInvoice.NPH;
                string months = NPH.Substring(5, 2);
                string number = NPH.Substring(7);
                if (months == DateTime.Now.Month.ToString("D2"))
                {
                    counterId = Convert.ToInt32(number) + 1;
                }
                else
                {
                    counterId = 1;
                }
            }
            else
            {
                counterId = 1;
            }
            NPH = "NPH" + formatDate + counterId.ToString("D4");

            return NPH;
        }
        public int Delete(int id, string username)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var model = this.dbSet
                        .Include(d => d.Items)
                            .ThenInclude(d => d.Details)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    EntityExtension.FlagForDelete(model, username, USER_AGENT);

                    foreach (var item in model.Items)
                    {
                        GarmentDeliveryOrder garmentDeliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.DeliveryOrderId);
                        if (garmentDeliveryOrder != null)
                            garmentDeliveryOrder.IsInvoice = false;
                        EntityExtension.FlagForDelete(item, username, USER_AGENT);
                        var deleted = _garmentDebtBalanceService.EmptyInvoice((int)item.DeliveryOrderId).Result;
                        foreach (var detail in item.Details)
                        {
                            EntityExtension.FlagForDelete(detail, username, USER_AGENT);
                        }
                    }

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Delete Garment Invoice - " + model.InvoiceNo);

                    Deleted = dbContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Deleted;
        }


        public async Task<int> DeleteMerge(int id, string username)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var internNoteId = this.dbContext.GarmentInternNotes.Where(s => s.Items.Any(r => r.InvoiceId == id)).FirstOrDefault().Id;

                    await _facadeInternNote.DeleteMerge(Convert.ToInt32(internNoteId), username);
                    
                    var model = this.dbSet
                        .Include(d => d.Items)
                            .ThenInclude(d => d.Details)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    EntityExtension.FlagForDelete(model, username, USER_AGENT);

                    foreach (var item in model.Items)
                    {
                        GarmentDeliveryOrder garmentDeliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.DeliveryOrderId);
                        if (garmentDeliveryOrder != null)
                            garmentDeliveryOrder.IsInvoice = false;
                        EntityExtension.FlagForDelete(item, username, USER_AGENT);
                        var deleted = _garmentDebtBalanceService.EmptyInvoice((int)item.DeliveryOrderId).Result;
                        foreach (var detail in item.Details)
                        {
                            EntityExtension.FlagForDelete(detail, username, USER_AGENT);
                        }
                    }

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Delete Garment Invoice - " + model.InvoiceNo);

                    Deleted = dbContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Deleted;
        }

        public HashSet<long> GetGarmentInvoiceId(long id)
        {
            return new HashSet<long>(dbContext.GarmentInvoiceItems.Where(d => d.GarmentInvoice.Id == id).Select(d => d.Id));
        }
        public async Task<int> Update(int id, GarmentInvoice model, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (model.Items != null)
                    {
                        double total = 0;
                        HashSet<long> detailIds = GetGarmentInvoiceId(id);
                        foreach (var itemId in detailIds)
                        {
                            GarmentInvoiceItem data = model.Items.FirstOrDefault(prop => prop.Id.Equals(itemId));
                            if (data == null)
                            {
                                GarmentInvoiceItem dataItem = dbContext.GarmentInvoiceItems.FirstOrDefault(prop => prop.Id.Equals(itemId));
                                EntityExtension.FlagForDelete(dataItem, user, USER_AGENT);
                                var Details = dbContext.GarmentInvoiceDetails.Where(prop => prop.InvoiceItemId.Equals(itemId)).ToList();
                                GarmentDeliveryOrder deliveryOrder = dbContext.GarmentDeliveryOrders.FirstOrDefault(s => s.Id.Equals(dataItem.DeliveryOrderId));
                                deliveryOrder.IsInvoice = false;
                                foreach (GarmentInvoiceDetail detail in Details)
                                {

                                    EntityExtension.FlagForDelete(detail, user, USER_AGENT);
                                }

                                await _garmentDebtBalanceService.EmptyInvoice((int)dataItem.DeliveryOrderId);
                            }
                            else
                            {
                                EntityExtension.FlagForUpdate(data, user, USER_AGENT);
                            }

                            foreach (GarmentInvoiceItem item in model.Items)
                            {
                                total += item.TotalAmount;
                                if (item.Id <= 0)
                                {
                                    GarmentDeliveryOrder garmentDeliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.DeliveryOrderId);
                                    if (garmentDeliveryOrder != null)
                                        garmentDeliveryOrder.IsInvoice = true;
                                    EntityExtension.FlagForCreate(item, user, USER_AGENT);

                                    var deliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.DeliveryOrderId);
                                    if (deliveryOrder != null)
                                    {
                                        var amount = 0.0;
                                        var currencyAmount = 0.0;
                                        var vatAmount = 0.0;
                                        var currencyVATAmount = 0.0;
                                        var incomeTaxAmount = 0.0;
                                        var currencyIncomeTaxAmount = 0.0;

                                        if (model.CurrencyCode == "IDR")
                                        {
                                            amount = item.TotalAmount;
                                            if (model.IsPayVat)
                                            {
                                                vatAmount = item.TotalAmount * 0.1;
                                            }

                                            if (model.IsPayTax)
                                            {
                                                incomeTaxAmount = item.TotalAmount * model.IncomeTaxRate / 100;
                                            }
                                        }
                                        else
                                        {
                                            amount = item.TotalAmount * deliveryOrder.DOCurrencyRate.GetValueOrDefault();
                                            currencyAmount = item.TotalAmount;
                                            if (model.IsPayVat)
                                            {
                                                vatAmount = amount * 0.1;
                                                currencyVATAmount = item.TotalAmount * 0.1;
                                            }

                                            if (model.IsPayTax)
                                            {
                                                incomeTaxAmount = amount * model.IncomeTaxRate / 100;
                                                currencyIncomeTaxAmount = item.TotalAmount * model.IncomeTaxRate / 100;
                                            }
                                        }

                                        await _garmentDebtBalanceService.UpdateFromInvoice((int)deliveryOrder.Id, new InvoiceFormDto((int)model.Id, model.InvoiceDate, model.InvoiceNo, amount, currencyAmount, vatAmount, incomeTaxAmount, model.IsPayVat, model.IsPayTax, currencyVATAmount, currencyIncomeTaxAmount, model.VatNo));
                                    }
                                }
                                else
                                    EntityExtension.FlagForUpdate(item, user, USER_AGENT);

                                foreach (GarmentInvoiceDetail detail in item.Details)
                                {
                                    if (item.Id <= 0)
                                    {
                                        EntityExtension.FlagForCreate(detail, user, USER_AGENT);
                                    }
                                    else
                                        EntityExtension.FlagForUpdate(detail, user, USER_AGENT);
                                }
                            }
                        }
                    }
                    EntityExtension.FlagForUpdate(model, user, USER_AGENT);
                    this.dbSet.Update(model);

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Update Garment Invoice - " + model.InvoiceNo);

                    Updated = await dbContext.SaveChangesAsync();
                    transaction.Commit();

                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Updated;
        }

        public GarmentInvoice ReadByDOId(int id)
        {
            var model = dbSet.Where(m => m.Items.Any(i => i.DeliveryOrderId == id))
                 .Include(m => m.Items)
                     .ThenInclude(i => i.Details)
                 .FirstOrDefault();
            return model;
        }

        public List<GarmentInvoice> ReadForInternNote(List<long> garmentInvoiceIds)
        {
            var models = dbSet.Where(m => m.Items.Any(i => garmentInvoiceIds.Contains(m.Id)))
                .Select(m => new GarmentInvoice
                {
                    Id = m.Id,
                    Items = m.Items.Select(i => new GarmentInvoiceItem
                    {
                        Details = i.Details.Select(d => new GarmentInvoiceDetail
                        {
                            Id = d.Id,
                            DODetailId = d.DODetailId
                        }).ToList()
                    }).ToList()
                }).ToList();

            return models;
        }

        private async Task<GarmentInternNote> MapToModelInvoice(GarmentInvoice model, GarmentInvoiceViewModel viewModel)
        {
            var internNote = new GarmentInternNote
            {
                Active = model.Active,
            
                CreatedAgent = model.CreatedAgent,
                CreatedBy = model.CreatedBy,
                CreatedUtc = model.CreatedUtc,
                DeletedAgent = model.DeletedAgent,
                DeletedBy = model.DeletedBy,
                DeletedUtc = model.DeletedUtc,
                IsDeleted = model.IsDeleted,
                LastModifiedAgent = model.LastModifiedAgent,
                LastModifiedBy = model.LastModifiedBy,
                LastModifiedUtc = model.LastModifiedUtc,
                INNo = null,
                Remark = null,
                INDate = DateTimeOffset.Now,
                CurrencyId = model.CurrencyId,
                CurrencyCode = model.CurrencyCode,
                CurrencyRate = viewModel.currency.Rate,
                SupplierId = model.SupplierId,
                SupplierCode = model.SupplierCode,
                SupplierName = model.SupplierName,
                IsCreatedVB = false,
                IsPphPaid = model.IsPayTax,
                DPPVATIsPaid = model.IsPayVat,
                Items = model.Items.Select(i => new GarmentInternNoteItem
                {
                    Active = i.Active,
                    CreatedAgent = i.CreatedAgent,
                    CreatedBy = i.CreatedBy,
                    CreatedUtc = i.CreatedUtc,
                    DeletedAgent = i.DeletedAgent,
                    DeletedBy = i.DeletedBy,
                    DeletedUtc = i.DeletedUtc,
                    IsDeleted = i.IsDeleted,
                    LastModifiedAgent = i.LastModifiedAgent,
                    LastModifiedBy = i.LastModifiedBy,
                    LastModifiedUtc = i.LastModifiedUtc,
                    InvoiceId = i.InvoiceId,
                    InvoiceNo = model.InvoiceNo,
                    InvoiceDate = model.InvoiceDate,
                    TotalAmount = i.TotalAmount,
                    Details = i.Details.Select(d => new GarmentInternNoteDetail
                    {
                        Active = d.Active,
                        CreatedAgent = d.CreatedAgent,
                        CreatedBy = d.CreatedBy,
                        CreatedUtc = d.CreatedUtc,
                        DeletedAgent = d.DeletedAgent,
                        DeletedBy = d.DeletedBy,
                        DeletedUtc = d.DeletedUtc,
                        IsDeleted = d.IsDeleted,
                        LastModifiedAgent = d.LastModifiedAgent,
                        LastModifiedBy = d.LastModifiedBy,
                        LastModifiedUtc = d.LastModifiedUtc,
                        EPOId = d.EPOId,
                        EPONo = d.EPONo,
                        DOId = i.DeliveryOrderId,
                        DONo = i.DeliveryOrderNo,
                        POSerialNumber = d.POSerialNumber,
                        RONo = d.RONo,
                        PaymentMethod = i.PaymentMethod,
                        PaymentType = i.PaymentType,
                        PaymentDueDays = d.PaymentDueDays,
                        PaymentDueDate = i.DODate.AddDays(d.PaymentDueDays),
                        DODate = i.DODate,
                        ProductCode = d.ProductCode,
                        ProductId = d.ProductId,
                        ProductName = d.ProductName,
                        Quantity = d.DOQuantity,
                        UOMId = d.UomId,
                        UOMUnit = d.UomUnit,
                        PricePerDealUnit = d.PricePerDealUnit,
                        PriceTotal = d.PricePerDealUnit * d.DOQuantity,
                        InvoiceDetailId = Convert.ToInt32(d.Id)




                    }).ToList()



                }).ToList()
            };

            return internNote;

        }

        public string GetInternNoteNo(long invoiceId)
        {
            var internNote = dbContext.GarmentInternNotes.Where(x =>x.Items.Any(i => i.InvoiceId == invoiceId)).FirstOrDefault().INNo ;
            return internNote;
        }
    }
}
