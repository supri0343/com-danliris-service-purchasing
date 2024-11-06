using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Facades.LogHistoryFacade;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternalPurchaseOrderViewModel;
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
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades
{
    public class GarmentDeliveryOrderFacade : IGarmentDeliveryOrderFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentDeliveryOrder> dbSet;
        //private readonly DbSet<GarmentDeliveryOrderItem> dbSetItem;
        private readonly ILogHistoryFacades logHistoryFacades;
        private readonly IMapper mapper;

        public GarmentDeliveryOrderFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentDeliveryOrder>();
            this.serviceProvider = serviceProvider;
            logHistoryFacades = serviceProvider.GetService<ILogHistoryFacades>();
            mapper = serviceProvider == null ? null : (IMapper)serviceProvider.GetService(typeof(IMapper));
        }

        public Tuple<List<GarmentDeliveryOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            //IQueryable<GarmentDeliveryOrder> Query = this.dbSet.Include(m => m.Items);
            IQueryable<GarmentDeliveryOrder> Query = this.dbSet.AsNoTracking().Include(x => x.Items).ThenInclude(x => x.Details);

            List<string> searchAttributes = new List<string>()
            {
                "DONo", "BillNo", "PaymentBill","SupplierName", "Items.EPONo"
            };

            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

            Query = Query.Select(x => new GarmentDeliveryOrder
            {
                Id = x.Id,
                DONo = x.DONo,
                DODate = x.DODate,
                ArrivalDate = x.ArrivalDate,
                BillNo = x.BillNo,
                PaymentBill = x.PaymentBill,
                SupplierId = x.SupplierId,
                SupplierCode = x.SupplierCode,
                SupplierName = x.SupplierName,
                CreatedBy = x.CreatedBy,
                IsClosed = x.IsClosed,
                IsCustoms = x.IsCustoms,
                IsInvoice = x.IsInvoice,
                IsSubconInvoice = x.IsSubconInvoice,
                LastModifiedUtc = x.LastModifiedUtc,
                Items = x.Items.Select(y => new GarmentDeliveryOrderItem
                {
                    Id = y.Id,
                    EPOId = y.EPOId,
                    EPONo = y.EPONo,
                    CurrencyId = y.CurrencyId,
                    CurrencyCode = y.CurrencyCode,
                    PaymentDueDays = y.PaymentDueDays,
                    Details = y.Details.Select(z => new GarmentDeliveryOrderDetail
                    {
                        Id = z.Id,
                        DOQuantity = z.DOQuantity,
                        ProductCode = z.ProductCode,
                        ProductId = z.ProductId,
                        ProductName = z.ProductName,
                        ProductRemark = z.ProductRemark,
                        SmallQuantity = z.SmallQuantity,
                        SmallUomId = z.SmallUomId,
                        SmallUomUnit = z.SmallUomUnit,
                        PricePerDealUnit = z.PricePerDealUnit,
                        PriceTotal = z.PriceTotal
                    }),
                }),

            });

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentDeliveryOrder> pageable = new Pageable<GarmentDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentDeliveryOrder> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public ReadResponse<dynamic> ReadLoader(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}", string Select = "{}", string Search = "[]")
        {
            IQueryable<GarmentDeliveryOrder> Query = dbSet;

            List<string> SearchAttributes = JsonConvert.DeserializeObject<List<string>>(Search);
            if (SearchAttributes.Count.Equals(0))
            {
                SearchAttributes = new List<string>() { "DONo" };
            }
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureSearch(Query, SearchAttributes, Keyword, SearchWith: "StartsWith");

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Dictionary<string, string> SelectDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Select);
            var SelectedQuery = QueryHelper<GarmentDeliveryOrder>.ConfigureSelect(Query, SelectDictionary);

            int TotalData = SelectedQuery.Count();

            List<dynamic> Data = SelectedQuery
                .Skip((Page - 1) * Size)
                .Take(Size)
                .ToDynamicList();

            return new ReadResponse<dynamic>(Data, TotalData, OrderDictionary);
        }

        public GarmentDeliveryOrder ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                    .ThenInclude(i => i.Details)
                .FirstOrDefault();
            return model;
        }
        //
        public IQueryable<GarmentDOUrnViewModel> GetDataDOQuery(int id)
        {
            var Query = from a in dbContext.GarmentDeliveryOrders
                        join b in dbContext.GarmentBeacukais on a.CustomsId equals b.Id
                        join c in dbContext.GarmentUnitReceiptNotes on a.Id equals c.DOId
                        where a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false && a.Id == id
                        select new GarmentDOUrnViewModel
                        {
                            DOId = a.Id,
                            DONo = a.DONo,
                            BCNo = b.BeacukaiNo,
                            BCType = b.CustomsType,
                            URNNo = c.URNNo,
                        };

            return Query.AsQueryable();
        }
        //
        public List<GarmentDOUrnViewModel> GetDataDO(int id)
        {
            var Query = GetDataDOQuery(id);

            return Query.ToList();
        }
        //

        public List<GarmentDeliveryOrder> ReadForInternNote(List<long> deliveryOrderIds)
        {
            var models = dbSet.Where(m => deliveryOrderIds.Contains(m.Id))
                .Select(m => new GarmentDeliveryOrder
                {
                    Id = m.Id,
                    Items = m.Items.Select(i => new GarmentDeliveryOrderItem
                    {
                        Details = i.Details.Select(d => new GarmentDeliveryOrderDetail
                        {
                            Id = d.Id,
                            ReceiptQuantity = d.ReceiptQuantity
                        }).ToList()
                    }).ToList()
                }).ToList();

            return models;
        }

        public async Task<int> Create(GarmentDeliveryOrder m, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, USER_AGENT);

                    m.IsClosed = false;
                    m.IsCorrection = false;
                    m.IsCustoms = false;

                    foreach (var item in m.Items)
                    {
                        EntityExtension.FlagForCreate(item, user, USER_AGENT);

                        CurrencyViewModel garmentCurrencyViewModel = GetCurrency(item.CurrencyCode, m.DODate);
                        m.DOCurrencyId = garmentCurrencyViewModel.Id;
                        m.DOCurrencyCode = garmentCurrencyViewModel.Code;
                        m.DOCurrencyRate = garmentCurrencyViewModel.Rate;

                        foreach (var detail in item.Details)
                        {
                            GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(detail.POId));
                            GarmentInternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(s => s.GPOId.Equals(internalPurchaseOrder.Id));

                            detail.POItemId = (int)internalPurchaseOrderItem.Id;
                            detail.PRItemId = internalPurchaseOrderItem.GPRItemId;
                            detail.UnitId = internalPurchaseOrder.UnitId;
                            detail.UnitCode = internalPurchaseOrder.UnitCode;
                            detail.ProductRemark = internalPurchaseOrderItem.ProductRemark;
                            EntityExtension.FlagForCreate(detail, user, USER_AGENT);

                            GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(detail.EPOItemId));
                            externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity + detail.DOQuantity;

                            if (externalPurchaseOrderItem.ReceiptQuantity == 0)
                            {
                                if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity < externalPurchaseOrderItem.DealQuantity)
                                {
                                    internalPurchaseOrderItem.Status = "Barang sudah datang parsial";
                                }
                                else if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity >= externalPurchaseOrderItem.DealQuantity)
                                {
                                    internalPurchaseOrderItem.Status = "Barang sudah datang semua";
                                }
                            }

                            detail.QuantityCorrection = detail.DOQuantity;
                            detail.PricePerDealUnitCorrection = detail.PricePerDealUnit;
                            detail.PriceTotalCorrection = detail.PriceTotal;

                            m.TotalAmount += detail.PriceTotal;

                        }
                    }

                    this.dbSet.Add(m);

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Create Surat Jalan - " + m.DONo);

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

        public async Task<int> Update(int id, GarmentDeliveryOrderViewModel vm, GarmentDeliveryOrder m, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldM = this.dbSet.AsNoTracking()
                        .Include(d => d.Items)
                            .ThenInclude(d => d.Details)
                               .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    if (oldM != null && oldM.Id == id)
                    {
                        EntityExtension.FlagForUpdate(m, user, USER_AGENT);
                        m.TotalAmount = 0;
                        foreach (var vmItem in vm.items)
                        {

                            foreach (var modelItem in m.Items.Where(i => i.Id == vmItem.Id))
                            {
                                if (modelItem.Id == 0)
                                {
                                    EntityExtension.FlagForCreate(modelItem, user, USER_AGENT);

                                    CurrencyViewModel garmentCurrencyViewModel = GetCurrency(modelItem.CurrencyCode, m.DODate);
                                    m.DOCurrencyId = garmentCurrencyViewModel.Id;
                                    m.DOCurrencyCode = garmentCurrencyViewModel.Code;
                                    m.DOCurrencyRate = garmentCurrencyViewModel.Rate;
                                    foreach (var vmDetail in vmItem.fulfillments)
                                    {
                                        foreach (var modelDetail in modelItem.Details.Where(j => j.POId == vmDetail.pOId))
                                        {
                                            if (vmDetail.isSave)
                                            {
                                                GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(modelDetail.POId));
                                                GarmentInternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(s => s.GPOId.Equals(internalPurchaseOrder.Id));

                                                modelDetail.POItemId = (int)internalPurchaseOrderItem.Id;
                                                modelDetail.PRItemId = internalPurchaseOrderItem.GPRItemId;
                                                modelDetail.UnitId = internalPurchaseOrder.UnitId;
                                                modelDetail.UnitCode = internalPurchaseOrder.UnitCode;
                                                EntityExtension.FlagForCreate(modelDetail, user, USER_AGENT);

                                                GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(modelDetail.EPOItemId));
                                                externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity + modelDetail.DOQuantity;

                                                if (externalPurchaseOrderItem.ReceiptQuantity == 0)
                                                {
                                                    if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity < externalPurchaseOrderItem.DealQuantity)
                                                    {
                                                        internalPurchaseOrderItem.Status = "Barang sudah datang parsial";
                                                    }
                                                    else if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity >= externalPurchaseOrderItem.DealQuantity)
                                                    {
                                                        internalPurchaseOrderItem.Status = "Barang sudah datang semua";
                                                    }
                                                }

                                                modelDetail.QuantityCorrection = modelDetail.DOQuantity;
                                                modelDetail.PricePerDealUnitCorrection = modelDetail.PricePerDealUnit;
                                                modelDetail.PriceTotalCorrection = modelDetail.PriceTotal;

                                                m.TotalAmount += modelDetail.PriceTotal;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var item in oldM.Items.Where(i => i.EPOId == modelItem.EPOId).ToList())
                                    {
                                        EntityExtension.FlagForUpdate(modelItem, user, USER_AGENT);

                                        CurrencyViewModel garmentCurrencyViewModel = GetCurrency(item.CurrencyCode, m.DODate);
                                        m.DOCurrencyId = garmentCurrencyViewModel.Id;
                                        m.DOCurrencyCode = garmentCurrencyViewModel.Code;
                                        m.DOCurrencyRate = garmentCurrencyViewModel.Rate;

                                        foreach (var vmDetail in vmItem.fulfillments)
                                        {
                                            foreach (var modelDetail in modelItem.Details.Where(j => j.Id == vmDetail.Id))
                                            {
                                                foreach (var detail in item.Details.Where(j => j.EPOItemId == modelDetail.EPOItemId).ToList())
                                                {
                                                    GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(modelDetail.POId));
                                                    GarmentInternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(s => s.GPOId.Equals(modelDetail.POId));
                                                    GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(modelDetail.EPOItemId));

                                                    if (vmDetail.isSave == false)
                                                    {
                                                        externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity - detail.DOQuantity;
                                                        EntityExtension.FlagForDelete(modelDetail, user, USER_AGENT);
                                                    }
                                                    else
                                                    {
                                                        externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity - detail.DOQuantity + modelDetail.DOQuantity;
                                                        modelDetail.POItemId = (int)internalPurchaseOrderItem.Id;
                                                        modelDetail.PRItemId = internalPurchaseOrderItem.GPRItemId;
                                                        modelDetail.UnitId = internalPurchaseOrder.UnitId;
                                                        modelDetail.UnitCode = internalPurchaseOrder.UnitCode;

                                                        modelDetail.QuantityCorrection = modelDetail.DOQuantity;
                                                        modelDetail.PricePerDealUnitCorrection = modelDetail.PricePerDealUnit;
                                                        modelDetail.PriceTotalCorrection = modelDetail.PriceTotal;
                                                        m.TotalAmount += modelDetail.PriceTotal;

                                                        EntityExtension.FlagForUpdate(modelDetail, user, USER_AGENT);
                                                    }
                                                    if (externalPurchaseOrderItem.ReceiptQuantity == 0)
                                                    {
                                                        if (externalPurchaseOrderItem.DOQuantity == 0)
                                                        {
                                                            GarmentPurchaseRequestItem purchaseRequestItem = this.dbContext.GarmentPurchaseRequestItems.FirstOrDefault(s => s.Id.Equals(modelDetail.PRItemId));
                                                            purchaseRequestItem.Status = "Sudah diorder ke Supplier";
                                                            internalPurchaseOrderItem.Status = "Sudah diorder ke Supplier";
                                                        }
                                                        else if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity < externalPurchaseOrderItem.DealQuantity)
                                                        {
                                                            internalPurchaseOrderItem.Status = "Barang sudah datang parsial";
                                                        }
                                                        else if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity >= externalPurchaseOrderItem.DealQuantity)
                                                        {
                                                            internalPurchaseOrderItem.Status = "Barang sudah datang semua";
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        dbSet.Update(m);

                        foreach (var oldItem in oldM.Items)
                        {
                            var newItem = m.Items.FirstOrDefault(i => i.EPOId.Equals(oldItem.EPOId));
                            foreach (var oldDetail in oldItem.Details)
                            {
                                GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(oldDetail.EPOItemId));
                                if (newItem == null)
                                {
                                    EntityExtension.FlagForDelete(oldItem, user, USER_AGENT);
                                    dbContext.GarmentDeliveryOrderItems.Update(oldItem);
                                    EntityExtension.FlagForDelete(oldDetail, user, USER_AGENT);
                                    externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity - oldDetail.DOQuantity;
                                    dbContext.GarmentDeliveryOrderDetails.Update(oldDetail);
                                }
                            }
                        }

                        //Create Log History
                        logHistoryFacades.Create("PEMBELIAN", "Update Surat Jalan - " + m.DONo);

                        Updated = await dbContext.SaveChangesAsync();
                        transaction.Commit();
                    }
                    else
                    {
                        throw new Exception("Invalid Id");
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }

            return Updated;
        }

        public async Task<int> Delete(int id, string user)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var model = this.dbSet
                                .Include(m => m.Items)
                                .ThenInclude(i => i.Details)
                                .SingleOrDefault(m => m.Id == id && !m.IsDeleted);

                    EntityExtension.FlagForDelete(model, user, USER_AGENT);
                    foreach (var item in model.Items)
                    {
                        EntityExtension.FlagForDelete(item, user, USER_AGENT);

                        foreach (var detail in item.Details)
                        {
                            GarmentExternalPurchaseOrderItem externalPurchaseOrderItem = this.dbContext.GarmentExternalPurchaseOrderItems.FirstOrDefault(s => s.Id.Equals(detail.EPOItemId));
                            GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(detail.POId));
                            GarmentInternalPurchaseOrderItem internalPurchaseOrderItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(s => s.GPOId.Equals(detail.POId));

                            GarmentDeliveryOrderDetail deliveryOrderDetail = this.dbContext.GarmentDeliveryOrderDetails.FirstOrDefault(s => s.Id.Equals(detail.Id));
                            externalPurchaseOrderItem.DOQuantity = externalPurchaseOrderItem.DOQuantity - detail.DOQuantity;

                            if (externalPurchaseOrderItem.ReceiptQuantity == 0)
                            {
                                if (externalPurchaseOrderItem.DOQuantity == 0)
                                {
                                    GarmentPurchaseRequestItem purchaseRequestItem = this.dbContext.GarmentPurchaseRequestItems.FirstOrDefault(s => s.Id.Equals(detail.PRItemId));
                                    purchaseRequestItem.Status = "Sudah diorder ke Supplier";
                                    internalPurchaseOrderItem.Status = "Sudah diorder ke Supplier";
                                }
                                else if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity < externalPurchaseOrderItem.DealQuantity)
                                {
                                    internalPurchaseOrderItem.Status = "Barang sudah datang parsial";
                                }
                                else if (externalPurchaseOrderItem.DOQuantity > 0 && externalPurchaseOrderItem.DOQuantity >= externalPurchaseOrderItem.DealQuantity)
                                {
                                    internalPurchaseOrderItem.Status = "Barang sudah datang Semua";
                                }
                            }

                            EntityExtension.FlagForDelete(detail, user, USER_AGENT);
                        }
                    }

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Delete Surat Jalan - " + model.DONo);

                    Deleted = await dbContext.SaveChangesAsync();

                    await dbContext.SaveChangesAsync();
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

        public IQueryable<GarmentDeliveryOrder> ReadBySupplier(string Keyword, string Filter)
        {
            IQueryable<GarmentDeliveryOrder> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "DONo"
            };

            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword); // kalo search setelah Select dengan .Where setelahnya maka case sensitive, kalo tanpa .Where tidak masalah
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);
            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>("{}");

            if (OrderDictionary.Count > 0 && OrderDictionary.Keys.First().Contains("."))
            {
                string Key = OrderDictionary.Keys.First();
                string SubKey = Key.Split(".")[1];
                string OrderType = OrderDictionary[Key];

                Query = Query.Include(m => m.Items)
                    .ThenInclude(i => i.Details);
            }
            else
            {

                Query = QueryHelper<GarmentDeliveryOrder>.ConfigureOrder(Query, OrderDictionary).Include(m => m.Items)
                    .ThenInclude(i => i.Details).Where(s => s.IsInvoice == false && s.CustomsId != 0);
            }

            foreach (var data in Query)
            {
                data.TotalAmount = 0;
                foreach (var item in data.Items)
                {
                    foreach (var detail in item.Details)
                    {
                        data.TotalAmount += detail.DOQuantity * detail.PricePerDealUnit;
                    }
                }
            }

            return Query;
        }

        public IQueryable<GarmentDeliveryOrder> DOForCustoms(string Keyword, string Filter, string BillNo = null)
        {
            IQueryable<GarmentDeliveryOrder> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "DONo"
            };

            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword); // kalo search setelah Select dengan .Where setelahnya maka case sensitive, kalo tanpa .Where tidak masalah
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);
            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>("{}");

            //if (OrderDictionary.Count > 0 && OrderDictionary.Keys.First().Contains("."))
            //{
            //	string Key = OrderDictionary.Keys.First();
            //	string SubKey = Key.Split(".")[1];
            //	string OrderType = OrderDictionary[Key];

            //	Query = Query.Include(m => m.Items)
            //		.ThenInclude(i => i.Details);
            //}
            //else
            //{

            var DOCurrencyCodes = dbSet.Where(w => w.BillNo == BillNo).Select(s => s.DOCurrencyCode);
            var SupplierIds = dbSet.Where(w => w.BillNo == BillNo).Select(s => s.SupplierId);

            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureOrder(Query, OrderDictionary).Include(m => m.Items)
                .ThenInclude(i => i.Details)
                .Where(s => s.CustomsId == 0
                    && (DOCurrencyCodes.Count() == 0 || DOCurrencyCodes.Contains(s.DOCurrencyCode))
                    && (SupplierIds.Count() == 0 || SupplierIds.Contains(s.SupplierId))
                    );
            //}

            return Query;
        }

        public int IsReceived(List<int> id)
        {
            int isReceived = 0;
            foreach (var no in id)
            {
                var model = dbSet.Where(m => m.Id == no)
                               .Include(m => m.Items)
                                   .ThenInclude(i => i.Details)
                               .FirstOrDefault();
                if (model.IsInvoice == true)
                {
                    isReceived = 1;
                    break;
                }
                else
                {
                    foreach (var item in model.Items)
                    {
                        foreach (var detail in item.Details)
                        {
                            if (detail.ReceiptQuantity > 0)
                                isReceived = 1;
                            break;
                        }
                    }
                }
            }

            return isReceived;
        }

        public async Task<int> SetIsSubconInvoice(List<string> DONos, bool isSubconInvoice)
        {
            int isUpdate = 0;
            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var doNo in DONos)
                    {
                        var model = dbSet.Where(m => m.DONo == doNo).FirstOrDefault();
                        model.IsSubconInvoice = isSubconInvoice;
                    }
                    isUpdate = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw new Exception(e.Message);
                }
            }
            return isUpdate;
        }

        private CurrencyViewModel GetCurrency(string currencyCode, DateTimeOffset doDate)
        {
            string currencyUri = "master/garment-currencies/byCode";
            IHttpClientService httpClient = (IHttpClientService)serviceProvider.GetService(typeof(IHttpClientService));

            var response = httpClient.GetAsync($"{APIEndpoint.Core}{currencyUri}/{currencyCode}").Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                List<CurrencyViewModel> viewModel = JsonConvert.DeserializeObject<List<CurrencyViewModel>>(result.GetValueOrDefault("data").ToString());
                return viewModel.OrderByDescending(s => s.Date).FirstOrDefault(s => s.Date < doDate.AddDays(1)); ;
            }
            else
            {
                return null;
            }
        }

        public ReadResponse<object> ReadForUnitReceiptNote(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            long filterSupplierId = FilterDictionary.ContainsKey("SupplierId") ? long.Parse(FilterDictionary["SupplierId"]) : 0;
            FilterDictionary.Remove("SupplierId");

            var filterUnitId = FilterDictionary.ContainsKey("UnitId") ? FilterDictionary["UnitId"] : string.Empty;
            FilterDictionary.Remove("UnitId");

            IQueryable<GarmentDeliveryOrder> Query = dbSet
                .Where(m => m.DONo.Contains(Keyword ?? "") && (filterSupplierId == 0 ? true : m.SupplierId == filterSupplierId) && m.CustomsId != 0 && m.Items.Any(i => i.Details.Any(d => d.ReceiptQuantity == 0 && (string.IsNullOrWhiteSpace(filterUnitId) ? true : d.UnitId == filterUnitId))))
                .Select(m => new GarmentDeliveryOrder
                {
                    Id = m.Id,
                    DONo = m.DONo,
                    LastModifiedUtc = m.LastModifiedUtc,
                    Items = m.Items.Select(i => new GarmentDeliveryOrderItem
                    {
                        Id = i.Id,
                        EPOId = i.EPOId,
                        Details = i.Details.Where(d => d.ReceiptQuantity == 0 && (string.IsNullOrWhiteSpace(filterUnitId) ? true : d.UnitId == filterUnitId)).ToList()
                    }).ToList()
                });

            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentDeliveryOrder> pageable = new Pageable<GarmentDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentDeliveryOrder> DataModel = pageable.Data.ToList();
            int Total = pageable.TotalCount;

            List<GarmentDeliveryOrderViewModel> DataViewModel = mapper.Map<List<GarmentDeliveryOrderViewModel>>(DataModel);

            List<dynamic> listData = new List<dynamic>();
            listData.AddRange(
                DataViewModel.Select(s => new
                {
                    s.Id,
                    s.doNo,
                    s.LastModifiedUtc,
                    items = s.items.Select(i => new
                    {
                        i.Id,
                        paymentType = dbContext.GarmentExternalPurchaseOrders.Where(m => m.Id == i.ePOId).Select(m => m.PaymentType).FirstOrDefault(),
                        paymentMethod = dbContext.GarmentExternalPurchaseOrders.Where(m => m.Id == i.ePOId).Select(m => m.PaymentMethod).FirstOrDefault(),
                        fulfillments = i.fulfillments.Select(d => new
                        {
                            d.Id,

                            d.ePOItemId,

                            d.pRId,
                            d.pRNo,
                            d.pRItemId,

                            d.pOId,
                            d.pOItemId,
                            d.poSerialNumber,

                            d.product,
                            productRemark = dbContext.GarmentExternalPurchaseOrderItems.Where(m => m.Id == d.ePOItemId).Select(m => m.Remark).FirstOrDefault(),

                            d.rONo,

                            d.doQuantity,
                            d.receiptQuantity,

                            d.purchaseOrderUom,

                            d.pricePerDealUnit,
                            d.pricePerDealUnitCorrection,

                            d.conversion,

                            d.smallUom,

                            buyer = new
                            {
                                name = dbContext.GarmentPurchaseRequests.Where(m => m.Id == d.pRId).Select(m => m.BuyerName).FirstOrDefault()
                            },
                            article = dbContext.GarmentExternalPurchaseOrderItems.Where(m => m.Id == d.ePOItemId).Select(m => m.Article).FirstOrDefault()
                        }).ToList()
                    }).ToList()
                }).ToList()
            );

            return new ReadResponse<object>(listData, Total, OrderDictionary);
        }

        public ReadResponse<object> ReadForCorrectionNoteQuantity(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);


            IQueryable<GarmentDeliveryOrder> Query = dbSet;
            List<string> searchAttributes = new List<string>()
            {
                "DONo"
            };

            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Query = Query
                .Where(m => m.CustomsId != 0 && m.Items.Any(i => i.Details.Any(d => d.ReceiptQuantity > 0)))
                .Select(m => new GarmentDeliveryOrder
                {
                    Id = m.Id,
                    DONo = m.DONo,
                    BillNo = m.BillNo,
                    IsInvoice = m.IsInvoice,
                    UseIncomeTax = m.UseIncomeTax,
                    SupplierName = m.SupplierName,
                    SupplierId = m.SupplierId,
                    SupplierCode = m.SupplierCode,
                    DOCurrencyId = m.DOCurrencyId,
                    DOCurrencyCode = m.DOCurrencyCode,
                    UseVat = m.UseVat,
                    VatId = m.VatId,
                    VatRate = m.VatRate,
                    IncomeTaxId = m.IncomeTaxId,
                    IncomeTaxName = m.IncomeTaxName,
                    IncomeTaxRate = m.IncomeTaxRate,
                    LastModifiedUtc = m.LastModifiedUtc,
                    DODate = m.DODate,
                    Items = m.Items.Select(i => new GarmentDeliveryOrderItem
                    {
                        Id = i.Id,
                        EPOId = i.EPOId,
                        EPONo = i.EPONo,
                        Details = i.Details.Where(d => d.ReceiptQuantity > 0).ToList()
                    }).ToList()
                });

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentDeliveryOrder> pageable = new Pageable<GarmentDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentDeliveryOrder> DataModel = pageable.Data.ToList();
            int Total = pageable.TotalCount;

            List<GarmentDeliveryOrderViewModel> DataViewModel = mapper.Map<List<GarmentDeliveryOrderViewModel>>(DataModel);

            List<dynamic> listData = new List<dynamic>();
            listData.AddRange(
                DataViewModel.Select(s => new
                {
                    s.Id,
                    s.doNo,
                    s.docurrency,
                    s.supplier,
                    s.useIncomeTax,
                    s.billNo,
                    s.isInvoice,
                    s.useVat,
                    s.incomeTax,
                    s.vat,
                    s.LastModifiedUtc,
                    s.doDate,
                    items = s.items.Select(i => new
                    {
                        i.Id,
                        i.purchaseOrderExternal,
                        fulfillments = i.fulfillments.Select(d => new
                        {
                            d.Id,

                            d.pRId,
                            d.pRNo,

                            d.pOId,
                            d.poSerialNumber,

                            d.product,

                            d.rONo,
                            d.doQuantity,
                            d.purchaseOrderUom,
                            d.quantityCorrection,
                            d.priceTotalCorrection,

                            d.pricePerDealUnit,
                            d.pricePerDealUnitCorrection,

                            d.returQuantity,
                            receiptCorrection = dbContext.GarmentUnitReceiptNoteItems.Where(m => m.DODetailId == d.Id && m.IsDeleted == false).Select(m => m.ReceiptCorrection).FirstOrDefault()
                        }).ToList()
                    }).ToList()
                }).ToList()
            );
            return new ReadResponse<object>(listData, Total, OrderDictionary);
        }

        public IQueryable<AccuracyOfArrivalReportViewModel> GetReportQuery(string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;
            //List<string> Category = null;
            //List<string> Product = null;
            var Status = new[] { "" };
            var Supplier = new[] { "MADEIRA", "MARATHON" };

            //switch (category)
            //{
            //    case "BB":
            //        Status = new[] { "FABRIC", "INTERLINING" };
            //        break;
            //    case "BP":
            //        Status = new[] { "FABRIC", "INTERLINING", "PLISKET", "PRINT", "QUILTING", "WASH", "EMBROIDERY", "PROCESS" };
            //        break;
            //    default:
            //        Status = new[] { "" };
            //        break;
            //}
            // if (category == "")
            // {
            //  var categoryAll = "[\"BB\",\"BP\"]";
            //  Category = JsonConvert.DeserializeObject<List<string>>(categoryAll);
            // }

            List<AccuracyOfArrivalReportViewModel> listAccuracyOfArrival = new List<AccuracyOfArrivalReportViewModel>();

            var Query = (from a in dbContext.GarmentDeliveryOrders
                         join b in dbContext.GarmentDeliveryOrderItems on a.Id equals b.GarmentDOId
                         join c in dbContext.GarmentDeliveryOrderDetails on b.Id equals c.GarmentDOItemId
                         //join d in dbContext.GarmentInternalPurchaseOrders on c.POId equals d.Id
                         //join e in dbContext.GarmentInternalPurchaseOrderItems on d.Id equals e.GPOId
                         join f in dbContext.GarmentPurchaseRequests on c.PRId equals f.Id
                         //join g in dbContext.GarmentPurchaseRequestItems on f.Id equals g.GarmentPRId
                         //join h in dbContext.GarmentExternalPurchaseOrders on b.EPOId equals h.Id
                         //join i in dbContext.GarmentExternalPurchaseOrderItems on h.Id equals i.GarmentEPOId
                         where a.IsDeleted == false
                               && b.IsDeleted == false
                               && f.IsDeleted == false
                               && c.IsDeleted == false
                               && ((DateFrom != new DateTime(1970, 1, 1)) ? (a.DODate.Date >= DateFrom && a.DODate.Date <= DateTo) : true)
                               //&& (category == "BB" ? Status.Contains(c.ProductName) : (category == "BP" ? !Status.Contains(c.ProductName) || Supplier.Contains(a.SupplierName) : c.ProductName == c.ProductName))
                               && (category == "BB" ? c.CodeRequirment == "BB" : (category == "BP" ? c.CodeRequirment == "BP" : (c.CodeRequirment == "BB" || c.CodeRequirment == "BP")))
                               && !c.RONo.EndsWith("S")
                         //  && (category == "" ? Category.Contains(c.CodeRequirment) : c.CodeRequirment==category)
                         select new AccuracyOfArrivalReportViewModel
                         {
                             supplier = new SupplierViewModel
                             {
                                 Code = a.SupplierCode,
                                 Id = a.SupplierId,
                                 Name = a.SupplierName
                             },
                             //poSerialNumber = c.POSerialNumber,
                             //prDate = f.Date,    //distinct garmentdodetailid
                             //poDate = d.CreatedUtc,
                             //epoDate = h.OrderDate,
                             //product = new GarmentProductViewModel
                             //{
                             //    Code = c.ProductCode,
                             //    Id = c.ProductId,
                             //    Name = c.ProductName,
                             //    Remark = c.ProductRemark,
                             //},
                             //article = i.Article,
                             //roNo = c.RONo,
                             shipmentDate = f.ShipmentDate,
                             doDate = a.DODate,
                             //staff = h.CreatedBy,
                             category = category,
                             doNo = a.DONo,
                             ok_notOk = "NOT OK",
                             //LastModifiedUtc = i.LastModifiedUtc
                         }).Distinct();

            Query = Query.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.doDate);
            var suppTemp = "";
            var percentOK = 0;
            var percentNotOk = 0;
            var jumlah = 0;

            foreach (var item in Query)
            {
                var ShipmentDate = new DateTimeOffset(item.shipmentDate.Date, TimeSpan.Zero);
                var DODate = new DateTimeOffset(item.doDate.Date, TimeSpan.Zero);
                var jumlahOk = 0;
                var datediff = ((TimeSpan)(ShipmentDate - DODate)).Days;

                if (item.category == "BB")
                {
                    if (datediff >= 27)
                    {
                        item.ok_notOk = "OK";
                    }
                }
                else if (item.category == "BP")
                {
                    if (datediff >= 20)
                    {
                        item.ok_notOk = "OK";
                    }
                }
                if (suppTemp == "")
                {
                    jumlah += 1;
                    suppTemp = item.supplier.Code;
                    if (item.ok_notOk == "OK")
                    {
                        percentOK += 1;
                    }
                    else
                    {
                        percentNotOk += 1;
                    }

                }
                else if (suppTemp == item.supplier.Code)
                {
                    jumlah += 1;
                    if (item.ok_notOk == "OK")
                    {
                        percentOK += 1;
                    }
                    else
                    {
                        percentNotOk += 1;
                    }
                }
                else if (suppTemp != item.supplier.Code)
                {
                    var perOk = 0;
                    var perNotOk = 0;
                    suppTemp = item.supplier.Code;
                    jumlah = 1;
                    jumlahOk = perOk;
                    if (item.ok_notOk == "OK")
                    {
                        percentOK = perOk + 1;
                        percentNotOk = perNotOk;
                    }
                    else
                    {
                        percentNotOk = perNotOk + 1;
                        percentOK = perOk;
                    }
                }
                jumlahOk = percentOK + percentNotOk;
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = item.supplier,
                    poSerialNumber = item.poSerialNumber,
                    prDate = item.prDate,
                    poDate = item.poDate,
                    epoDate = item.epoDate,
                    product = item.product,
                    article = item.article,
                    roNo = item.roNo,
                    shipmentDate = item.shipmentDate,
                    doDate = item.doDate,
                    staff = item.staff,
                    category = item.category,
                    doNo = item.doNo,
                    ok_notOk = item.ok_notOk,
                    percentOk_notOk = (percentOK * 100) / jumlah,
                    jumlahOk = percentOK,
                    jumlah = jumlah,
                    dateDiff = datediff,
                    LastModifiedUtc = item.LastModifiedUtc
                };
                listAccuracyOfArrival.Add(_new);
            }
            return listAccuracyOfArrival.Distinct().OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.doDate).AsQueryable();
        }

        public Tuple<List<AccuracyOfArrivalReportViewModel>, int> GetReportHeaderAccuracyofArrival(string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var ctg = "";
            if (category == "Bahan Baku")
            {
                ctg = "BB";
            }
            else if (category == "Bahan Pendukung")
            {
                ctg = "BP";
            }

            var QuerySupplier = GetReportQuery(ctg, dateFrom, dateTo, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();
            List<AccuracyOfArrivalReportViewModel> Data2 = new List<AccuracyOfArrivalReportViewModel>();

            var SuppTemp = "";
            foreach (var item in QuerySupplier.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.jumlah))
            {
                if (SuppTemp == "" || SuppTemp != item.supplier.Code)
                {
                    SuppTemp = item.supplier.Code;

                    AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                    {
                        supplier = item.supplier,
                        poSerialNumber = item.poSerialNumber,
                        prDate = item.prDate,
                        poDate = item.poDate,
                        epoDate = item.epoDate,
                        product = item.product,
                        article = item.article,
                        roNo = item.roNo,
                        shipmentDate = item.shipmentDate,
                        doDate = item.doDate,
                        staff = item.staff,
                        category = item.category,
                        doNo = item.doNo,
                        ok_notOk = item.ok_notOk,
                        percentOk_notOk = item.percentOk_notOk,
                        jumlah = item.jumlah,
                        jumlahOk = item.jumlahOk,
                        dateDiff = item.dateDiff,
                        LastModifiedUtc = item.LastModifiedUtc
                    };
                    Data.Add(_new);
                }
            }
            foreach (var items in Data.OrderBy(b => b.percentOk_notOk).ThenByDescending(b => b.jumlah))
            {
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = items.supplier,
                    poSerialNumber = items.poSerialNumber,
                    prDate = items.prDate,
                    poDate = items.poDate,
                    epoDate = items.epoDate,
                    product = items.product,
                    article = items.article,
                    roNo = items.roNo,
                    shipmentDate = items.shipmentDate,
                    doDate = items.doDate,
                    staff = items.staff,
                    category = items.category,
                    doNo = items.doNo,
                    ok_notOk = items.ok_notOk,
                    percentOk_notOk = items.percentOk_notOk,
                    jumlah = items.jumlah,
                    jumlahOk = items.jumlahOk,
                    dateDiff = items.dateDiff,
                    LastModifiedUtc = items.LastModifiedUtc
                };
                Data2.Add(_new);
            }
            return Tuple.Create(Data2, Data.Count);
        }

        public AccuracyOfArrivalReportHeaderResult GetAccuracyOfArrivalHeader(string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            //var endDate = dateTo ?? DateTime.Now;
            //var startDate = dateFrom ?? endDate.AddDays(-30);

            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            if (category == "Bahan Baku")
            {
                category = "BB";
            }
            else if (category == "Bahan Pendukung")
            {
                category = "BP";
            }

            //if (endDate < startDate)
            //    throw new Exception("date range is invalid");

            //var Status = ""; //new[] { "" };
            var Supplier = new[] { "MADEIRA", "MARATHON" };

            var selectedGarmentDeliveryOrders = dbContext.GarmentDeliveryOrders.Where(w => w.ArrivalDate.AddHours(7).Date >= dateFrom && w.ArrivalDate.AddHours(7).Date <= dateTo).Select(s => new SelectedGarmentDeliveryOrder(s)).ToList();
            var reportResult = new List<AccuracyOfArrivalReportHeader>();
            //switch (category)
            //{
            //    case "BB":
            //        Status = new[] { "FABRIC", "INTERLINING" };
            //        reportResult = GetBBResult(selectedGarmentDeliveryOrders, Status);
            //        break;
            //    case "BP":
            //        Status = new[] { "FABRIC", "INTERLINING", "PLISKET", "PRINT", "QUILTING", "WASH", "EMBROIDERY", "PROCESS" };
            //        reportResult = GetBPResult(selectedGarmentDeliveryOrders, Status, Supplier);
            //        break;
            //    default:
            //        reportResult = GetDefaultResult(selectedGarmentDeliveryOrders);
            //        break;
            //}

            switch (category)
            {
                case "BB":
                    reportResult = GetBBResult(selectedGarmentDeliveryOrders);
                    break;
                case "BP":
                    reportResult = GetBPResult(selectedGarmentDeliveryOrders);
                    break;
                default:
                    reportResult = GetDefaultResult(selectedGarmentDeliveryOrders);
                    break;
            }

            reportResult = reportResult.Where(w => w.Total > 0).ToList();
            reportResult = reportResult.OrderBy(o => o.OKStatusPercentage).ThenByDescending(t => t.Total).ToList();

            return new AccuracyOfArrivalReportHeaderResult()
            {
                ReportHeader = reportResult,
                Total = reportResult.Sum(s => s.Total)
            };
        }

        private List<AccuracyOfArrivalReportHeader> GetDefaultResult(List<SelectedGarmentDeliveryOrder> selectedGarmentDeliveryOrders)
        {
            var selectedSupplierIds = selectedGarmentDeliveryOrders.Select(s => s.SupplierId).Distinct().ToList();
            var garmentDeliveryOrderIds = selectedGarmentDeliveryOrders.Select(s => s.GarmentDeliveryOrderId).ToList();
            var garmentDeliveryOrderItems = dbContext.GarmentDeliveryOrderItems.Where(w => garmentDeliveryOrderIds.Contains(w.GarmentDOId)).Select(s => new { s.Id, s.GarmentDOId }).ToList();
            var garmentDeliveryOrderItemIds = garmentDeliveryOrderItems.Select(s => s.Id).ToList();
            var garmentDeliveryOrderDetails = dbContext.GarmentDeliveryOrderDetails.Where(w => !w.RONo.EndsWith("S") && garmentDeliveryOrderItemIds.Contains(w.GarmentDOItemId)).Select(s => new { s.Id, s.PRId, s.GarmentDOItemId }).ToList();

            var purchaseRequestIds = garmentDeliveryOrderDetails.Select(s => s.PRId).ToList();
            var purchaseRequests = dbContext.GarmentPurchaseRequests.Where(w => purchaseRequestIds.Contains(w.Id)).Select(s => new { s.Id, s.ShipmentDate }).ToList();

            var result = new List<AccuracyOfArrivalReportHeader>();
            var index = 0;
            foreach (var selectedSupplierId in selectedSupplierIds)
            {
                index++;
                //var se
                var selectedDeliveryIds = selectedGarmentDeliveryOrders.Where(w => w.SupplierId == selectedSupplierId).Select(s => s.GarmentDeliveryOrderId).ToList();
                var selectedDeliveryItemIds = garmentDeliveryOrderItems.Where(w => selectedDeliveryIds.Contains(w.GarmentDOId)).Select(s => s.Id).ToList();
                var selectedGarmentDeliveryOrderDetails = garmentDeliveryOrderDetails.Where(w => selectedDeliveryItemIds.Contains(w.GarmentDOItemId)).ToList();

                //var selectedPurchaseRequestIds = selectedGarmentDeliveryOrderDetails.Select(s => s.PRId).ToList();

                //var selectedPurchaseRequests = purchaseRequests.Where(w => selectedPurchaseRequestIds.Contains(w.Id)).ToList();
                var resultToCount = (from garmentDODetail in selectedGarmentDeliveryOrderDetails
                                     join garmentDOItem in garmentDeliveryOrderItems on garmentDODetail.GarmentDOItemId equals garmentDOItem.Id
                                     join garmentDO in selectedGarmentDeliveryOrders on garmentDOItem.GarmentDOId equals garmentDO.GarmentDeliveryOrderId
                                     join purchaseRequest in purchaseRequests on garmentDODetail.PRId equals purchaseRequest.Id

                                     select new
                                     {
                                         garmentDO.DODate,
                                         purchaseRequest.ShipmentDate
                                     }).ToList();

                var total = resultToCount.Count;
                var okPercentage = 0;

                var selectedSupplier = selectedGarmentDeliveryOrders.FirstOrDefault(f => f.SupplierId == selectedSupplierId);

                var datum = new AccuracyOfArrivalReportHeader
                {
                    No = index,
                    SupplierId = selectedSupplierId,
                    SupplierCode = selectedSupplier.SupplierCode,
                    SupplierName = selectedSupplier.SupplierName,
                    //OKStatusPercentage = Math.Floor((double)okPercentage),
                    OKStatusPercentage = okPercentage,
                    OKTotal = 0,
                    Total = total
                };

                result.Add(datum);

            }

            return result;
        }

        private List<AccuracyOfArrivalReportHeader> GetBPResult(List<SelectedGarmentDeliveryOrder> selectedGarmentDeliveryOrders)
        {
            //var bpGarmentDOIds = selectedGarmentDeliveryOrders.Select(s => s.GarmentDeliveryOrderId).ToList();
            ////var garment

            //var selectedSupplierIds = selectedGarmentDeliveryOrders.Select(s => s.SupplierId).Distinct().ToList();
            //var garmentDeliveryOrderIds = selectedGarmentDeliveryOrders.Select(s => s.GarmentDeliveryOrderId).ToList();
            //var garmentDeliveryOrderItems = dbContext.GarmentDeliveryOrderItems.Where(w => garmentDeliveryOrderIds.Contains(w.GarmentDOId) || bpGarmentDOIds.Contains(w.GarmentDOId)).Select(s => new { s.Id, s.GarmentDOId }).ToList();
            //var garmentDeliveryOrderItemIds = garmentDeliveryOrderItems.Select(s => s.Id).ToList();
            ////var garmentDeliveryOrderDetails = dbContext.GarmentDeliveryOrderDetails.Where(w => w.CodeRequirment == "BP" && !w.RONo.EndsWith("S") && garmentDeliveryOrderItemIds.Contains(w.GarmentDOItemId)).Select(s => new SelectedGarmentDeliveryOrderDetail() { Id = s.Id, PRId = s.PRId, GarmentDOItemId = s.GarmentDOItemId }).ToList();
            //var garmentDeliveryOrderDetails = dbContext.GarmentDeliveryOrderDetails.Where(w => w.CodeRequirment == "BP" && !w.RONo.EndsWith("S") && garmentDeliveryOrderItemIds.Contains(w.GarmentDOItemId)).Select(s => new { s.Id, s.PRId, s.GarmentDOItemId }).ToList();

            ////var bpGarmentDeliveryOrderItemIds = garmentDeliveryOrderItems.Where(w => bpGarmentDOIds.Contains(w.GarmentDOId)).Select(s => s.Id).ToList();
            ////var bpGarmentDeliveryOrderDetails = dbContext.GarmentDeliveryOrderDetails.Where(w => bpGarmentDeliveryOrderItemIds.Contains(w.GarmentDOItemId)).Select(s => new SelectedGarmentDeliveryOrderDetail() { Id = s.Id, PRId = s.PRId, GarmentDOItemId = s.GarmentDOItemId }).ToList();

            ////garmentDeliveryOrderDetails.AddRange(bpGarmentDeliveryOrderDetails);
            ////garmentDeliveryOrderDetails = garmentDeliveryOrderDetails.Distinct().ToList();

            //var purchaseRequestIds = garmentDeliveryOrderDetails.Select(s => s.PRId).ToList();
            //var purchaseRequests = dbContext.GarmentPurchaseRequests.Where(w => purchaseRequestIds.Contains(w.Id)).Select(s => new { s.Id, s.ShipmentDate }).ToList();

            //var result = new List<AccuracyOfArrivalReportHeader>();
            //var index = 0;
            //foreach (var selectedSupplierId in selectedSupplierIds)
            //{
            //    index++;
            //    //var se
            //    var selectedDeliveryIds = selectedGarmentDeliveryOrders.Where(w => w.SupplierId == selectedSupplierId).Select(s => s.GarmentDeliveryOrderId).ToList();
            //    var selectedDeliveryItemIds = garmentDeliveryOrderItems.Where(w => selectedDeliveryIds.Contains(w.GarmentDOId)).Select(s => s.Id).ToList();
            //    var selectedGarmentDeliveryOrderDetails = garmentDeliveryOrderDetails.Where(w => selectedDeliveryItemIds.Contains(w.GarmentDOItemId)).ToList();

            //    //var selectedPurchaseRequestIds = selectedGarmentDeliveryOrderDetails.Select(s => s.PRId).ToList();

            //    //var selectedPurchaseRequests = purchaseRequests.Where(w => selectedPurchaseRequestIds.Contains(w.Id)).ToList();
            //    var resultToCount = (from garmentDODetail in selectedGarmentDeliveryOrderDetails
            //                         join garmentDOItem in garmentDeliveryOrderItems on garmentDODetail.GarmentDOItemId equals garmentDOItem.Id
            //                         join garmentDO in selectedGarmentDeliveryOrders on garmentDOItem.GarmentDOId equals garmentDO.GarmentDeliveryOrderId
            //                         join purchaseRequest in purchaseRequests on garmentDODetail.PRId equals purchaseRequest.Id

            //                         select new
            //                         {
            //                             garmentDO.DODate,
            //                             purchaseRequest.ShipmentDate
            //                         }).ToList();

            //    var total = resultToCount.Count;
            //    var okTotal = resultToCount.Count(c => (c.ShipmentDate - c.DODate).Days >= 30);
            //    var okPercentage = total > 0 ? (decimal)((okTotal / total) * 100) : 0;

            //    var notOkTotal = resultToCount.Count(c => (c.ShipmentDate - c.DODate).Days < 30);
            //    var notOkPercentage = total > 0 ? (decimal)((notOkTotal / total) * 100) : 0;

            //    var selectedSupplier = selectedGarmentDeliveryOrders.FirstOrDefault(f => f.SupplierId == selectedSupplierId);

            //    var datum = new AccuracyOfArrivalReportHeader
            //    {
            //        SupplierId = selectedSupplierId,
            //        SupplierCode = selectedSupplier.SupplierCode,
            //        SupplierName = selectedSupplier.SupplierName,
            //        //OKStatusPercentage = Math.Floor((double)okPercentage),
            //        OKStatusPercentage = okPercentage,
            //        OKTotal = okTotal,
            //        //NotOKStatusPercentage = Math.Floor((double)notOkPercentage),
            //        NotOKStatusPercentage = notOkPercentage,
            //        NotOKTotal = notOkTotal,
            //        Total = total,
            //    };

            //    result.Add(datum);

            //}

            //return result;
            var selectedSupplierIds = selectedGarmentDeliveryOrders.Select(s => s.SupplierId).Distinct().ToList();
            //var garmentDeliveryOrderIds = selectedGarmentDeliveryOrders.Select(s => s.GarmentDeliveryOrderId).ToList();
            //var garmentDeliveryOrderItems = dbContext.GarmentDeliveryOrderItems.Where(w => garmentDeliveryOrderIds.Contains(w.GarmentDOId)).Select(s => new { s.Id, s.GarmentDOId }).ToList();
            //var garmentDeliveryOrderItemIds = garmentDeliveryOrderItems.Select(s => s.Id).ToList();
            //var garmentDeliveryOrderDetails = dbContext.GarmentDeliveryOrderDetails.Where(w => w.CodeRequirment == "BB" && !w.RONo.EndsWith("S") && garmentDeliveryOrderItemIds.Contains(w.GarmentDOItemId)).Select(s => new { s.Id, s.PRId, s.GarmentDOItemId }).ToList();

            //var purchaseRequestIds = garmentDeliveryOrderDetails.Select(s => s.PRId).ToList();
            //var purchaseRequests = dbContext.GarmentPurchaseRequests.Where(w => purchaseRequestIds.Contains(w.Id)).Select(s => new { s.Id, s.ShipmentDate }).ToList();

            //var result = new List<AccuracyOfArrivalReportHeader>();
            //var index = 0;
            //foreach (var selectedSupplierId in selectedSupplierIds)
            //{
            //    index++;
            //    //var se
            //    var selectedDeliveryIds = selectedGarmentDeliveryOrders.Where(w => w.SupplierId == selectedSupplierId).Select(s => s.GarmentDeliveryOrderId).ToList();
            //    var selectedDeliveryItemIds = garmentDeliveryOrderItems.Where(w => selectedDeliveryIds.Contains(w.GarmentDOId)).Select(s => s.Id).ToList();
            //    var selectedGarmentDeliveryOrderDetails = garmentDeliveryOrderDetails.Where(w => selectedDeliveryItemIds.Contains(w.GarmentDOItemId)).ToList();

            //    //var selectedPurchaseRequestIds = selectedGarmentDeliveryOrderDetails.Select(s => s.PRId).ToList();

            //    //var selectedPurchaseRequests = purchaseRequests.Where(w => selectedPurchaseRequestIds.Contains(w.Id)).ToList();
            //    var resultToCount = (from garmentDODetail in selectedGarmentDeliveryOrderDetails
            //                         join garmentDOItem in garmentDeliveryOrderItems on garmentDODetail.GarmentDOItemId equals garmentDOItem.Id
            //                         join garmentDO in selectedGarmentDeliveryOrders on garmentDOItem.GarmentDOId equals garmentDO.GarmentDeliveryOrderId
            //                         join purchaseRequest in purchaseRequests on garmentDODetail.PRId equals purchaseRequest.Id

            //                         select new
            //                         {
            //                             garmentDO.DODate,
            //                             purchaseRequest.ShipmentDate
            //                         }).ToList();

            //    var total = resultToCount.Count;
            //    var okTotal = resultToCount.Count(c => (c.ShipmentDate - c.DODate).Days >= 30);
            //    var okPercentage = total > 0 ? (decimal)((okTotal / total) * 100) : 0;

            //    var notOkTotal = resultToCount.Count(c => (c.ShipmentDate - c.DODate).Days < 30);
            //    var notOkPercentage = total > 0 ? (decimal)((notOkTotal / total) * 100) : 0;

            //    var selectedSupplier = selectedGarmentDeliveryOrders.FirstOrDefault(f => f.SupplierId == selectedSupplierId);

            //    var datum = new AccuracyOfArrivalReportHeader
            //    {
            //        No = index,
            //        SupplierId = selectedSupplierId,
            //        SupplierCode = selectedSupplier.SupplierCode,
            //        SupplierName = selectedSupplier.SupplierName,
            //        //OKStatusPercentage = Math.Floor((double)okPercentage),
            //        OKStatusPercentage = okPercentage,
            //        OKTotal = okTotal,
            //        //NotOKStatusPercentage = Math.Floor((double)notOkPercentage),
            //        NotOKStatusPercentage = notOkPercentage,
            //        NotOKTotal = notOkTotal,
            //        Total = total
            //    };

            //    result.Add(datum);

            //}
            var garmentDeliveryOrderIds = selectedGarmentDeliveryOrders.Select(s => s.GarmentDeliveryOrderId).ToList();
            var garmentDeliveryOrderItems = dbContext.GarmentDeliveryOrderItems.Where(w => garmentDeliveryOrderIds.Contains(w.GarmentDOId)).Select(s => new { s.Id, s.GarmentDOId, s.EPOId }).ToList();
            var garmentDeliveryOrderItemIds = garmentDeliveryOrderItems.Select(s => s.Id).ToList();
            var garmentDeliveryOrderDetails = dbContext.GarmentDeliveryOrderDetails.Where(w => w.CodeRequirment == "BP" && !w.RONo.EndsWith("S") && garmentDeliveryOrderItemIds.Contains(w.GarmentDOItemId)).Select(s => new { s.Id, s.PRId, s.GarmentDOItemId, s.ProductCode, s.ProductName, s.ProductRemark, s.EPOItemId, s.RONo, s.POSerialNumber }).ToList();

            var garmentPurchaseRequestIds = garmentDeliveryOrderDetails.Select(s => s.PRId).ToList();
            var garmentPurchaseRequests = dbContext.GarmentPurchaseRequests.Where(w => garmentPurchaseRequestIds.Contains(w.Id)).Select(s => new { s.Id, s.ShipmentDate, s.Date }).ToList();

            var garmentInternalPurchaseOrders = dbContext.GarmentInternalPurchaseOrders.Where(w => garmentPurchaseRequestIds.Contains(w.PRId)).Select(s => new { s.Id, s.CreatedUtc, s.PRId }).ToList();

            var garmentExternalPurchaseOrderIds = garmentDeliveryOrderItems.Select(s => s.EPOId).ToList();
            var garmentExternalPurchaseOrders = dbContext.GarmentExternalPurchaseOrders.Where(w => garmentExternalPurchaseOrderIds.Contains(w.Id)).Select(s => new { s.Id, s.OrderDate, s.CreatedBy }).ToList();

            var garmentExternalPurchaseOrderItemIds = garmentDeliveryOrderDetails.Select(s => s.EPOItemId).ToList();
            var garmentExternalPurchaseOrderItems = dbContext.GarmentExternalPurchaseOrderItems.Where(w => garmentExternalPurchaseOrderItemIds.Contains(w.Id)).Select(s => new { s.Id, s.Article }).ToList();

            var result = new List<AccuracyOfArrivalReportDetail>();
            var index = 0;
            foreach (var garmentDeliveryOrderDetail in garmentDeliveryOrderDetails)
            {
                index++;

                var garmentDeliveryOrderItem = garmentDeliveryOrderItems.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.GarmentDOItemId);
                var garmentDeliveryOrder = selectedGarmentDeliveryOrders.FirstOrDefault(f => f.GarmentDeliveryOrderId == garmentDeliveryOrderItem.GarmentDOId);
                var garmentPurchaseRequest = garmentPurchaseRequests.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.PRId);
                var garmentInternalPurchaseOrder = garmentInternalPurchaseOrders.FirstOrDefault(f => f.PRId == garmentDeliveryOrderDetail.PRId);
                var garmentExternalPurchaseOrder = garmentExternalPurchaseOrders.FirstOrDefault(f => f.Id == garmentDeliveryOrderItem.EPOId);
                var garmentExternalPurchaseOrderItem = garmentExternalPurchaseOrderItems.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.EPOItemId);

                var datum = new AccuracyOfArrivalReportDetail
                {
                    No = index,
                    SupplierId = garmentDeliveryOrder.SupplierId,
                    SupplierCode = garmentDeliveryOrder.SupplierCode,
                    SupplierName = garmentDeliveryOrder.SupplierName,
                    PRDate = garmentPurchaseRequest.Date,
                    IPODate = garmentInternalPurchaseOrder.CreatedUtc,
                    EPODate = garmentExternalPurchaseOrder.OrderDate,
                    DONo = garmentDeliveryOrder.DONo,
                    ProductCode = garmentDeliveryOrderDetail.ProductCode,
                    ProductName = garmentDeliveryOrderDetail.ProductName,
                    ProductRemark = garmentDeliveryOrderDetail.ProductRemark,
                    Article = garmentExternalPurchaseOrderItem.Article,
                    RONo = garmentDeliveryOrderDetail.RONo,
                    ShipmentDate = garmentPurchaseRequest.ShipmentDate,
                    DODate = garmentDeliveryOrder.DODate,
                    Status = (garmentPurchaseRequest.ShipmentDate - garmentDeliveryOrder.DODate).Days >= 30 ? 1 : 0,
                    Status1 = (garmentPurchaseRequest.ShipmentDate - garmentDeliveryOrder.DODate).Days < 30 ? 1 : 0,
                    Staff = garmentExternalPurchaseOrder.CreatedBy,
                    POSerialNumber = garmentDeliveryOrderDetail.POSerialNumber
                };

                result.Add(datum);

                //var total = resultToCount.Count;
                //    var okTotal = resultToCount.Count(c => (c.ShipmentDate - c.DODate).Days >= 30);
                //    var okPercentage = total > 0 ? (decimal)((okTotal / total) * 100) : 0;

                //    var notOkTotal = resultToCount.Count(c => (c.ShipmentDate - c.DODate).Days < 30);
                //    var notOkPercentage = total > 0 ? (decimal)((notOkTotal / total) * 100) : 0;

                //    var selectedSupplier = selectedGarmentDeliveryOrders.FirstOrDefault(f => f.SupplierId == selectedSupplierId);

                //    var datum = new AccuracyOfArrivalReportHeader
                //    {
                //        No = index,
                //        SupplierId = selectedSupplierId,
                //        SupplierCode = selectedSupplier.SupplierCode,
                //        SupplierName = selectedSupplier.SupplierName,
                //        OKStatusPercentage = okPercentage,
                //        OKTotal = okTotal,
                //        NotOKStatusPercentage = notOkPercentage,
                //        NotOKTotal = notOkTotal,
                //        Total = total
                //    };

                //    result.Add(datum);

            }
            //

            var reportResult = (from a in result
                                group new { OK = a.Status, NOK = a.Status1 } by new
                                {
                                    a.SupplierId,
                                    a.SupplierCode,
                                    a.SupplierName
                                } into G

                                select new AccuracyOfArrivalReportHeader
                                {
                                    SupplierId = G.Key.SupplierId,
                                    SupplierCode = G.Key.SupplierCode,
                                    SupplierName = G.Key.SupplierName,
                                    OKTotal = G.Sum(m => m.OK),
                                    NotOKTotal = G.Sum(m => m.NOK),
                                    OKStatusPercentage = Math.Round(((G.Sum(m => m.OK) / G.Count()) * 100), 2),
                                    NotOKStatusPercentage = Math.Round(((G.Sum(m => m.NOK) / G.Count()) * 100), 2),
                                    Total = G.Count(),
                                });

            return reportResult.OrderBy(x => x.SupplierCode).ToList();

            //return result;

        }

        private List<AccuracyOfArrivalReportHeader> GetBBResult(List<SelectedGarmentDeliveryOrder> selectedGarmentDeliveryOrders)
        {
            var selectedSupplierIds = selectedGarmentDeliveryOrders.Select(s => s.SupplierId).Distinct().ToList();
            //var garmentDeliveryOrderIds = selectedGarmentDeliveryOrders.Select(s => s.GarmentDeliveryOrderId).ToList();
            //var garmentDeliveryOrderItems = dbContext.GarmentDeliveryOrderItems.Where(w => garmentDeliveryOrderIds.Contains(w.GarmentDOId)).Select(s => new { s.Id, s.GarmentDOId }).ToList();
            //var garmentDeliveryOrderItemIds = garmentDeliveryOrderItems.Select(s => s.Id).ToList();
            //var garmentDeliveryOrderDetails = dbContext.GarmentDeliveryOrderDetails.Where(w => w.CodeRequirment == "BB" && !w.RONo.EndsWith("S") && garmentDeliveryOrderItemIds.Contains(w.GarmentDOItemId)).Select(s => new { s.Id, s.PRId, s.GarmentDOItemId }).ToList();

            //var purchaseRequestIds = garmentDeliveryOrderDetails.Select(s => s.PRId).ToList();
            //var purchaseRequests = dbContext.GarmentPurchaseRequests.Where(w => purchaseRequestIds.Contains(w.Id)).Select(s => new { s.Id, s.ShipmentDate }).ToList();

            //var result = new List<AccuracyOfArrivalReportHeader>();
            //var index = 0;
            //foreach (var selectedSupplierId in selectedSupplierIds)
            //{
            //    index++;
            //    //var se
            //    var selectedDeliveryIds = selectedGarmentDeliveryOrders.Where(w => w.SupplierId == selectedSupplierId).Select(s => s.GarmentDeliveryOrderId).ToList();
            //    var selectedDeliveryItemIds = garmentDeliveryOrderItems.Where(w => selectedDeliveryIds.Contains(w.GarmentDOId)).Select(s => s.Id).ToList();
            //    var selectedGarmentDeliveryOrderDetails = garmentDeliveryOrderDetails.Where(w => selectedDeliveryItemIds.Contains(w.GarmentDOItemId)).ToList();

            //    //var selectedPurchaseRequestIds = selectedGarmentDeliveryOrderDetails.Select(s => s.PRId).ToList();

            //    //var selectedPurchaseRequests = purchaseRequests.Where(w => selectedPurchaseRequestIds.Contains(w.Id)).ToList();
            //    var resultToCount = (from garmentDODetail in selectedGarmentDeliveryOrderDetails
            //                         join garmentDOItem in garmentDeliveryOrderItems on garmentDODetail.GarmentDOItemId equals garmentDOItem.Id
            //                         join garmentDO in selectedGarmentDeliveryOrders on garmentDOItem.GarmentDOId equals garmentDO.GarmentDeliveryOrderId
            //                         join purchaseRequest in purchaseRequests on garmentDODetail.PRId equals purchaseRequest.Id

            //                         select new
            //                         {
            //                             garmentDO.DODate,
            //                             purchaseRequest.ShipmentDate
            //                         }).ToList();

            //    var total = resultToCount.Count;
            //    var okTotal = resultToCount.Count(c => (c.ShipmentDate - c.DODate).Days >= 30);
            //    var okPercentage = total > 0 ? (decimal)((okTotal / total) * 100) : 0;

            //    var notOkTotal = resultToCount.Count(c => (c.ShipmentDate - c.DODate).Days < 30);
            //    var notOkPercentage = total > 0 ? (decimal)((notOkTotal / total) * 100) : 0;

            //    var selectedSupplier = selectedGarmentDeliveryOrders.FirstOrDefault(f => f.SupplierId == selectedSupplierId);

            //    var datum = new AccuracyOfArrivalReportHeader
            //    {
            //        No = index,
            //        SupplierId = selectedSupplierId,
            //        SupplierCode = selectedSupplier.SupplierCode,
            //        SupplierName = selectedSupplier.SupplierName,
            //        //OKStatusPercentage = Math.Floor((double)okPercentage),
            //        OKStatusPercentage = okPercentage,
            //        OKTotal = okTotal,
            //        //NotOKStatusPercentage = Math.Floor((double)notOkPercentage),
            //        NotOKStatusPercentage = notOkPercentage,
            //        NotOKTotal = notOkTotal,
            //        Total = total
            //    };

            //    result.Add(datum);

            //}
            var garmentDeliveryOrderIds = selectedGarmentDeliveryOrders.Select(s => s.GarmentDeliveryOrderId).ToList();
            var garmentDeliveryOrderItems = dbContext.GarmentDeliveryOrderItems.Where(w => garmentDeliveryOrderIds.Contains(w.GarmentDOId)).Select(s => new { s.Id, s.GarmentDOId, s.EPOId }).ToList();
            var garmentDeliveryOrderItemIds = garmentDeliveryOrderItems.Select(s => s.Id).ToList();
            var garmentDeliveryOrderDetails = dbContext.GarmentDeliveryOrderDetails.Where(w => w.CodeRequirment == "BB" && !w.RONo.EndsWith("S") && garmentDeliveryOrderItemIds.Contains(w.GarmentDOItemId)).Select(s => new { s.Id, s.PRId, s.GarmentDOItemId, s.ProductCode, s.ProductName, s.ProductRemark, s.EPOItemId, s.RONo, s.POSerialNumber }).ToList();

            var garmentPurchaseRequestIds = garmentDeliveryOrderDetails.Select(s => s.PRId).ToList();
            var garmentPurchaseRequests = dbContext.GarmentPurchaseRequests.Where(w => garmentPurchaseRequestIds.Contains(w.Id)).Select(s => new { s.Id, s.ShipmentDate, s.Date }).ToList();

            var garmentInternalPurchaseOrders = dbContext.GarmentInternalPurchaseOrders.Where(w => garmentPurchaseRequestIds.Contains(w.PRId)).Select(s => new { s.Id, s.CreatedUtc, s.PRId }).ToList();

            var garmentExternalPurchaseOrderIds = garmentDeliveryOrderItems.Select(s => s.EPOId).ToList();
            var garmentExternalPurchaseOrders = dbContext.GarmentExternalPurchaseOrders.Where(w => garmentExternalPurchaseOrderIds.Contains(w.Id)).Select(s => new { s.Id, s.OrderDate, s.CreatedBy }).ToList();

            var garmentExternalPurchaseOrderItemIds = garmentDeliveryOrderDetails.Select(s => s.EPOItemId).ToList();
            var garmentExternalPurchaseOrderItems = dbContext.GarmentExternalPurchaseOrderItems.Where(w => garmentExternalPurchaseOrderItemIds.Contains(w.Id)).Select(s => new { s.Id, s.Article }).ToList();

            var result = new List<AccuracyOfArrivalReportDetail>();
            var index = 0;
            foreach (var garmentDeliveryOrderDetail in garmentDeliveryOrderDetails)
            {
                index++;

                var garmentDeliveryOrderItem = garmentDeliveryOrderItems.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.GarmentDOItemId);
                var garmentDeliveryOrder = selectedGarmentDeliveryOrders.FirstOrDefault(f => f.GarmentDeliveryOrderId == garmentDeliveryOrderItem.GarmentDOId);
                var garmentPurchaseRequest = garmentPurchaseRequests.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.PRId);
                var garmentInternalPurchaseOrder = garmentInternalPurchaseOrders.FirstOrDefault(f => f.PRId == garmentDeliveryOrderDetail.PRId);
                var garmentExternalPurchaseOrder = garmentExternalPurchaseOrders.FirstOrDefault(f => f.Id == garmentDeliveryOrderItem.EPOId);
                var garmentExternalPurchaseOrderItem = garmentExternalPurchaseOrderItems.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.EPOItemId);

                var datum = new AccuracyOfArrivalReportDetail
                {
                    No = index,
                    SupplierId = garmentDeliveryOrder.SupplierId,
                    SupplierCode = garmentDeliveryOrder.SupplierCode,
                    SupplierName = garmentDeliveryOrder.SupplierName,
                    PRDate = garmentPurchaseRequest.Date,
                    IPODate = garmentInternalPurchaseOrder.CreatedUtc,
                    EPODate = garmentExternalPurchaseOrder.OrderDate,
                    DONo = garmentDeliveryOrder.DONo,
                    ProductCode = garmentDeliveryOrderDetail.ProductCode,
                    ProductName = garmentDeliveryOrderDetail.ProductName,
                    ProductRemark = garmentDeliveryOrderDetail.ProductRemark,
                    Article = garmentExternalPurchaseOrderItem.Article,
                    RONo = garmentDeliveryOrderDetail.RONo,
                    ShipmentDate = garmentPurchaseRequest.ShipmentDate,
                    DODate = garmentDeliveryOrder.DODate,
                    Status = (garmentPurchaseRequest.ShipmentDate - garmentDeliveryOrder.DODate).Days >= 30 ? 1 : 0,
                    Status1 = (garmentPurchaseRequest.ShipmentDate - garmentDeliveryOrder.DODate).Days < 30 ? 1 : 0,
                    Staff = garmentExternalPurchaseOrder.CreatedBy,
                    POSerialNumber = garmentDeliveryOrderDetail.POSerialNumber
                };

                result.Add(datum);

                //var total = resultToCount.Count;
                //    var okTotal = resultToCount.Count(c => (c.ShipmentDate - c.DODate).Days >= 30);
                //    var okPercentage = total > 0 ? (decimal)((okTotal / total) * 100) : 0;

                //    var notOkTotal = resultToCount.Count(c => (c.ShipmentDate - c.DODate).Days < 30);
                //    var notOkPercentage = total > 0 ? (decimal)((notOkTotal / total) * 100) : 0;

                //    var selectedSupplier = selectedGarmentDeliveryOrders.FirstOrDefault(f => f.SupplierId == selectedSupplierId);

                //    var datum = new AccuracyOfArrivalReportHeader
                //    {
                //        No = index,
                //        SupplierId = selectedSupplierId,
                //        SupplierCode = selectedSupplier.SupplierCode,
                //        SupplierName = selectedSupplier.SupplierName,
                //        OKStatusPercentage = okPercentage,
                //        OKTotal = okTotal,
                //        NotOKStatusPercentage = notOkPercentage,
                //        NotOKTotal = notOkTotal,
                //        Total = total
                //    };

                //    result.Add(datum);

            }
            //

            var reportResult = (from a in result
                        group new { OK = a.Status, NOK = a.Status1 } by new
                        {
                            a.SupplierId,
                            a.SupplierCode,
                            a.SupplierName
                        } into G

                        select new AccuracyOfArrivalReportHeader
                        {
                            SupplierId = G.Key.SupplierId,
                            SupplierCode = G.Key.SupplierCode,
                            SupplierName = G.Key.SupplierName,
                            OKTotal = G.Sum(m => m.OK),
                            NotOKTotal = G.Sum(m => m.NOK),
                            OKStatusPercentage = Math.Round(((G.Sum(m => m.OK) / G.Count()) * 100), 2),
                            NotOKStatusPercentage = Math.Round(((G.Sum(m => m.NOK) / G.Count()) * 100), 2),
                            Total = G.Count(),
                        });

            return reportResult.OrderBy(x => x.SupplierCode).ToList();

            //return result;
        }

        public MemoryStream GenerateExcelArrivalHeader(string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var ctg = "";
            if (category == "Bahan Baku")
            {
                category = "BB";
            }
            else if (category == "Bahan Pendukung")
            {
                category = "BP";
            }

            var selectedGarmentDeliveryOrders = dbContext.GarmentDeliveryOrders.Where(w => w.ArrivalDate.AddHours(7).Date >= dateFrom && w.ArrivalDate.AddHours(7).Date <= dateTo).Select(s => new SelectedGarmentDeliveryOrder(s)).ToList();
            var reportResult = new List<AccuracyOfArrivalReportHeader>();

            switch (category)
            {
                case "BB":
                    reportResult = GetBBResult(selectedGarmentDeliveryOrders);
                    break;
                case "BP":
                    reportResult = GetBPResult(selectedGarmentDeliveryOrders);
                    break;
                default:
                    reportResult = GetDefaultResult(selectedGarmentDeliveryOrders);
                    break;
            }

            reportResult = reportResult.Where(w => w.Total > 0).ToList();
            reportResult = reportResult.OrderBy(o => o.OKStatusPercentage).ThenByDescending(t => t.Total).ToList();

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "NO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "OK %", DataType = typeof(int) });
            result.Columns.Add(new DataColumn() { ColumnName = "Not OK %", DataType = typeof(int) });
            result.Columns.Add(new DataColumn() { ColumnName = "JUMLAH", DataType = typeof(int) });

            if (reportResult.ToArray().Count() == 0)
                result.Rows.Add("", "", 0, 0); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in reportResult)
                {
                    index++;
                    result.Rows.Add(index, item.SupplierName, item.OKStatusPercentage, item.Total);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }

        public Tuple<List<AccuracyOfArrivalReportViewModel>, int> GetReportDetailAccuracyofArrival(string supplier, string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var ctg = "";
            if (category == "Bahan Baku")
            {
                ctg = "BB";
            }
            else if (category == "Bahan Pendukung")
            {
                ctg = "BP";
            }

            var QuerySupplier = GetReportQuery(ctg, dateFrom, dateTo, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();

            foreach (var item in QuerySupplier.Where(b => b.supplier.Code == supplier).OrderByDescending(b => b.doDate))
            {
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = item.supplier,
                    poSerialNumber = item.poSerialNumber,
                    prDate = item.prDate,
                    poDate = item.poDate,
                    epoDate = item.epoDate,
                    product = item.product,
                    article = item.article,
                    roNo = item.roNo,
                    shipmentDate = item.shipmentDate,
                    doDate = item.doDate,
                    staff = item.staff,
                    category = item.category,
                    doNo = item.doNo,
                    ok_notOk = item.ok_notOk,
                    percentOk_notOk = item.percentOk_notOk,
                    jumlah = item.jumlah,
                    jumlahOk = item.jumlahOk,
                    dateDiff = item.dateDiff,
                    LastModifiedUtc = item.LastModifiedUtc
                };
                Data.Add(_new);
            }
            return Tuple.Create(Data, Data.Count);
        }

        public List<AccuracyOfArrivalReportDetail> GetAccuracyOfArrivalDetail(string supplierCode, string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            //var endDate = dateTo ?? DateTime.Now;
            //var startDate = dateFrom ?? endDate.AddDays(-30);

            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            if (category == "Bahan Baku")
            {
                category = "BB";
            }
            else if (category == "Bahan Pendukung")
            {
                category = "BP";
            }

            //if (endDate < startDate)
            //    throw new Exception("date range is invalid");

            var Status = "";  // new[] { "" };
            var Supplier = new[] { "MADEIRA", "MARATHON" };

            var selectedGarmentDeliveryOrders = dbContext.GarmentDeliveryOrders.Where(w => w.ArrivalDate.AddHours(7).Date >= dateFrom && w.ArrivalDate.AddHours(7).Date <= dateTo && w.SupplierCode.Equals(supplierCode)).Select(s => new SelectedGarmentDeliveryOrder(s)).ToList();
            var reportDetailResult = new List<AccuracyOfArrivalReportDetail>();
            //switch (category)
            //{
            //    case "BB":
            //        Status = new[] { "FABRIC", "INTERLINING" };
            //        reportDetailResult = GetBBDetailResult(selectedGarmentDeliveryOrders, Status);
            //        break;
            //    case "BP":
            //        Status = new[] { "FABRIC", "INTERLINING", "PLISKET", "PRINT", "QUILTING", "WASH", "EMBROIDERY", "PROCESS" };
            //        reportDetailResult = GetBPDetailResult(selectedGarmentDeliveryOrders, Status);
            //        break;
            //    default:
            //        reportDetailResult = GetDefaultDetailResult(selectedGarmentDeliveryOrders);
            //        break;
            //}

            switch (category)
            {
                case "BB":
                    reportDetailResult = GetBBDetailResult(selectedGarmentDeliveryOrders);
                    break;
                case "BP":
                    reportDetailResult = GetBPDetailResult(selectedGarmentDeliveryOrders);
                    break;
                default:
                    reportDetailResult = GetDefaultDetailResult(selectedGarmentDeliveryOrders);
                    break;
            }

            return reportDetailResult;
        }

        private List<AccuracyOfArrivalReportDetail> GetDefaultDetailResult(List<SelectedGarmentDeliveryOrder> selectedGarmentDeliveryOrders)
        {
            var garmentDeliveryOrderIds = selectedGarmentDeliveryOrders.Select(s => s.GarmentDeliveryOrderId).ToList();
            var garmentDeliveryOrderItems = dbContext.GarmentDeliveryOrderItems.Where(w => garmentDeliveryOrderIds.Contains(w.GarmentDOId)).Select(s => new { s.Id, s.GarmentDOId, s.EPOId }).ToList();
            var garmentDeliveryOrderItemIds = garmentDeliveryOrderItems.Select(s => s.Id).ToList();
            var garmentDeliveryOrderDetails = dbContext.GarmentDeliveryOrderDetails.Where(w => !w.RONo.EndsWith("S") && garmentDeliveryOrderItemIds.Contains(w.GarmentDOItemId)).Select(s => new { s.Id, s.PRId, s.GarmentDOItemId, s.ProductCode, s.ProductName, s.ProductRemark, s.EPOItemId, s.RONo, s.POSerialNumber }).ToList();

            var garmentPurchaseRequestIds = garmentDeliveryOrderDetails.Select(s => s.PRId).ToList();
            var garmentPurchaseRequests = dbContext.GarmentPurchaseRequests.Where(w => garmentPurchaseRequestIds.Contains(w.Id)).Select(s => new { s.Id, s.ShipmentDate, s.Date }).ToList();

            var garmentInternalPurchaseOrders = dbContext.GarmentInternalPurchaseOrders.Where(w => garmentPurchaseRequestIds.Contains(w.PRId)).Select(s => new { s.Id, s.CreatedUtc, s.PRId }).ToList();

            var garmentExternalPurchaseOrderIds = garmentDeliveryOrderItems.Select(s => s.EPOId).ToList();
            var garmentExternalPurchaseOrders = dbContext.GarmentExternalPurchaseOrders.Where(w => garmentExternalPurchaseOrderIds.Contains(w.Id)).Select(s => new { s.Id, s.OrderDate, s.CreatedBy }).ToList();

            var garmentExternalPurchaseOrderItemIds = garmentDeliveryOrderDetails.Select(s => s.EPOItemId).ToList();
            var garmentExternalPurchaseOrderItems = dbContext.GarmentExternalPurchaseOrderItems.Where(w => garmentExternalPurchaseOrderItemIds.Contains(w.Id)).Select(s => new { s.Id, s.Article }).ToList();

            var result = new List<AccuracyOfArrivalReportDetail>();
            var index = 0;
            foreach (var garmentDeliveryOrderDetail in garmentDeliveryOrderDetails)
            {
                index++;

                var garmentDeliveryOrderItem = garmentDeliveryOrderItems.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.GarmentDOItemId);
                var garmentDeliveryOrder = selectedGarmentDeliveryOrders.FirstOrDefault(f => f.GarmentDeliveryOrderId == garmentDeliveryOrderItem.GarmentDOId);
                var garmentPurchaseRequest = garmentPurchaseRequests.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.PRId);
                var garmentInternalPurchaseOrder = garmentInternalPurchaseOrders.FirstOrDefault(f => f.PRId == garmentDeliveryOrderDetail.PRId);
                var garmentExternalPurchaseOrder = garmentExternalPurchaseOrders.FirstOrDefault(f => f.Id == garmentDeliveryOrderItem.EPOId);
                var garmentExternalPurchaseOrderItem = garmentExternalPurchaseOrderItems.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.EPOItemId);

                var datum = new AccuracyOfArrivalReportDetail
                {
                    No = index,
                    SupplierCode = garmentDeliveryOrder.SupplierCode,
                    SupplierName = garmentDeliveryOrder.SupplierName,
                    PRDate = garmentPurchaseRequest.Date,
                    IPODate = garmentInternalPurchaseOrder.CreatedUtc,
                    EPODate = garmentExternalPurchaseOrder.OrderDate,
                    DONo = garmentDeliveryOrder.DONo,
                    ProductCode = garmentDeliveryOrderDetail.ProductCode,
                    ProductName = garmentDeliveryOrderDetail.ProductName,
                    ProductRemark = garmentDeliveryOrderDetail.ProductRemark,
                    Article = garmentExternalPurchaseOrderItem.Article,
                    RONo = garmentDeliveryOrderDetail.RONo,
                    ShipmentDate = garmentPurchaseRequest.ShipmentDate,
                    DODate = garmentDeliveryOrder.DODate,
                    OKStatus = "NOT OK",
                    Staff = garmentExternalPurchaseOrder.CreatedBy,
                    POSerialNumber = garmentDeliveryOrderDetail.POSerialNumber
                    //Category = cat
                };

                result.Add(datum);

            }

            return result;
        }

        private List<AccuracyOfArrivalReportDetail> GetBPDetailResult(List<SelectedGarmentDeliveryOrder> selectedGarmentDeliveryOrders)
        {
            var garmentDeliveryOrderIds = selectedGarmentDeliveryOrders.Select(s => s.GarmentDeliveryOrderId).ToList();
            var garmentDeliveryOrderItems = dbContext.GarmentDeliveryOrderItems.Where(w => garmentDeliveryOrderIds.Contains(w.GarmentDOId)).Select(s => new { s.Id, s.GarmentDOId, s.EPOId }).ToList();
            var garmentDeliveryOrderItemIds = garmentDeliveryOrderItems.Select(s => s.Id).ToList();
            var garmentDeliveryOrderDetails = dbContext.GarmentDeliveryOrderDetails.Where(w => w.CodeRequirment == "BP" && !w.RONo.EndsWith("S") && garmentDeliveryOrderItemIds.Contains(w.GarmentDOItemId)).Select(s => new { s.Id, s.PRId, s.GarmentDOItemId, s.ProductCode, s.ProductName, s.ProductRemark, s.EPOItemId, s.RONo, s.POSerialNumber }).ToList();

            var garmentPurchaseRequestIds = garmentDeliveryOrderDetails.Select(s => s.PRId).ToList();
            var garmentPurchaseRequests = dbContext.GarmentPurchaseRequests.Where(w => garmentPurchaseRequestIds.Contains(w.Id)).Select(s => new { s.Id, s.ShipmentDate, s.Date }).ToList();

            var garmentInternalPurchaseOrders = dbContext.GarmentInternalPurchaseOrders.Where(w => garmentPurchaseRequestIds.Contains(w.PRId)).Select(s => new { s.Id, s.CreatedUtc, s.PRId }).ToList();

            var garmentExternalPurchaseOrderIds = garmentDeliveryOrderItems.Select(s => s.EPOId).ToList();
            var garmentExternalPurchaseOrders = dbContext.GarmentExternalPurchaseOrders.Where(w => garmentExternalPurchaseOrderIds.Contains(w.Id)).Select(s => new { s.Id, s.OrderDate, s.CreatedBy }).ToList();

            var garmentExternalPurchaseOrderItemIds = garmentDeliveryOrderDetails.Select(s => s.EPOItemId).ToList();
            var garmentExternalPurchaseOrderItems = dbContext.GarmentExternalPurchaseOrderItems.Where(w => garmentExternalPurchaseOrderItemIds.Contains(w.Id)).Select(s => new { s.Id, s.Article }).ToList();

            var result = new List<AccuracyOfArrivalReportDetail>();
            var index = 0;
            foreach (var garmentDeliveryOrderDetail in garmentDeliveryOrderDetails)
            {
                index++;
                //var se
                var garmentDeliveryOrderItem = garmentDeliveryOrderItems.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.GarmentDOItemId);
                var garmentDeliveryOrder = selectedGarmentDeliveryOrders.FirstOrDefault(f => f.GarmentDeliveryOrderId == garmentDeliveryOrderItem.GarmentDOId);
                var garmentPurchaseRequest = garmentPurchaseRequests.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.PRId);
                var garmentInternalPurchaseOrder = garmentInternalPurchaseOrders.FirstOrDefault(f => f.PRId == garmentDeliveryOrderDetail.PRId);
                var garmentExternalPurchaseOrder = garmentExternalPurchaseOrders.FirstOrDefault(f => f.Id == garmentDeliveryOrderItem.EPOId);
                var garmentExternalPurchaseOrderItem = garmentExternalPurchaseOrderItems.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.EPOItemId);

                var datum = new AccuracyOfArrivalReportDetail
                {
                    No = index,
                    SupplierCode = garmentDeliveryOrder.SupplierCode,
                    SupplierName = garmentDeliveryOrder.SupplierName,
                    PRDate = garmentPurchaseRequest.Date,
                    IPODate = garmentInternalPurchaseOrder.CreatedUtc,
                    EPODate = garmentExternalPurchaseOrder.OrderDate,
                    DONo = garmentDeliveryOrder.DONo,
                    ProductCode = garmentDeliveryOrderDetail.ProductCode,
                    ProductName = garmentDeliveryOrderDetail.ProductName,
                    ProductRemark = garmentDeliveryOrderDetail.ProductRemark,
                    Article = garmentExternalPurchaseOrderItem.Article,
                    RONo = garmentDeliveryOrderDetail.RONo,
                    ShipmentDate = garmentPurchaseRequest.ShipmentDate,
                    DODate = garmentDeliveryOrder.DODate,
                    OKStatus = (garmentPurchaseRequest.ShipmentDate - garmentDeliveryOrder.DODate).Days >= 30 ? "OK" : "NOT OK",
                    Staff = garmentExternalPurchaseOrder.CreatedBy,
                    POSerialNumber = garmentDeliveryOrderDetail.POSerialNumber
                    //Category = cat
                };

                result.Add(datum);

            }

            return result;
        }

        private List<AccuracyOfArrivalReportDetail> GetBBDetailResult(List<SelectedGarmentDeliveryOrder> selectedGarmentDeliveryOrders)
        {
            //throw new NotImplementedException();
            var garmentDeliveryOrderIds = selectedGarmentDeliveryOrders.Select(s => s.GarmentDeliveryOrderId).ToList();
            var garmentDeliveryOrderItems = dbContext.GarmentDeliveryOrderItems.Where(w => garmentDeliveryOrderIds.Contains(w.GarmentDOId)).Select(s => new { s.Id, s.GarmentDOId, s.EPOId }).ToList();
            var garmentDeliveryOrderItemIds = garmentDeliveryOrderItems.Select(s => s.Id).ToList();
            var garmentDeliveryOrderDetails = dbContext.GarmentDeliveryOrderDetails.Where(w => w.CodeRequirment == "BB" && !w.RONo.EndsWith("S") && garmentDeliveryOrderItemIds.Contains(w.GarmentDOItemId)).Select(s => new { s.Id, s.PRId, s.GarmentDOItemId, s.ProductCode, s.ProductName, s.ProductRemark, s.EPOItemId, s.RONo, s.POSerialNumber }).ToList();

            var garmentPurchaseRequestIds = garmentDeliveryOrderDetails.Select(s => s.PRId).ToList();
            var garmentPurchaseRequests = dbContext.GarmentPurchaseRequests.Where(w => garmentPurchaseRequestIds.Contains(w.Id)).Select(s => new { s.Id, s.ShipmentDate, s.Date }).ToList();

            var garmentInternalPurchaseOrders = dbContext.GarmentInternalPurchaseOrders.Where(w => garmentPurchaseRequestIds.Contains(w.PRId)).Select(s => new { s.Id, s.CreatedUtc, s.PRId }).ToList();

            var garmentExternalPurchaseOrderIds = garmentDeliveryOrderItems.Select(s => s.EPOId).ToList();
            var garmentExternalPurchaseOrders = dbContext.GarmentExternalPurchaseOrders.Where(w => garmentExternalPurchaseOrderIds.Contains(w.Id)).Select(s => new { s.Id, s.OrderDate, s.CreatedBy }).ToList();

            var garmentExternalPurchaseOrderItemIds = garmentDeliveryOrderDetails.Select(s => s.EPOItemId).ToList();
            var garmentExternalPurchaseOrderItems = dbContext.GarmentExternalPurchaseOrderItems.Where(w => garmentExternalPurchaseOrderItemIds.Contains(w.Id)).Select(s => new { s.Id, s.Article }).ToList();

            var result = new List<AccuracyOfArrivalReportDetail>();
            var index = 0;
            foreach (var garmentDeliveryOrderDetail in garmentDeliveryOrderDetails)
            {
                index++;

                var garmentDeliveryOrderItem = garmentDeliveryOrderItems.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.GarmentDOItemId);
                var garmentDeliveryOrder = selectedGarmentDeliveryOrders.FirstOrDefault(f => f.GarmentDeliveryOrderId == garmentDeliveryOrderItem.GarmentDOId);
                var garmentPurchaseRequest = garmentPurchaseRequests.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.PRId);
                var garmentInternalPurchaseOrder = garmentInternalPurchaseOrders.FirstOrDefault(f => f.PRId == garmentDeliveryOrderDetail.PRId);
                var garmentExternalPurchaseOrder = garmentExternalPurchaseOrders.FirstOrDefault(f => f.Id == garmentDeliveryOrderItem.EPOId);
                var garmentExternalPurchaseOrderItem = garmentExternalPurchaseOrderItems.FirstOrDefault(f => f.Id == garmentDeliveryOrderDetail.EPOItemId);

                var datum = new AccuracyOfArrivalReportDetail
                {
                    No = index,
                    SupplierCode = garmentDeliveryOrder.SupplierCode,
                    SupplierName = garmentDeliveryOrder.SupplierName,
                    PRDate = garmentPurchaseRequest.Date,
                    IPODate = garmentInternalPurchaseOrder.CreatedUtc,
                    EPODate = garmentExternalPurchaseOrder.OrderDate,
                    DONo = garmentDeliveryOrder.DONo,
                    ProductCode = garmentDeliveryOrderDetail.ProductCode,
                    ProductName = garmentDeliveryOrderDetail.ProductName,
                    ProductRemark = garmentDeliveryOrderDetail.ProductRemark,
                    Article = garmentExternalPurchaseOrderItem.Article,
                    RONo = garmentDeliveryOrderDetail.RONo,
                    ShipmentDate = garmentPurchaseRequest.ShipmentDate,
                    DODate = garmentDeliveryOrder.DODate,
                    OKStatus = (garmentPurchaseRequest.ShipmentDate - garmentDeliveryOrder.DODate).Days >= 30 ? "OK" : "NOT OK",
                    Staff = garmentExternalPurchaseOrder.CreatedBy,
                    POSerialNumber = garmentDeliveryOrderDetail.POSerialNumber
                    //Category = cat
                };

                result.Add(datum);

            }

            return result;
        }

        public MemoryStream GenerateExcelArrivalDetail(string supplier, string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            if (category == "Bahan Baku")
            {
                category = "BB";
            }
            else if (category == "Bahan Pendukung")
            {
                category = "BP";
            }
            //var QuerySupplier = GetReportQuery(ctg, dateFrom, dateTo, offset);

            //List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();

            //foreach (var item in QuerySupplier.Where(b => b.supplier.Code == supplier).OrderByDescending(b => b.doDate))
            //{
            //    AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
            //    {
            //        supplier = item.supplier,
            //        poSerialNumber = item.poSerialNumber,
            //        prDate = item.prDate,
            //        poDate = item.poDate,
            //        epoDate = item.epoDate,
            //        product = item.product,
            //        article = item.article,
            //        roNo = item.roNo,
            //        shipmentDate = item.shipmentDate,
            //        doDate = item.doDate,
            //        staff = item.staff,
            //        category = item.category,
            //        doNo = item.doNo,
            //        ok_notOk = item.ok_notOk,
            //        percentOk_notOk = item.percentOk_notOk,
            //        jumlah = item.jumlah,
            //        jumlahOk = item.jumlahOk,
            //        dateDiff = item.dateDiff,
            //        LastModifiedUtc = item.LastModifiedUtc
            //    };
            //    Data.Add(_new);
            //}
            var selectedGarmentDeliveryOrders = dbContext.GarmentDeliveryOrders.Where(w => w.ArrivalDate.AddHours(7).Date >= dateFrom && w.ArrivalDate.AddHours(7).Date <= dateTo && w.SupplierCode.Equals(supplier)).Select(s => new SelectedGarmentDeliveryOrder(s)).ToList();
            var reportDetailResult = new List<AccuracyOfArrivalReportDetail>();
            //switch (category)
            //{
            //    case "BB":
            //        Status = new[] { "FABRIC", "INTERLINING" };
            //        reportDetailResult = GetBBDetailResult(selectedGarmentDeliveryOrders, Status);
            //        break;
            //    case "BP":
            //        Status = new[] { "FABRIC", "INTERLINING", "PLISKET", "PRINT", "QUILTING", "WASH", "EMBROIDERY", "PROCESS" };
            //        reportDetailResult = GetBPDetailResult(selectedGarmentDeliveryOrders, Status);
            //        break;
            //    default:
            //        reportDetailResult = GetDefaultDetailResult(selectedGarmentDeliveryOrders);
            //        break;
            //}

            switch (category)
            {
                case "BB":
                    reportDetailResult = GetBBDetailResult(selectedGarmentDeliveryOrders);
                    break;
                case "BP":
                    reportDetailResult = GetBPDetailResult(selectedGarmentDeliveryOrders);
                    break;
                default:
                    reportDetailResult = GetDefaultDetailResult(selectedGarmentDeliveryOrders);
                    break;
            }

            reportDetailResult = reportDetailResult.OrderBy(o => o.DODate).ThenBy(t => t.DONo).ToList();

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "NO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "PLAN PO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PURCHASE REQUEST", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PO INTERNAL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PEMBELIAN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NO SJ", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KODE BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KETERANGAN BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "ARTIKEL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL SHIPMENT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL DATANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "+/- DATANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "STAFF", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KATEGORI", DataType = typeof(String) });

            if (reportDetailResult.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in reportDetailResult)
                {
                    index++;
                    string prDate = item.PRDate == null ? "-" : item.PRDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string poDate = item.IPODate == null ? "-" : item.IPODate.ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string epoDate = item.EPODate == null ? "-" : item.EPODate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string shipmentDate = item.ShipmentDate == null ? "-" : item.ShipmentDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string doDate = item.DODate == null ? "-" : item.DODate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(index, item.SupplierName, item.POSerialNumber, prDate, poDate, epoDate, item.DONo, item.ProductCode, item.ProductName, item.ProductRemark, item.Article, item.RONo,
                        shipmentDate, doDate, item.OKStatus, item.Staff, item.ProductName);
                }
            }
            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }

        //
        public MemoryStream GenerateExcelArrivalDetailAll(string category, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            if (category == "Bahan Baku")
            {
                category = "BB";
            }
            else if (category == "Bahan Pendukung")
            {
                category = "BP";
            }

            var selectedGarmentDeliveryOrders = dbContext.GarmentDeliveryOrders.Where(w => w.ArrivalDate.AddHours(7).Date >= dateFrom && w.ArrivalDate.AddHours(7).Date <= dateTo).Select(s => new SelectedGarmentDeliveryOrder(s)).ToList();
            var reportDetailResult = new List<AccuracyOfArrivalReportDetail>();

            switch (category)
            {
                case "BB":
                    reportDetailResult = GetBBDetailResult(selectedGarmentDeliveryOrders);
                    break;
                case "BP":
                    reportDetailResult = GetBPDetailResult(selectedGarmentDeliveryOrders);
                    break;
                default:
                    reportDetailResult = GetDefaultDetailResult(selectedGarmentDeliveryOrders);
                    break;
            }

            reportDetailResult = reportDetailResult.OrderBy(o => o.DODate).ThenBy(t => t.DONo).ToList();

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "NO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "PLAN PO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PURCHASE REQUEST", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PO INTERNAL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PEMBELIAN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NO SJ", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KODE BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KETERANGAN BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "ARTIKEL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL SHIPMENT", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL DATANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "+/- DATANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "STAFF", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KATEGORI", DataType = typeof(String) });

            if (reportDetailResult.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in reportDetailResult)
                {
                    index++;
                    string prDate = item.PRDate == null ? "-" : item.PRDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string poDate = item.IPODate == null ? "-" : item.IPODate.ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string epoDate = item.EPODate == null ? "-" : item.EPODate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string shipmentDate = item.ShipmentDate == null ? "-" : item.ShipmentDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string doDate = item.DODate == null ? "-" : item.DODate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(index, item.SupplierName, item.POSerialNumber, prDate, poDate, epoDate, item.DONo, item.ProductCode, item.ProductName, item.ProductRemark, item.Article, item.RONo,
                        shipmentDate, doDate, item.OKStatus, item.Staff, item.ProductName);
                }
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template

                int Total = 0;
                int TotalOk = 0;
                string PercentOk = "";
                int TotalNotOk = 0;
                string PercentNotOk = "";

                Total = reportDetailResult.Count;
                TotalOk = reportDetailResult.Count(c => (c.ShipmentDate - c.DODate).Days >= 30);
                PercentOk = ((decimal)TotalOk / Total).ToString("P", new CultureInfo("id-ID"));
                TotalNotOk = reportDetailResult.Count(c => (c.ShipmentDate - c.DODate).Days < 30);
                PercentNotOk = ((decimal)TotalNotOk / Total).ToString("P", new CultureInfo("id-ID"));

                result.Rows.Add(null, "KESIAPAN BAHAN BAKU / BAHAN PENDUKUNG DENGAN LEAD TIME 30 HARI", null, null, null, null, null, null, null, null);
                result.Rows.Add(null, "Status OK", null, "Perhitungan dari selisih Tgl Shipment dengan Tgl Kedatangan Barang >= 30 hari", null, null, null, null, null, null);
                result.Rows.Add(null, "Persentase Status OK", null, $"{TotalOk}/{Total} X 100% = {PercentOk}", null, null, null, null, null, null);
                result.Rows.Add(null, "Status NOT OK", null, "Perhitungan dari selisih Tgl Shipment dengan Tgl Kedatangan Barang < 30 hari", null, null, null, null, null, null);
                result.Rows.Add(null, "Persentase Status NOT OK", null, $"{TotalNotOk}/{Total} X 100% = {PercentNotOk}", null, null, null, null, null, null);

            }
            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }
        //

        public IQueryable<AccuracyOfArrivalReportViewModel> GetReportQuery2(DateTime? dateFrom, DateTime? dateTo, string paymentType, string paymentMethod, int offset)
        {

            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;
            //bool flagPaymentType = ;
            //bool flagPaymentMethod = ;
            List<AccuracyOfArrivalReportViewModel> listAccuracyOfArrival = new List<AccuracyOfArrivalReportViewModel>();

            var Query = (from a in dbContext.GarmentDeliveryOrders
                         join b in dbContext.GarmentDeliveryOrderItems on a.Id equals b.GarmentDOId
                         join c in dbContext.GarmentDeliveryOrderDetails on b.Id equals c.GarmentDOItemId
                         join d in dbContext.GarmentInternalPurchaseOrders on c.POId equals d.Id
                         join e in dbContext.GarmentInternalPurchaseOrderItems on d.Id equals e.GPOId
                         join f in dbContext.GarmentPurchaseRequests on c.PRId equals f.Id
                         join g in dbContext.GarmentPurchaseRequestItems on f.Id equals g.GarmentPRId
                         join h in dbContext.GarmentExternalPurchaseOrders on b.EPOId equals h.Id
                         join i in dbContext.GarmentExternalPurchaseOrderItems on h.Id equals i.GarmentEPOId
                         where a.IsDeleted == false
                             && d.IsDeleted == false
                             && f.IsDeleted == false
                             && h.IsDeleted == false
                             && ((DateFrom != new DateTime(1970, 1, 1)) ? (a.DODate.Date >= DateFrom && a.DODate.Date <= DateTo) : true)
                         select new AccuracyOfArrivalReportViewModel
                         {
                             supplier = new SupplierViewModel
                             {
                                 Code = a.SupplierCode,
                                 Id = a.SupplierId,
                                 Name = a.SupplierName
                             },
                             poSerialNumber = c.POSerialNumber,
                             prDate = f.Date,
                             poDate = d.CreatedUtc,
                             epoDate = h.OrderDate,
                             product = new GarmentProductViewModel
                             {
                                 Code = c.ProductCode,
                                 Id = c.ProductId,
                                 Name = c.ProductName,
                                 Remark = c.ProductRemark,
                             },
                             article = i.Article,
                             roNo = c.RONo,
                             shipmentDate = h.DeliveryDate,
                             doDate = a.DODate,
                             staff = a.CreatedBy,
                             doNo = a.DONo,
                             ok_notOk = "NOT OK",
                             LastModifiedUtc = i.LastModifiedUtc,
                             paymentMethod = h.PaymentMethod,
                             paymentType = h.PaymentType
                         }).Distinct();

            if (!string.IsNullOrEmpty(paymentType))
            {
                Query = Query.Where(x => x.paymentType == paymentType);
            }

            if (!string.IsNullOrEmpty(paymentMethod))
            {
                Query = Query.Where(x => x.paymentMethod == paymentMethod);
            }

            Query = Query.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.doDate);
            var suppTemp = "";
            var percentOK = 0;
            var percentNotOk = 0;
            var jumlah = 0;

            foreach (var item in Query)
            {
                var ShipmentDate = new DateTimeOffset(item.shipmentDate.Date, TimeSpan.Zero);
                var DODate = new DateTimeOffset(item.doDate.Date, TimeSpan.Zero);
                var jumlahOk = 0;
                var datediff = ((TimeSpan)(DODate - ShipmentDate)).Days;

                if (datediff <= 7)
                {
                    item.ok_notOk = "OK";
                }

                if (suppTemp == "")
                {
                    jumlah += 1;
                    suppTemp = item.supplier.Code;
                    if (item.ok_notOk == "OK")
                    {
                        percentOK += 1;
                    }
                    else
                    {
                        percentNotOk += 1;
                    }

                }
                else if (suppTemp == item.supplier.Code)
                {
                    jumlah += 1;
                    if (item.ok_notOk == "OK")
                    {
                        percentOK += 1;
                    }
                    else
                    {
                        percentNotOk += 1;
                    }
                }
                else if (suppTemp != item.supplier.Code)
                {
                    var perOk = 0;
                    var perNotOk = 0;
                    suppTemp = item.supplier.Code;
                    jumlah = 1;
                    if (item.ok_notOk == "OK")
                    {
                        percentOK = perOk + 1;
                        percentNotOk = perNotOk;
                    }
                    else
                    {
                        percentNotOk = perNotOk + 1;
                        percentOK = perOk;
                    }
                }
                jumlahOk = percentOK + percentNotOk;
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = item.supplier,
                    poSerialNumber = item.poSerialNumber,
                    prDate = item.prDate,
                    poDate = item.poDate,
                    epoDate = item.epoDate,
                    product = item.product,
                    article = item.article,
                    roNo = item.roNo,
                    shipmentDate = item.shipmentDate,
                    doDate = item.doDate,
                    staff = item.staff,
                    doNo = item.doNo,
                    ok_notOk = item.ok_notOk,
                    percentOk_notOk = (percentOK * 100) / jumlah,
                    jumlah = jumlah,
                    jumlahOk = percentOK,
                    dateDiff = datediff,
                    LastModifiedUtc = item.LastModifiedUtc,
                    paymentMethod = item.paymentMethod,
                    paymentType = item.paymentType
                };
                listAccuracyOfArrival.Add(_new);
            }
            return listAccuracyOfArrival.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.doDate).AsQueryable();
        }

        public Tuple<List<AccuracyOfArrivalReportViewModel>, int> GetReportHeaderAccuracyofDelivery(DateTime? dateFrom, DateTime? dateTo, string paymentType, string paymentMethod, int offset)
        {
            var QuerySupplier = GetReportQuery2(dateFrom, dateTo, paymentType, paymentMethod, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();

            var SuppTemp = "";
            foreach (var item in QuerySupplier.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.jumlah))
            {
                if (SuppTemp == "" || SuppTemp != item.supplier.Code)
                {
                    SuppTemp = item.supplier.Code;

                    AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                    {
                        supplier = item.supplier,
                        poSerialNumber = item.poSerialNumber,
                        prDate = item.prDate,
                        poDate = item.poDate,
                        epoDate = item.epoDate,
                        product = item.product,
                        article = item.article,
                        roNo = item.roNo,
                        shipmentDate = item.shipmentDate,
                        doDate = item.doDate,
                        staff = item.staff,
                        doNo = item.doNo,
                        ok_notOk = item.ok_notOk,
                        percentOk_notOk = item.percentOk_notOk,
                        jumlah = item.jumlah,
                        jumlahOk = item.jumlahOk,
                        dateDiff = item.dateDiff,
                        LastModifiedUtc = item.LastModifiedUtc,
                        paymentMethod = item.paymentMethod,
                        paymentType = item.paymentType
                    };
                    Data.Add(_new);
                }
            }
            return Tuple.Create(Data, Data.Count);
        }

        public MemoryStream GenerateExcelDeliveryHeader(DateTime? dateFrom, DateTime? dateTo, string paymentType, string paymentMethod, int offset)
        {
            var Query = GetReportQuery2(dateFrom, dateTo, paymentType, paymentMethod, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();

            var SuppTemp = "";
            foreach (var item in Query.OrderByDescending(b => b.supplier.Code).ThenByDescending(b => b.jumlah))
            {
                if (SuppTemp == "" || SuppTemp != item.supplier.Code)
                {
                    SuppTemp = item.supplier.Code;

                    AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                    {
                        supplier = item.supplier,
                        poSerialNumber = item.poSerialNumber,
                        prDate = item.prDate,
                        poDate = item.poDate,
                        epoDate = item.epoDate,
                        product = item.product,
                        article = item.article,
                        roNo = item.roNo,
                        shipmentDate = item.shipmentDate,
                        doDate = item.doDate,
                        staff = item.staff,
                        doNo = item.doNo,
                        ok_notOk = item.ok_notOk,
                        percentOk_notOk = item.percentOk_notOk,
                        jumlah = item.jumlah,
                        jumlahOk = item.jumlahOk,
                        dateDiff = item.dateDiff,
                        LastModifiedUtc = item.LastModifiedUtc,
                        paymentType = item.paymentType,
                        paymentMethod = item.paymentMethod
                    };
                    Data.Add(_new);
                }
            }

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "NO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "OK %", DataType = typeof(int) });
            result.Columns.Add(new DataColumn() { ColumnName = "JUMLAH", DataType = typeof(int) });

            if (Data.ToArray().Count() == 0)
                result.Rows.Add("", "", 0, 0); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Data)
                {
                    index++;
                    result.Rows.Add(index, item.supplier.Name, item.percentOk_notOk, item.jumlah);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }

        public Tuple<List<AccuracyOfArrivalReportViewModel>, int> GetReportDetailAccuracyofDelivery(string supplier, DateTime? dateFrom, DateTime? dateTo, string paymentType, string paymentMethod, int offset)
        {
            var QuerySupplier = GetReportQuery2(dateFrom, dateTo, paymentType, paymentMethod, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();

            foreach (var item in QuerySupplier.Where(b => b.supplier.Code == supplier).OrderByDescending(b => b.doDate))
            {
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = item.supplier,
                    poSerialNumber = item.poSerialNumber,
                    prDate = item.prDate,
                    poDate = item.poDate,
                    epoDate = item.epoDate,
                    product = item.product,
                    article = item.article,
                    roNo = item.roNo,
                    shipmentDate = item.shipmentDate,
                    doDate = item.doDate,
                    staff = item.staff,
                    doNo = item.doNo,
                    ok_notOk = item.ok_notOk,
                    percentOk_notOk = item.percentOk_notOk,
                    jumlah = item.jumlah,
                    jumlahOk = item.jumlahOk,
                    dateDiff = item.dateDiff,
                    LastModifiedUtc = item.LastModifiedUtc,
                    paymentType = item.paymentType,
                    paymentMethod = item.paymentMethod
                };
                Data.Add(_new);
            }
            return Tuple.Create(Data, Data.Count);
        }

        public MemoryStream GenerateExcelDeliveryDetail(string supplier, DateTime? dateFrom, DateTime? dateTo, string paymentType, string paymentMethod, int offset)
        {
            var QuerySupplier = GetReportQuery2(dateFrom, dateTo, paymentType, paymentMethod, offset);

            List<AccuracyOfArrivalReportViewModel> Data = new List<AccuracyOfArrivalReportViewModel>();

            foreach (var item in QuerySupplier.Where(b => b.supplier.Code == supplier).OrderByDescending(b => b.doDate))
            {
                AccuracyOfArrivalReportViewModel _new = new AccuracyOfArrivalReportViewModel
                {
                    supplier = item.supplier,
                    poSerialNumber = item.poSerialNumber,
                    prDate = item.prDate,
                    poDate = item.poDate,
                    epoDate = item.epoDate,
                    product = item.product,
                    article = item.article,
                    roNo = item.roNo,
                    shipmentDate = item.shipmentDate,
                    doDate = item.doDate,
                    staff = item.staff,
                    category = item.category,
                    doNo = item.doNo,
                    ok_notOk = item.ok_notOk,
                    percentOk_notOk = item.percentOk_notOk,
                    jumlah = item.jumlah,
                    jumlahOk = item.jumlahOk,
                    dateDiff = item.dateDiff,
                    LastModifiedUtc = item.LastModifiedUtc,
                    paymentType = item.paymentType,
                    paymentMethod = item.paymentMethod
                };
                Data.Add(_new);
            }

            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "NO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "SUPPLIER", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "PLAN PO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PURCHASE REQUEST", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PO INTERNAL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL PEMBELIAN", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NO SJ", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KODE BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "NAMA BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KETERANGAN BARANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "ARTIKEL", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL ESTIMASI DATANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "TANGGAL DATANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "+/- DATANG", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "STAFF", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "KATEGORI", DataType = typeof(String) });

            if (Data.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Data)
                {
                    index++;
                    string prDate = item.prDate == null ? "-" : item.prDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string poDate = item.poDate == null ? "-" : item.poDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string epoDate = item.epoDate == null ? "-" : item.epoDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string shipmentDate = item.shipmentDate == null ? "-" : item.shipmentDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string doDate = item.doDate == null ? "-" : item.doDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(index, item.supplier.Name, item.poSerialNumber, prDate, poDate, epoDate, item.doNo, item.product.Code, item.product.Name, item.product.Remark, item.article, item.roNo,
                        shipmentDate, doDate, item.ok_notOk, item.staff, item.product.Name);
                }
            }
            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }

        public IQueryable<GarmentDeliveryOrderReportViewModel> GetReportQueryDO(string no, string poEksNo, long supplierId, string billNo, string paymentBill, DateTime? dateFrom, DateTime? dateTo, int offset)

        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            var Query1 = (from a in dbContext.GarmentDeliveryOrders
                          join i in dbContext.GarmentDeliveryOrderItems on a.Id equals i.GarmentDOId
                          join j in dbContext.GarmentDeliveryOrderDetails on i.Id equals j.GarmentDOItemId
                          join m in dbContext.GarmentExternalPurchaseOrders on i.EPOId equals m.Id
                          join o in dbContext.GarmentBeacukaiItems on a.Id equals o.GarmentDOId into beaitems
                          from oo in beaitems.DefaultIfEmpty()
                          join r in dbContext.GarmentBeacukais on oo.BeacukaiId equals r.Id into beas
                          from rr in beas.DefaultIfEmpty()
                          join n in dbContext.GarmentUnitReceiptNoteItems on j.Id equals n.DODetailId into p
                          from URNItem in p.DefaultIfEmpty()
                          join k in dbContext.GarmentUnitReceiptNotes on URNItem.URNId equals k.Id into l
                          from URN in l.DefaultIfEmpty()
                          where a.IsDeleted == false
                              && i.IsDeleted == false
                              && j.IsDeleted == false
                              && m.IsDeleted == false
                              && URN.IsDeleted == false
                              && a.DONo == (string.IsNullOrWhiteSpace(no) ? a.DONo : no)
                              && a.SupplierId == (supplierId == 0 ? a.SupplierId : supplierId)
                              && a.BillNo == (string.IsNullOrWhiteSpace(billNo) ? a.BillNo : billNo)
                              && a.PaymentBill == (string.IsNullOrWhiteSpace(paymentBill) ? a.PaymentBill : paymentBill)
                              && a.DODate.AddHours(offset).Date >= DateFrom.Date
                              && a.DODate.AddHours(offset).Date <= DateTo.Date
                              && i.EPONo == (string.IsNullOrWhiteSpace(poEksNo) ? i.EPONo : poEksNo)
                          //&& URN.URNType == "PEMBELIAN"

                          select new GarmentDeliveryOrderReportViewModel
                          {
                              no = a.DONo,
                              supplierDoDate = a.DODate == null ? new DateTime(1970, 1, 1) : a.DODate,
                              date = a.ArrivalDate,
                              URNNo = URN == null ? "-" : URN.URNNo,
                              URNDate = URN == null ? new DateTime(1970, 1, 1) : URN.ReceiptDate,
                              URNType = URN == null ? "-" : URN.URNType,
                              urnQuantity = URNItem == null ? 0 : URNItem.ReceiptQuantity,
                              urnUom = URNItem == null ? "-" : URNItem.UomUnit,
                              UnitName = URN.UnitName ?? "-",
                              //urnQuantity = URNItem == null ? 0 : URNItem.ReceiptQuantity,
                              //urnUom = URNItem == null ? "-" : URNItem.UomUnit,
                              supplierName = a.SupplierName,
                              supplierCode = a.SupplierCode,
                              shipmentNo = a.ShipmentNo,
                              shipmentType = a.ShipmentType,
                              createdBy = a.CreatedBy,
                              doCurrencyCode = a.DOCurrencyCode,
                              doCurrencyRate = a.DOCurrencyRate,
                              isCustoms = a.IsCustoms,
                              price = j.PricePerDealUnit,
                              ePONo = i.EPONo,
                              productCode = j.ProductCode,
                              productName = j.ProductName,
                              productRemark = j.ProductRemark,
                              prRefNo = j.POSerialNumber,
                              roNo = j.RONo,
                              prNo = j.PRNo,
                              remark = a.Remark,
                              dOQuantity = j.DOQuantity,
                              dealQuantity = j.DealQuantity,
                              uomUnit = j.UomUnit,
                              createdUtc = j.CreatedUtc,
                              EPOcreatedBy = m.CreatedBy,
                              INNo = a.InternNo,
                              TermPayment = m.PaymentMethod,
                              BeacukaiNo = rr != null ? rr.BeacukaiNo : "-",
                              BeacukaiDate = rr != null ? rr.CreatedUtc : DateTimeOffset.MinValue,
                              BeacukaiType = rr != null ? rr.CustomsType : "-",
                              BillNo = a.BillNo,
                              PaymentBill = a.PaymentBill,
                              BCDate = rr != null ? rr.BeacukaiDate : new DateTime(1970, 1, 1)
                          });

            var Query = (from gdo in Query1
                         where gdo.URNType != "PROSES" && gdo.URNType != "GUDANG LAIN"

                         select new GarmentDeliveryOrderReportViewModel
                         {
                             no = gdo.no,
                             supplierDoDate = gdo.supplierDoDate,
                             date = gdo.date,
                             URNNo = gdo.URNNo,
                             URNDate = gdo.URNDate,
                             UnitName = gdo.UnitName,
                             urnQuantity = gdo.urnQuantity,
                             urnUom = gdo.urnUom,
                             supplierName = gdo.supplierName,
                             supplierCode = gdo.supplierCode,
                             shipmentNo = gdo.shipmentNo,
                             shipmentType = gdo.shipmentType,
                             createdBy = gdo.createdBy,
                             doCurrencyCode = gdo.doCurrencyCode,
                             doCurrencyRate = gdo.doCurrencyRate,
                             isCustoms = gdo.isCustoms,
                             price = gdo.price,
                             ePONo = gdo.ePONo,
                             productCode = gdo.productCode,
                             productName = gdo.productName,
                             productRemark = gdo.productRemark,
                             prRefNo = gdo.prRefNo,
                             roNo = gdo.roNo,
                             prNo = gdo.prNo,
                             remark = gdo.remark,
                             dOQuantity = gdo.dOQuantity,
                             dealQuantity = gdo.dealQuantity,
                             uomUnit = gdo.uomUnit,
                             createdUtc = gdo.createdUtc,
                             EPOcreatedBy = gdo.EPOcreatedBy,
                             INNo = gdo.INNo,
                             TermPayment = gdo.TermPayment,
                             URNType = gdo.URNType,
                             BeacukaiNo = gdo.BeacukaiNo,
                             BeacukaiDate = gdo.BeacukaiDate,
                             BeacukaiType = gdo.BeacukaiType,
                             BillNo = gdo.BillNo,
                             PaymentBill = gdo.PaymentBill,
                             BCDate = gdo.BCDate,
                             diffdate = gdo.BeacukaiDate == DateTimeOffset.MinValue ? "No BC Belum Diinput" : Math.Abs((gdo.date.ToOffset(new TimeSpan(offset, 0, 0)) - gdo.BeacukaiDate.ToOffset(new TimeSpan(offset, 0, 0))).Days).ToString()
                         });

            Dictionary<string, double> q = new Dictionary<string, double>();
            List<GarmentDeliveryOrderReportViewModel> urn = new List<GarmentDeliveryOrderReportViewModel>();
            foreach (GarmentDeliveryOrderReportViewModel data in Query.ToList())
            {
                double value;
                if (q.TryGetValue(data.productCode + data.ePONo + data.ePODetailId, out value))
                {
                    q[data.productCode + data.ePONo + data.ePODetailId] -= data.dOQuantity;
                    data.remainingQuantity = q[data.productCode + data.ePONo + data.ePODetailId];
                    urn.Add(data);
                }
                else
                {
                    q[data.productCode + data.ePONo + data.ePODetailId] = data.remainingQuantity - data.dOQuantity;
                    data.remainingQuantity = q[data.productCode + data.ePONo + data.ePODetailId];
                    urn.Add(data);
                }
            }
            return Query = urn.AsQueryable();
        }

        public Tuple<List<GarmentDeliveryOrderReportViewModel>, int> GetReportDO(string no, string poEksNo, long supplierId, string billNo, string paymentBill, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)

        {
            var Query = GetReportQueryDO(no, poEksNo, supplierId, billNo, paymentBill, dateFrom, dateTo, offset);


            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderByDescending(b => b.supplierDoDate).ThenByDescending(b => b.createdUtc);
            }


            Pageable<GarmentDeliveryOrderReportViewModel> pageable = new Pageable<GarmentDeliveryOrderReportViewModel>(Query, page - 1, size);
            List<GarmentDeliveryOrderReportViewModel> Data = pageable.Data.ToList<GarmentDeliveryOrderReportViewModel>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }

        public MemoryStream GenerateExcelDO(string no, string poEksNo, long supplierId, string billNo, string paymentBill, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetReportQueryDO(no, poEksNo, supplierId, billNo, paymentBill, dateFrom, dateTo, offset);
            Query = Query.OrderByDescending(b => b.supplierDoDate).ThenByDescending(b => b.createdUtc);
            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Tiba", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jenis Supplier", DataType = typeof(String) });
            //result.Columns.Add(new DataColumn() { ColumnName = "Pengiriman", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor BL/AWB", DataType = typeof(String) });
            //result.Columns.Add(new DataColumn() { ColumnName = "Dikenakan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor PO Eksternal", DataType = typeof(String) });
            //result.Columns.Add(new DataColumn() { ColumnName = "Nomor PR", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Referensi PR", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Dipesan", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Diterima", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Rate", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Staff Pembelian (S/J)", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Staff Pembelian (P/O)", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No BC", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tipe BC", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal BC", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Input BC", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No BP Besar", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No BP Kecil", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor Bon Unit", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Bon Unit", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Bon Unit", DataType = typeof(Decimal) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Bon Unit", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Unit Yang Membutuhkan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Nota Intern", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Term Pembayaran", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Selisih Tanggal Tiba - Tanggal Input BC", DataType = typeof(String) });

            if (Query.ToArray().Count() == 0)
                // result.Rows.Add("", "", "", "", "", "", "", "", "", "", 0, 0, 0, ""); // to allow column name to be generated properly for empty data as template
                //result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", 0, 0, "", 0, "", 0, "", "", "", "", "", "", "", "", "", "", "", 0, "", "", "", "", "");
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", 0, 0, "", 0, "", 0, "", "", "", "", "", "", "", "", "", "", "", 0, "", "", "", "", "");
            //result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", 0, 0, "", 0, "", 0, "", "", "", "", "", "", "", "", "", "", "", 0, "", "", "", "");
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;
                    string date = item.date == null ? "-" : item.date.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string supplierDoDate = item.supplierDoDate == new DateTime(1970, 1, 1) ? "-" : item.supplierDoDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string dikenakan = item.isCustoms == true ? "Ya" : "Tidak";
                    string jenissupp = item.shipmentType == "" ? "Local" : "Import";
                    string URNDate = item.URNDate == new DateTime(1970, 1, 1) ? "-" : item.URNDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string BcinputDate = item.BeacukaiDate == DateTimeOffset.MinValue ? "-" : item.BeacukaiDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string BcDate = item.BCDate == new DateTime(1970, 1, 1) ? "-" : item.BCDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    // result.Rows.Add(index, item.supplierCode, item.supplierName, item.no, supplierDoDate, date, item.ePONo, item.productCode, item.productName, item.productRemark, item.dealQuantity, item.dOQuantity, item.remainingQuantity, item.uomUnit);
                    result.Rows.Add(index, item.no, supplierDoDate, date, item.supplierName, jenissupp, item.shipmentNo, item.ePONo, item.roNo, item.prRefNo, item.productCode, item.productName, item.dealQuantity, item.dOQuantity, item.uomUnit, item.price, item.doCurrencyCode, item.doCurrencyRate, item.productRemark, item.createdBy, item.EPOcreatedBy, item.BeacukaiNo, item.BeacukaiType, BcDate, BcinputDate, item.BillNo, item.PaymentBill, item.URNNo, URNDate, item.urnQuantity, item.urnUom, item.UnitName, item.INNo, item.TermPayment, item.diffdate);
                    //result.Rows.Add(index, item.no, supplierDoDate, date, item.supplierName, jenissupp, item.shipmentNo, item.ePONo, item.roNo, item.prRefNo, item.productCode, item.productName, item.dealQuantity, item.dOQuantity, item.uomUnit, item.price, item.doCurrencyCode, item.doCurrencyRate, item.productRemark, item.createdBy, item.EPOcreatedBy, item.BeacukaiNo, item.BeacukaiType, BcDate, BcinputDate, item.BillNo, item.PaymentBill, item.URNNo, URNDate, item.urnQuantity, item.urnUom, item.UnitName, item.INNo, item.diffdate);

                    //result.Rows.Add(index, item.no, supplierDoDate, date, item.supplierName, jenissupp, item.shipmentNo, item.ePONo, item.roNo, item.prRefNo, item.productCode, item.productName, item.dealQuantity, item.dOQuantity, item.uomUnit, item.price, item.doCurrencyCode, item.doCurrencyRate, item.productRemark, item.createdBy, item.EPOcreatedBy, item.BeacukaiNo, item.BeacukaiType, BcDate, BcinputDate, item.BillNo, item.PaymentBill, item.URNNo, URNDate, item.urnQuantity, item.urnUom, item.UnitName, item.INNo);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }

        //
        public IQueryable<OverBudgetQtyReportViewModel> GetReportQueryOB(string epoNo, string poNo, string inNo)
        {
            var reportResult = new List<OverBudgetQtyReportViewModel>();

            var Query1 = (from a in dbContext.GarmentExternalPurchaseOrders 
                         join b in dbContext.GarmentExternalPurchaseOrderItems on a.Id equals b.GarmentEPOId
                         join c in dbContext.GarmentDeliveryOrderDetails on b.Id equals c.EPOItemId
                         join d in dbContext.GarmentDeliveryOrderItems on c.GarmentDOItemId equals d.Id
                         join e in dbContext.GarmentDeliveryOrders on d.GarmentDOId equals e.Id
                          join f in dbContext.GarmentInvoiceDetails on c.Id equals f.DODetailId into aa
                          from DInv in aa.DefaultIfEmpty()
                          join g in dbContext.GarmentInvoiceItems on DInv.InvoiceItemId equals g.Id into bb
                          from IInv in bb.DefaultIfEmpty()
                          join h in dbContext.GarmentInvoices on IInv.InvoiceId equals h.Id into cc
                          from Inv in cc.DefaultIfEmpty()

                          join i in dbContext.GarmentInternNoteItems on Inv.Id equals i.InvoiceId into dd
                          from INItm in dd.DefaultIfEmpty()

                          join j in dbContext.GarmentInternNotes on INItm.GarmentINId equals j.Id into ee
                          from IN in ee.DefaultIfEmpty()

                          where a.EPONo == (string.IsNullOrWhiteSpace(epoNo) ? a.EPONo : epoNo)
                                && b.PO_SerialNumber == (string.IsNullOrWhiteSpace(poNo) ? b.PO_SerialNumber : poNo)
                                && IN.INNo == (string.IsNullOrWhiteSpace(inNo) ? IN.INNo : inNo)

                          select new OverBudgetQtyReportViewModel
                          {
                              epoNo = a.EPONo,
                              orderDate = a.OrderDate,
                              createdDate = e.CreatedUtc,
                              supplierCode = a.SupplierCode,
                              supplierName = a.SupplierName,
                              codeRequirement = c.CodeRequirment,
                              poNo = b.PO_SerialNumber,
                              productCode = b.ProductCode,
                              productName = b.ProductName,
                              poQuantity = b.SmallQuantity,
                              doQuantity = c.DOQuantity,
                              obQuantity = 0,
                              percentOB = 0,
                              doNo = e.DONo,
                              doDate = e.DODate,
                              invoiceNo = Inv == null ? "-" : Inv.InvoiceNo,
                              invoiceDate = Inv == null ? new DateTime(1970, 1, 1) : Inv.InvoiceDate,
                              inNo = IN == null ? "-" : IN.INNo,
                              inDate = IN == null ? new DateTime(1970, 1, 1) : IN.INDate,
                              remark = "",
                          }
                               ).OrderBy(b => b.epoNo).ThenBy(b => b.poNo).ThenBy(b => b.createdDate);

            //

            double QtyDO = 0;

            foreach (var item in Query1)
            {
                QtyDO += item.doQuantity;

                OverBudgetQtyReportViewModel _newData = new OverBudgetQtyReportViewModel
                 
                {
                    epoNo = item.epoNo,
                    orderDate = item.orderDate,
                    createdDate = item.createdDate,
                    supplierCode = item.supplierCode,
                    supplierName = item.supplierName,
                    poNo = item.poNo,
                    productCode = item.productCode,
                    productName = item.productName,
                    poQuantity = item.poQuantity,
                    doQuantity = item.doQuantity,
                    codeRequirement = item.codeRequirement,
                    accmltvQuantity = QtyDO,
                    obQuantity = QtyDO - item.poQuantity <= 0 ? 0 : QtyDO - item.poQuantity,
                    percentOB = QtyDO - item.poQuantity <= 0 ? 0 : Math.Round(((QtyDO - item.poQuantity) / item.poQuantity) * 100, 0),
                    doNo = item.doNo,
                    doDate = item.doDate,
                    invoiceNo = item.invoiceNo,
                    invoiceDate = item.invoiceDate,
                    inNo = item.inNo,
                    inDate = item.inDate,
                    remark = item.codeRequirement == "BB" ? (Math.Round(((QtyDO - item.poQuantity) / item.poQuantity) * 100, 0) > 50 ? "PRESIDEN DIREKTUR" : (Math.Round(((QtyDO - item.poQuantity) / item.poQuantity) * 100, 0) > 30 && Math.Round(((QtyDO - item.poQuantity) / item.poQuantity) * 100, 0) <= 50) ? "DIREKTUR KEUANGAN" : (Math.Round(((QtyDO - item.poQuantity) / item.poQuantity) * 100, 0) > 10 && Math.Round(((QtyDO - item.poQuantity) / item.poQuantity) * 100, 0) <= 30) ? "KABAG PEMBELIAN" : "-") : (Math.Round(((QtyDO - item.poQuantity) / item.poQuantity) * 100, 0) > 30 ? "DIREKTUR KEUANGAN" : (Math.Round(((QtyDO - item.poQuantity) / item.poQuantity) * 100, 0) > 10 && Math.Round(((QtyDO - item.poQuantity) / item.poQuantity) * 100, 0) <= 30) ? "KADIV PEMBELIAN" : (Math.Round(((QtyDO - item.poQuantity) / item.poQuantity) * 100, 0) > 0 && Math.Round(((QtyDO - item.poQuantity) / item.poQuantity) * 100, 0) <= 10) ? "KABAG PEMBELIAN" : "-"),
                };
                reportResult.Add(_newData);
            }

            return reportResult.AsQueryable();
        }

        public Tuple<List<OverBudgetQtyReportViewModel>, int> GetReportOB(string epoNo, string poNo, string inNo, int page, int size, string Order)

        {
            var Query = GetReportQueryOB(epoNo, poNo, inNo);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderBy(b => b.epoNo).ThenBy(b => b.poNo).ThenBy(b => b.createdDate);
            }

            Pageable<OverBudgetQtyReportViewModel> pageable = new Pageable<OverBudgetQtyReportViewModel>(Query, page - 1, size);
            List<OverBudgetQtyReportViewModel> Data = pageable.Data.ToList<OverBudgetQtyReportViewModel>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }

        public MemoryStream GenerateExcelOB(string epoNo, string poNo, string inNo)
        {
            var Query = GetReportQueryOB(epoNo, poNo, inNo);
            //Query = Query.OrderBy(b => b.epoNo).ThenBy(b => b.poNo).ThenBy(b => b.createdDate);
            
            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PO External", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl PO External", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Ref PO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah PO", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah SJ", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah OB", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah OB %", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Invoice", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Nota Intern", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Nota Intern", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan Approval", DataType = typeof(String) });

            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", 0, 0, 0, 0, "", "", "", "", "", "", "");
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;
                    
                    string EPODate = item.orderDate == new DateTime(1970, 1, 1) ? "-" : item.orderDate.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd-MM-yyyy", new CultureInfo("id-ID"));
                    string DODate = item.doDate == DateTimeOffset.MinValue ? "-" : item.doDate.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd-MM-yyyy", new CultureInfo("id-ID"));
                    string InvDate = item.invoiceDate == new DateTime(1970, 1, 1) ? "-" : item.invoiceDate.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd-MM-yyyy", new CultureInfo("id-ID"));
                    string NIDate = item.inDate == new DateTime(1970, 1, 1) ? "-" : item.inDate.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd-MM-yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(index, item.epoNo, EPODate, item.supplierCode, item.supplierName, item.poNo, item.productCode, item.productName, item.poQuantity, item.doQuantity, item.obQuantity, item.percentOB, item.doNo, DODate, item.invoiceNo, InvDate, item.inNo, NIDate, item.remark);
                    
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }
        //
    }

    public class AccuracyOfArrivalReportDetail
    {
        internal DateTimeOffset EPODate;

        public int No { get; internal set; }
        public long SupplierId { get; internal set; }
        public string SupplierCode { get; internal set; }
        public string SupplierName { get; internal set; }
        public DateTimeOffset PRDate { get; internal set; }
        public DateTime IPODate { get; internal set; }
        public string DONo { get; internal set; }
        public string ProductCode { get; internal set; }
        public string ProductName { get; internal set; }
        public string ProductRemark { get; internal set; }
        public string Article { get; internal set; }
        public string RONo { get; internal set; }
        public DateTimeOffset ShipmentDate { get; internal set; }
        public DateTimeOffset DODate { get; internal set; }
        public string OKStatus { get; internal set; }
        public decimal Status { get; internal set; }
        public decimal Status1 { get; internal set; }
        public string Staff { get; internal set; }
        public string POSerialNumber { get; internal set; }
    }

    public class SelectedGarmentDeliveryOrderDetail
    {
        public SelectedGarmentDeliveryOrderDetail()
        {
        }

        public long Id { get; set; }
        public long PRId { get; set; }
        public long GarmentDOItemId { get; set; }
    }

    public class AccuracyOfArrivalReportHeader
    {
        public int No { get; set; }
        public long SupplierId { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public decimal OKStatusPercentage { get; set; }
        public decimal NotOKStatusPercentage { get; set; }
        public int Total { get; set; }
        public decimal OKTotal { get; internal set; }
        public decimal NotOKTotal { get; internal set; }
    }

    public class AccuracyOfArrivalReportHeaderResult
    {
        public AccuracyOfArrivalReportHeaderResult()
        {
            ReportHeader = new List<AccuracyOfArrivalReportHeader>();
        }
        public List<AccuracyOfArrivalReportHeader> ReportHeader { get; set; }
        public int Total { get; set; }
    }

    public class SelectedGarmentDeliveryOrder
    {
        public SelectedGarmentDeliveryOrder(GarmentDeliveryOrder garmentDeliveryOrder)
        {
            SupplierId = garmentDeliveryOrder.SupplierId;
            SupplierCode = garmentDeliveryOrder.SupplierCode;
            SupplierName = garmentDeliveryOrder.SupplierName;
            GarmentDeliveryOrderId = garmentDeliveryOrder.Id;
            DODate = garmentDeliveryOrder.ArrivalDate;
            DONo = garmentDeliveryOrder.DONo;
            CreatedBy = garmentDeliveryOrder.CreatedBy;
        }

        public long SupplierId { get; }
        public string SupplierCode { get; }
        public string SupplierName { get; }
        public long GarmentDeliveryOrderId { get; }
        public DateTimeOffset DODate { get; }
        public string DONo { get; }
        public string CreatedBy { get; }
    }
}