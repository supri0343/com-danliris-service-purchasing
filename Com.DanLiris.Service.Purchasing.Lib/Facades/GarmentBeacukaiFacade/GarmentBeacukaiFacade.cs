﻿using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentBeacukaiModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderNonPOModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubconDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services.GarmentDebtBalance;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentBeacukaiViewModel;
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
using Microsoft.Extensions.DependencyInjection;
using Com.DanLiris.Service.Purchasing.Lib.Facades.LogHistoryFacade;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentBeacukaiFacade
{
    public class GarmentBeacukaiFacade : IGarmentBeacukaiFacade
    {
        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentBeacukai> dbSet;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentDeliveryOrder> dbSetDeliveryOrder;
        private readonly DbSet<GarmentDeliveryOrderNonPO> dbSetDeliveryOrderNonPOs;
        private readonly DbSet<GarmentSubconDeliveryOrder> dbSetSubconDeliveryOrders ;
        private readonly IGarmentDebtBalanceService _garmentDebtBalanceService;
        private string USER_AGENT = "Facade";
        private readonly ILogHistoryFacades logHistoryFacades;
        public GarmentBeacukaiFacade(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentBeacukai>();
            this.dbSetDeliveryOrder = dbContext.Set<GarmentDeliveryOrder>();
            this.dbSetDeliveryOrderNonPOs = dbContext.Set<GarmentDeliveryOrderNonPO>();
            this.dbSetSubconDeliveryOrders = dbContext.Set<GarmentSubconDeliveryOrder>();
            _garmentDebtBalanceService = serviceProvider.GetService<IGarmentDebtBalanceService>();
            this.serviceProvider = serviceProvider;
            logHistoryFacades = serviceProvider.GetService<ILogHistoryFacades>();
        }

        public Tuple<List<GarmentBeacukai>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentBeacukai> Query = this.dbSet.Include(m => m.Items);

            List<string> searchAttributes = new List<string>()
            {
                "beacukaiNo", "suppliername","customsType","items.garmentdono"
            };

            Query = QueryHelper<GarmentBeacukai>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentBeacukai>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentBeacukai>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentBeacukai> pageable = new Pageable<GarmentBeacukai>(Query, Page - 1, Size);
            List<GarmentBeacukai> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentBeacukai ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
             .Include(m => m.Items)
             .FirstOrDefault();
            return model;
        }

        public string? GenerateBillNo()
        {
            string BillNo = null;
            GarmentDeliveryOrder deliveryOrder = (from data in dbSetDeliveryOrder
                                                  orderby data.BillNo descending
                                                  select data).FirstOrDefault();
            string year = DateTimeOffset.Now.Year.ToString().Substring(2, 2);
            string month = DateTimeOffset.Now.Month.ToString("D2");
            string hour = (DateTimeOffset.Now.Hour + 7).ToString("D2");
            string day = DateTimeOffset.Now.Day.ToString("D2");
            string minute = DateTimeOffset.Now.Minute.ToString("D2");
            string second = DateTimeOffset.Now.Second.ToString("D2");
            string formatDate = year + month + day + hour + minute + second;
            int counterId = 0;
            if (deliveryOrder.BillNo != null)
            {
                BillNo = deliveryOrder.BillNo;
                string months = BillNo.Substring(4, 2);
                string number = BillNo.Substring(14);
                if (months == DateTimeOffset.Now.Month.ToString("D2"))
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
            BillNo = "BP" + formatDate + counterId.ToString("D6");
            return BillNo;

        }

        public (string format, int counterId) GeneratePaymentBillNo()
        {
            string PaymentBill = null;
            GarmentDeliveryOrder deliveryOrder = (from data in dbSetDeliveryOrder
                                                  orderby data.PaymentBill descending
                                                  select data).FirstOrDefault();
            string year = DateTimeOffset.Now.Year.ToString().Substring(2, 2);
            string month = DateTimeOffset.Now.Month.ToString("D2");
            string day = DateTimeOffset.Now.Day.ToString("D2");
            string formatDate = year + month + day;
            int counterId = 0;
            if (deliveryOrder.BillNo != null)
            {
                PaymentBill = deliveryOrder.PaymentBill;
                string date = PaymentBill.Substring(2, 6);
                string number = PaymentBill.Substring(8);
                if (date == formatDate)
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
            //PaymentBill = "BB" + formatDate + counterId.ToString("D3");

            return (string.Concat("BB", formatDate), counterId);

        }
        public async Task<int> Create(GarmentBeacukai model, string username, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {

                    EntityExtension.FlagForCreate(model, username, USER_AGENT);

                    var lastPaymentBill = GeneratePaymentBillNo();

                    foreach (GarmentBeacukaiItem item in model.Items)
                    {
                        if (item.IsPO)
                        {
                            GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.Include(m => m.Items)
                                                            .ThenInclude(i => i.Details).FirstOrDefault(s => s.Id == item.GarmentDOId);
                            if (deliveryOrder != null)
                            {

                                if (model.BillNo == "" | model.BillNo == null)
                                {
                                    deliveryOrder.BillNo = GenerateBillNo();

                                }
                                else
                                {
                                    deliveryOrder.BillNo = model.BillNo;
                                }
                                deliveryOrder.PaymentBill = string.Concat(lastPaymentBill.format, (lastPaymentBill.counterId++).ToString("D3"));
                                //deliveryOrder.CustomsId = model.Id;
                                double qty = 0;
                                foreach (var deliveryOrderItem in deliveryOrder.Items)
                                {
                                    foreach (var detail in deliveryOrderItem.Details)
                                    {
                                        qty += detail.DOQuantity;
                                    }
                                }
                                item.TotalAmount = Convert.ToDecimal(deliveryOrder.TotalAmount);
                                item.TotalQty = qty;
                                EntityExtension.FlagForCreate(item, username, USER_AGENT);
                            }
                        }
                        else
                        {
                            if (model.CustomsType == "BC 262")
                            {
                                GarmentDeliveryOrderNonPO deliveryOrderNonPO = dbSetDeliveryOrderNonPOs.Include(x => x.Items).FirstOrDefault(x => x.Id == item.GarmentDOId);
                                if (deliveryOrderNonPO != null)
                                {

                                    if (model.BillNo == "" | model.BillNo == null)
                                    {
                                        deliveryOrderNonPO.BillNo = GenerateBillNo();

                                    }
                                    else
                                    {
                                        deliveryOrderNonPO.BillNo = model.BillNo;
                                    }
                                    deliveryOrderNonPO.PaymentBill = string.Concat(lastPaymentBill.format, (lastPaymentBill.counterId++).ToString("D3"));
                                    //deliveryOrderNonPO.CustomsId = model.Id;
                                    double qty = 0;
                                    double totalAmount = 0;
                                    foreach (var deliveryOrderItem in deliveryOrderNonPO.Items)
                                    {
                                        {
                                            qty += deliveryOrderItem.Quantity;
                                            totalAmount += deliveryOrderItem.Quantity * deliveryOrderItem.PricePerDealUnit;
                                        }
                                    }
                                    item.TotalAmount = Convert.ToDecimal(totalAmount);
                                    item.TotalQty = qty;
                                    EntityExtension.FlagForCreate(item, username, USER_AGENT);
                                }
                            }
                            else if (model.CustomsType == "BC 40" || model.CustomsType == "BC 27")
                            {
                                GarmentSubconDeliveryOrder deliveryOrderNonPO = dbSetSubconDeliveryOrders.Include(x => x.Items).FirstOrDefault(x => x.Id == item.GarmentDOId);
                                if (deliveryOrderNonPO != null)
                                {
                                    double qty = 0;
                                    double totalAmount = 0;
                                    foreach (var deliveryOrderItem in deliveryOrderNonPO.Items)
                                    {
                                        {
                                            qty += deliveryOrderItem.DOQuantity;
                                            totalAmount += deliveryOrderItem.DOQuantity * deliveryOrderItem.PricePerDealUnit;
                                        }
                                    }
                                    item.TotalAmount = Convert.ToDecimal(totalAmount);
                                    item.TotalQty = qty;
                                    EntityExtension.FlagForCreate(item, username, USER_AGENT);
                                }
                            }
                        }
                        
                    }

                    this.dbSet.Add(model);
                    Created = await dbContext.SaveChangesAsync();

                    foreach (GarmentBeacukaiItem item in model.Items)
                    {
                        if (item.IsPO)
                        {
                            GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.Include(m => m.Items)
                                                           .ThenInclude(i => i.Details).FirstOrDefault(s => s.Id == item.GarmentDOId);
                            if (deliveryOrder != null)
                            {
                                deliveryOrder.CustomsId = model.Id;
                            }
                        }
                        else
                        {
                            if (model.CustomsType == "BC 262")
                            {
                                GarmentDeliveryOrderNonPO deliveryOrderNonPO = dbSetDeliveryOrderNonPOs.Include(m => m.Items)
                                                          .FirstOrDefault(s => s.Id == item.GarmentDOId);
                                if (deliveryOrderNonPO != null)
                                {
                                    deliveryOrderNonPO.CustomsId = model.Id;
                                }
                            }
                            else if (model.CustomsType == "BC 40" || model.CustomsType == "BC 27")
                            {
                                GarmentSubconDeliveryOrder deliveryOrderNonPO = dbSetSubconDeliveryOrders.Include(x => x.Items).FirstOrDefault(x => x.Id == item.GarmentDOId);
                                if (deliveryOrderNonPO != null)
                                {
                                    deliveryOrderNonPO.CustomsId = model.Id;
                                    deliveryOrderNonPO.BeacukaiNo = model.BeacukaiNo;
                                    deliveryOrderNonPO.BeacukaiType = model.CustomsType;
                                    deliveryOrderNonPO.BeacukaiDate = model.BeacukaiDate;
                                }
                            }
                        }
                    }

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Create Bea Cukai - " + model.BeacukaiNo);

                    Created = await dbContext.SaveChangesAsync();

                    foreach (var item in model.Items)
                    {
                        if (item.IsPO)
                        {
                            var deliveryOrder = dbSetDeliveryOrder
                           .Include(m => m.Items)
                           .ThenInclude(i => i.Details)
                           .FirstOrDefault(s => s.Id == item.GarmentDOId);

                            var deliveryOrderEPOIds = deliveryOrder.Items.Select(s => s.EPOId);
                            var garmentExternalOrder = dbContext.GarmentExternalPurchaseOrders.Where(s => deliveryOrderEPOIds.Contains(s.Id));

                            if (deliveryOrder != null)
                            {
                                var dppAmount = 0.0;
                                var currencyDPPAmount = 0.0;

                                if (deliveryOrder.DOCurrencyCode == "IDR")
                                {
                                    dppAmount = deliveryOrder.TotalAmount;
                                }
                                else
                                {
                                    currencyDPPAmount = deliveryOrder.TotalAmount;
                                    dppAmount = deliveryOrder.TotalAmount * deliveryOrder.DOCurrencyRate.GetValueOrDefault();
                                }

                                //var categories = deliveryOrder.Items.SelectMany(doItem => doItem.Details).Select(detail => detail.CodeRequirment);
                                var categories = string.Join(',', garmentExternalOrder.Select(s => s.Category).ToList().GroupBy(s => s).Select(s => s.Key));
                                var paymentMethod = garmentExternalOrder.FirstOrDefault().PaymentType;
                                var productNames = string.Join(", ", deliveryOrder.Items.SelectMany(doItem => doItem.Details).Select(doDetail => doDetail.ProductName).ToList());

                                await _garmentDebtBalanceService.CreateFromCustoms(new CustomsFormDto(0, string.Join("\n", categories), deliveryOrder.BillNo, deliveryOrder.PaymentBill, (int)deliveryOrder.Id, deliveryOrder.DONo, (int)model.SupplierId, model.SupplierCode, model.SupplierName, deliveryOrder.SupplierIsImport, (int)deliveryOrder.DOCurrencyId.GetValueOrDefault(), deliveryOrder.DOCurrencyCode, deliveryOrder.DOCurrencyRate.GetValueOrDefault(), productNames, deliveryOrder.ArrivalDate, dppAmount, currencyDPPAmount, paymentMethod));
                            }
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
        public int Delete(int id, string username)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var model = this.dbSet
                        .Include(d => d.Items)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    EntityExtension.FlagForDelete(model, username, USER_AGENT);

                    foreach (var item in model.Items)
                    {
                        if (item.IsPO)
                        {
                            GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.GarmentDOId);
                            if (deliveryOrder != null)
                            {
                                deliveryOrder.BillNo = null;
                                deliveryOrder.PaymentBill = null;
                                deliveryOrder.CustomsId = 0;
                                EntityExtension.FlagForDelete(item, username, USER_AGENT);

                                var deleted = _garmentDebtBalanceService.RemoveCustoms((int)deliveryOrder.Id).Result;
                            }
                        }
                        else
                        {
                            if (model.CustomsType == "BC 262")
                            {
                                GarmentDeliveryOrderNonPO deliveryOrder = dbSetDeliveryOrderNonPOs.FirstOrDefault(s => s.Id == item.GarmentDOId);
                                if (deliveryOrder != null)
                                {
                                    deliveryOrder.BillNo = null;
                                    deliveryOrder.PaymentBill = null;
                                    deliveryOrder.CustomsId = 0;
                                    EntityExtension.FlagForDelete(item, username, USER_AGENT);
                                }
                            }
                            else if (model.CustomsType == "BC 40" || model.CustomsType == "BC 27")
                            {
                                GarmentSubconDeliveryOrder deliveryOrder = dbSetSubconDeliveryOrders.FirstOrDefault(x => x.Id == item.GarmentDOId);
                                if (deliveryOrder != null)
                                {
                                    deliveryOrder.CustomsId = 0;
                                    deliveryOrder.BeacukaiNo = null;
                                    deliveryOrder.BeacukaiType = null;
                                    deliveryOrder.BeacukaiDate = DateTimeOffset.MinValue;
                                    EntityExtension.FlagForDelete(item, username, USER_AGENT);
                                }
                            }
                               
                        }
                    }

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Delete Bea Cukai - " + model.BeacukaiNo);

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

        public HashSet<long> GetGarmentBeacukaiId(long id)
        {
            return new HashSet<long>(dbContext.GarmentBeacukaiItems.Where(d => d.GarmentBeacukai.Id == id).Select(d => d.Id));
        }

        public async Task<int> Update(int id, GarmentBeacukaiViewModel vm, GarmentBeacukai model, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    GarmentBeacukai modelBC = dbSet.AsNoTracking().Include(a => a.Items).FirstOrDefault(s => s.Id == model.Id);

                    EntityExtension.FlagForUpdate(model, user, USER_AGENT);

                    var lastPaymentBill = GeneratePaymentBillNo();

                    foreach (GarmentBeacukaiItem item in model.Items)
                    {
                        GarmentBeacukaiItem oldItem = modelBC.Items.FirstOrDefault(s => s.Id.Equals(item.Id));
                        GarmentBeacukaiItemViewModel itemVM = vm.items.FirstOrDefault(s => s.deliveryOrder.Id.Equals(item.GarmentDOId));
                        if (itemVM.selected)
                        {
                            if (oldItem == null)
                            {
                                if (item.IsPO)
                                {
                                    GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.Include(m => m.Items)
                                                            .ThenInclude(i => i.Details).FirstOrDefault(s => s.Id == item.GarmentDOId);
                                    if (deliveryOrder != null)
                                    {

                                        var deliveryOrderEPOIds = deliveryOrder.Items.Select(s => s.EPOId);
                                        var garmentExternalOrder = dbContext.GarmentExternalPurchaseOrders.Where(s => deliveryOrderEPOIds.Contains(s.Id));

                                        if (model.BillNo == "" | model.BillNo == null)
                                        {
                                            deliveryOrder.BillNo = GenerateBillNo();

                                        }
                                        else
                                        {
                                            deliveryOrder.BillNo = model.BillNo;
                                        }
                                        deliveryOrder.PaymentBill = string.Concat(lastPaymentBill.format, (lastPaymentBill.counterId++).ToString("D3"));
                                        //deliveryOrder.CustomsId = model.Id;
                                        double qty = 0;
                                        foreach (var deliveryOrderItem in deliveryOrder.Items)
                                        {
                                            foreach (var detail in deliveryOrderItem.Details)
                                            {
                                                qty += detail.DOQuantity;
                                            }
                                        }
                                        item.TotalAmount = Convert.ToDecimal(deliveryOrder.TotalAmount);
                                        item.TotalQty = qty;
                                        EntityExtension.FlagForCreate(item, user, USER_AGENT);

                                        deliveryOrder.CustomsId = model.Id;

                                        var dppAmount = 0.0;
                                        var currencyDPPAmount = 0.0;

                                        if (deliveryOrder.DOCurrencyCode == "IDR")
                                        {
                                            dppAmount = deliveryOrder.TotalAmount;
                                        }
                                        else
                                        {
                                            currencyDPPAmount = deliveryOrder.TotalAmount;
                                            dppAmount = deliveryOrder.TotalAmount * deliveryOrder.DOCurrencyRate.GetValueOrDefault();
                                        }

                                        var categories = string.Join(',', garmentExternalOrder.Select(s => s.Category).ToList().GroupBy(s => s).Select(s => s.Key));
                                        var paymentMethod = garmentExternalOrder.FirstOrDefault().PaymentType;
                                        var productNames = string.Join(", ", deliveryOrder.Items.SelectMany(doItem => doItem.Details).Select(doDetail => doDetail.ProductName).ToList());

                                        await _garmentDebtBalanceService.CreateFromCustoms(new CustomsFormDto(0, string.Join("\n", categories), deliveryOrder.BillNo, deliveryOrder.PaymentBill, (int)deliveryOrder.Id, deliveryOrder.DONo, (int)model.SupplierId, model.SupplierCode, model.SupplierName, deliveryOrder.SupplierIsImport, (int)deliveryOrder.DOCurrencyId.GetValueOrDefault(), deliveryOrder.DOCurrencyCode, deliveryOrder.DOCurrencyRate.GetValueOrDefault(), productNames, deliveryOrder.ArrivalDate, dppAmount, currencyDPPAmount, paymentMethod));


                                    }
                                }
                                else
                                {
                                    if(model.CustomsType == "BC 262")
                                    {
                                        GarmentDeliveryOrderNonPO deliveryOrderNonPO = dbSetDeliveryOrderNonPOs.Include(x => x.Items).FirstOrDefault(x => x.Id == item.GarmentDOId);
                                        if (deliveryOrderNonPO != null)
                                        {

                                            if (model.BillNo == "" | model.BillNo == null)
                                            {
                                                deliveryOrderNonPO.BillNo = GenerateBillNo();

                                            }
                                            else
                                            {
                                                deliveryOrderNonPO.BillNo = model.BillNo;
                                            }
                                            deliveryOrderNonPO.PaymentBill = string.Concat(lastPaymentBill.format, (lastPaymentBill.counterId++).ToString("D3"));
                                            //deliveryOrderNonPO.CustomsId = model.Id;
                                            double qty = 0;
                                            double totalAmount = 0;
                                            foreach (var deliveryOrderItem in deliveryOrderNonPO.Items)
                                            {
                                                {
                                                    qty += deliveryOrderItem.Quantity;
                                                    totalAmount += deliveryOrderItem.Quantity * deliveryOrderItem.PricePerDealUnit;
                                                }
                                            }
                                            item.TotalAmount = Convert.ToDecimal(totalAmount);
                                            item.TotalQty = qty;
                                            EntityExtension.FlagForCreate(item, user, USER_AGENT);
                                            deliveryOrderNonPO.CustomsId = model.Id;
                                        }
                                    }else if (model.CustomsType == "BC 40" || model.CustomsType == "BC 27")
                                    {
                                        GarmentSubconDeliveryOrder deliveryOrderNonPO = dbSetSubconDeliveryOrders.Include(x => x.Items).FirstOrDefault(x => x.Id == item.GarmentDOId);
                                        double qty = 0;
                                        double totalAmount = 0;
                                        foreach (var deliveryOrderItem in deliveryOrderNonPO.Items)
                                        {
                                            {
                                                qty += deliveryOrderItem.DOQuantity;
                                                totalAmount += deliveryOrderItem.DOQuantity * deliveryOrderItem.PricePerDealUnit;
                                            }
                                        }
                                        item.TotalAmount = Convert.ToDecimal(totalAmount);
                                        item.TotalQty = qty;
                                        EntityExtension.FlagForCreate(item, user, USER_AGENT);
                                        deliveryOrderNonPO.CustomsId = model.Id;
                                        deliveryOrderNonPO.BeacukaiNo = model.BeacukaiNo;
                                        deliveryOrderNonPO.BeacukaiType = model.CustomsType;
                                        deliveryOrderNonPO.BeacukaiDate = model.BeacukaiDate;
                                    }
                                    
                                }
                            }
                            else if (oldItem != null)
                            {
                                item.TotalAmount = oldItem.TotalAmount;
                                item.TotalQty = oldItem.TotalQty;

                                //Update BC NO
                                GarmentSubconDeliveryOrder deliveryOrder = dbSetSubconDeliveryOrders.FirstOrDefault(s => s.Id == item.GarmentDOId);
                                if(deliveryOrder != null)
                                {
                                    if (deliveryOrder.BeacukaiNo != model.BeacukaiNo)
                                    {
                                        deliveryOrder.BeacukaiNo = model.BeacukaiNo;
                                    }
                                    if (deliveryOrder.BeacukaiType != model.CustomsType)
                                    {
                                        deliveryOrder.BeacukaiType = model.CustomsType;
                                    }
                                    if (deliveryOrder.BeacukaiDate != model.BeacukaiDate)
                                    {
                                        deliveryOrder.BeacukaiDate = model.BeacukaiDate;
                                    }
                                    EntityExtension.FlagForUpdate(deliveryOrder, user, USER_AGENT);
                                }

                                EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                            }


                        }
                        else
                        {
                            if (item.IsPO)
                            {
                                EntityExtension.FlagForDelete(item, user, USER_AGENT);
                                GarmentDeliveryOrder deleteDO = dbContext.GarmentDeliveryOrders.FirstOrDefault(s => s.Id == item.GarmentDOId);
                                deleteDO.BillNo = null;
                                deleteDO.PaymentBill = null;
                                deleteDO.CustomsId = 0;

                                await _garmentDebtBalanceService.RemoveCustoms((int)deleteDO.Id);
                            }
                            else
                            {
                                if (model.CustomsType == "BC 262")
                                {
                                    EntityExtension.FlagForDelete(item, user, USER_AGENT);
                                    GarmentDeliveryOrderNonPO deleteDO = dbSetDeliveryOrderNonPOs.FirstOrDefault(s => s.Id == item.GarmentDOId);
                                    deleteDO.BillNo = null;
                                    deleteDO.PaymentBill = null;
                                    deleteDO.CustomsId = 0;
                                }
                                else if (model.CustomsType == "BC 40" || model.CustomsType == "BC 27")
                                {
                                    EntityExtension.FlagForDelete(item, user, USER_AGENT);
                                    GarmentSubconDeliveryOrder deleteDO = dbSetSubconDeliveryOrders.FirstOrDefault(x => x.Id == item.GarmentDOId);
                                    deleteDO.CustomsId = 0;
                                    deleteDO.BeacukaiNo = null;
                                    deleteDO.BeacukaiType = null;
                                    deleteDO.BeacukaiDate = DateTimeOffset.MinValue;

                                }
                            }
                            
                        }
                    }

                    this.dbSet.Update(model);

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Update Bea Cukai - " + model.BeacukaiNo);

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

        public List<object> ReadBCByPOSerialNumber(string Keyword = null, string Filter = "{}")
        {
            //var Query = this.dbSet.Where(entity => entity.IsPosted && !entity.IsClosed && !entity.IsCanceled).Select(entity => new { entity.Id});

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            string POSerialNumber = (FilterDictionary["POSerialNumber"] ?? "").Trim();
            //IQueryable<GarmentExternalPurchaseOrderItem> QueryItem = dbContext.GarmentExternalPurchaseOrderItems.Where(entity=>entity.RONo==RONo ); //CreatedUtc > DateTime(2018, 12, 31)

            var DODetails = dbContext.GarmentDeliveryOrderDetails.Where(o => o.POSerialNumber == POSerialNumber).Select(a => a.GarmentDOItemId);

            var QueryData = (from dod in DODetails
                            join doi in dbContext.GarmentDeliveryOrderItems on dod equals doi.Id
                            join DO in dbContext.GarmentDeliveryOrders on doi.GarmentDOId equals DO.Id
                            join bci in dbContext.GarmentBeacukaiItems on DO.Id equals bci.GarmentDOId
                            join bc in dbContext.GarmentBeacukais on bci.BeacukaiId equals bc.Id
                            select new 
                            {
                                bc.Id
                            });
            var Ids = QueryData.Select(a => a.Id).Distinct().ToList();
            var data = this.dbSet.Where(o => Ids.Contains(o.Id))
                .Select(bc => new { bc.Id, bc.BeacukaiNo, bc.BeacukaiDate, bc.CustomsType}).ToList();

            List<object> ListData = new List<object>();
            foreach(var item in QueryData)
            {
                var custom = data.FirstOrDefault(f => f.Id.Equals(item.Id));

                ListData.Add(new { custom.BeacukaiNo, custom.BeacukaiDate, custom.CustomsType, POSerialNumber });
            }
            return ListData.Distinct().ToList();
        }

        public List<object> ReadBCByPOSerialNumbers(string Keyword)
        {
            //var Query = this.dbSet.Where(entity => entity.IsPosted && !entity.IsClosed && !entity.IsCanceled).Select(entity => new { entity.Id});

            var pos = Keyword.Split(",").ToArray();

            //string POSerialNumber = (FilterDictionary["POSerialNumber"] ?? "").Trim();
            //IQueryable<GarmentExternalPurchaseOrderItem> QueryItem = dbContext.GarmentExternalPurchaseOrderItems.Where(entity=>entity.RONo==RONo ); //CreatedUtc > DateTime(2018, 12, 31)

            var DODetails = dbContext.GarmentDeliveryOrderDetails.Where(o => pos.Contains(o.POSerialNumber)).Select(a => new { a.GarmentDOItemId, a.POSerialNumber });

            var QueryData = (from dod in DODetails
                             join doi in dbContext.GarmentDeliveryOrderItems on dod.GarmentDOItemId equals doi.Id
                             join DO in dbContext.GarmentDeliveryOrders on doi.GarmentDOId equals DO.Id
                             join bci in dbContext.GarmentBeacukaiItems on DO.Id equals bci.GarmentDOId
                             join bc in dbContext.GarmentBeacukais on bci.BeacukaiId equals bc.Id
                             select new
                             {
                                 bc.Id,
                                 dod.POSerialNumber,
                                 bc.BeacukaiNo,
                                 bc.BeacukaiDate,
                                 bc.CustomsType
                             }).Distinct();

            // var Ids = QueryData.Select(a => a.POSerialNumber).Distinct().ToList();
            //var data = this.dbSet.Where(o => Ids.Contains(o.Id))
            //    .Select(bc => new { bc.Id, bc.BeacukaiNo, bc.BeacukaiDate, bc.CustomsType });
            var listdata = QueryData.GroupBy(x => x.POSerialNumber).Select(x => new
            {
                POSerialNumber = x.Key,
                customnos = x.Select(y=>y.BeacukaiNo).ToList(),
                customdate = x.Select(y => y.BeacukaiDate).ToList(),
                customtype = x.Select(y => y.CustomsType).ToList()
            }).ToList();

            List<object> ListData = new List<object>();
            foreach (var item in listdata)
            {
                // var customno = QueryData.Where(f => f.POSerialNumber.Equals(item)).Select(x=>x.BeacukaiNo).ToList();
                // var customdate = QueryData.Where(f => f.POSerialNumber.Equals(item)).Select(x => x.BeacukaiDate).ToList();
                // var customtype = QueryData.Where(f => f.POSerialNumber.Equals(item)).Select(x => x.CustomsType).ToList();

                ListData.Add(new { POSerialNumber = item.POSerialNumber, customnos = item.customnos, customdates = item.customdate, customtypes = item.customtype });
            }
            return ListData.Distinct().ToList();
        }

        //     public async Task<List<ImportValueViewModel>> ReadImportValue(string keyword)
        //     {
        //var query = dbContext.ImportValues.AsQueryable();
        //if (!string.IsNullOrEmpty(keyword))
        //	query = query.Where(s => s.Name.Contains(keyword));

        //return await query.Select(s=> new ImportValueViewModel { 
        //	Name = s.Name,
        //	Id = s.Id
        //	})
        //	.ToListAsync();
        //     }

        public List<object> ReadBCByContractNo(string contractNo)
        {
            var data = (from a in dbContext.GarmentBeacukais
                        join b in dbContext.GarmentBeacukaiItems on a.Id equals b.BeacukaiId
                        where a.IsDeleted == false && b.IsDeleted == false && a.SubconContractNo == contractNo
                        select new
                        {
                            bcNoIn = a.BeacukaiNo,
                            bcDateIn = a.BeacukaiDate,
                            quantityIn = b.TotalQty,
                            fintype = a.FinishedGoodType,
                            garmentDONo = b.GarmentDONo
                        }).GroupBy(x => new { x.bcDateIn, x.bcNoIn, x.fintype, x.garmentDONo }, (key, group) => new
                        {
                            bcNoIn = key.bcNoIn,
                            bcDateIn = key.bcDateIn,
                            quantityIn = group.Sum(x => x.quantityIn),
                            fintype = key.fintype,
                            gamentDONo = key.garmentDONo
                        });

            List<object> ListData = new List<object>(data);

            return ListData;

        }

        public List<object> ReadBCByContractNoforSubcon(string contractNo, string subconContractType, string subconCategory)
        {
            var dataDO = (from a in dbContext.GarmentBeacukais
                          join b in dbContext.GarmentBeacukaiItems on a.Id equals b.BeacukaiId
                          join c in dbContext.GarmentDeliveryOrders on b.GarmentDOId equals c.Id
                          join d in dbContext.GarmentDeliveryOrderItems on c.Id equals d.GarmentDOId
                          join e in dbContext.GarmentDeliveryOrderDetails on d.Id equals e.GarmentDOItemId
                          where a.IsDeleted == false && b.IsDeleted == false
                          && c.IsDeleted == false
                            && d.IsDeleted == false
                            && e.IsDeleted == false
                          && a.SubconContractNo == contractNo
                          && b.IsPO == true
                          select new
                          {
                              bcNoIn = a.BeacukaiNo,
                              bcDateIn = a.BeacukaiDate,
                              quantityIn = e.SmallQuantity,
                              fintype = a.FinishedGoodType,
                              garmentDONo = b.GarmentDONo,
                              roNo = e.RONo,
                              uomUnitIn = e.UomUnit,
                              subconContractId = a.SubconContractId
                          }).Distinct();
            //.GroupBy(x => new { x.bcDateIn, x.bcNoIn, x.fintype, x.garmentDONo, x.roNo }, (key, group) => new
            //{
            //    bcNoIn = key.bcNoIn,
            //    bcDateIn = key.bcDateIn,
            //    quantityIn = group.Sum(x => x.quantityIn),
            //    fintype = key.fintype,
            //    gamentDONo = key.garmentDONo,
            //    roNo = key.roNo,
            //    uomUnitIn = group.First().uomUnitIn,
            //    subconContractId = group.First().subconContractId
            //}).Distinct();
            var dataDONonPO = (from a in dbContext.GarmentBeacukais
                               join b in dbContext.GarmentBeacukaiItems on a.Id equals b.BeacukaiId
                               join c in dbContext.GarmentDeliveryOrderNonPOs on b.GarmentDOId equals c.Id
                               join d in dbContext.GarmentDeliveryOrderNonPOItems on c.Id equals d.GarmentDeliveryOrderNonPOId
                               //join e in dbContext.GarmentDeliveryOrderNonPO on d.Id equals e.GarmentDOItemId into k


                               //join e in dbContext.GarmentDeliveryOrderDetails on d.Id equals e.GarmentDOItemId
                               where a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                               && d.IsDeleted == false
                               && a.SubconContractNo == contractNo && !b.IsPO
                               select new
                               {
                                   bcNoIn = a.BeacukaiNo,
                                   bcDateIn = a.BeacukaiDate,
                                   quantityIn = d.Quantity,
                                   fintype = a.FinishedGoodType,
                                   garmentDONo = b.GarmentDONo,
                                   roNo = "-",
                                   uomUnitIn = d.UomUnit,
                                   subconContractId = a.SubconContractId
                               });
                          //.GroupBy(x => new { x.bcDateIn, x.bcNoIn, x.fintype, x.garmentDONo, x.roNo }, (key, group) => new
                          //{
                          //    bcNoIn = key.bcNoIn,
                          //    bcDateIn = key.bcDateIn,
                          //    quantityIn = group.Sum(x => x.quantityIn),
                          //    fintype = key.fintype,
                          //    gamentDONo = key.garmentDONo,
                          //    roNo = key.roNo,
                          //    uomUnitIn = group.First().uomUnitIn,
                          //    subconContractId = group.First().subconContractId
                          //}).Distinct();

            var data = dataDO.Union(dataDONonPO).ToList();

            List<object> ListData = new List<object>(data);

            return ListData;

        }

        public List<object> GetFinInSubcon(string contractNo)
        {
            var data = (from a in dbContext.GarmentBeacukais
                        join b in dbContext.GarmentDeliveryOrders on a.Id equals b.CustomsId
                        where a.IsDeleted == false && b.IsDeleted == false && a.SubconContractNo == contractNo
                        select new
                        {
                            bcNoIn = a.BeacukaiNo,
                            bcDateIn = a.BeacukaiDate,
                            garmentDOId = b.Id,
                            subconContractId = a.SubconContractId
                            //quantityIn = b.TotalQty,
                            //fintype = a.FinishedGoodType,
                            //garmentDONo = b.GarmentDONo
                        }).GroupBy(x => new { x.bcDateIn, x.bcNoIn, x.garmentDOId, x.subconContractId}, (key, group) => new
                        {
                            bcNoIn = key.bcNoIn,
                            bcDateIn = key.bcDateIn,
                            garmentDOId = key.garmentDOId,
                            subconContractId = key.subconContractId
                        });

            List<object> ListData = new List<object>(data);

            return ListData;

        }

        public List<object> GetBCDOUrn(string contractNo, string subconContractType, string subconCategory)
        {

            var data = (from a in dbContext.GarmentBeacukais
                        join b in dbContext.GarmentDeliveryOrders on a.Id equals b.CustomsId
                        join c in dbContext.GarmentUnitReceiptNotes on b.Id equals c.DOId
                        join d in dbContext.GarmentUnitReceiptNoteItems on c.Id equals d.URNId
                        where a.IsDeleted == false && b.IsDeleted == false && a.SubconContractNo == contractNo
                        //d.ProductName != "PROCESS"
                        //&& d.ProductName.ToLower() != "fabric" && d.ProductName.ToLower() != "process"
                        && (subconContractType == "SUBCON GARMENT" && subconCategory == "SUBCON CUTTING SEWING" ?
                          d.ProductName.ToLower() != "process" :

                          subconContractType == "SUBCON JASA" && subconCategory == "SUBCON JASA KOMPONEN" ?
                          d.ProductName.ToLower() != "fabric"

                          : d.ProductName.ToLower() != "fabric" && d.ProductName.ToLower() != "process")


                        select new
                        {
                            bcNoIn = a.BeacukaiNo,
                            bcDateIn = a.BeacukaiDate,
                            garmentDOId = b.Id,
                            subconContractId = a.SubconContractId,
                            subconConrtacNo = a.SubconContractNo,
                            urnNo = c.URNNo,
                            productName = d.ProductName,
                            quantityIn = d.SmallQuantity,
                            uomUnitIn = d.SmallUomUnit
                            //quantityIn = b.TotalQty,
                            //fintype = a.FinishedGoodType,
                            //garmentDONo = b.GarmentDONo
                        }).GroupBy(x => new { x.bcDateIn, x.bcNoIn, x.garmentDOId, x.subconContractId, x.urnNo, x.productName }, (key, group) => new
                        {
                            bcNoIn = key.bcNoIn,
                            bcDateIn = key.bcDateIn,
                            garmentDOId = key.garmentDOId,
                            subconContractId = key.subconContractId,
                            subconConrtacNo = group.First().subconConrtacNo,
                            urnNo = key.urnNo,
                            productName = key.productName,
                            quantityIn = group.Sum(x => x.quantityIn) ,
                            uomUnitIn = group.First().uomUnitIn
                        });

            List<object> ListData = new List<object>(data);

            return ListData;

        }

        public List<object> GetROByUenNo(string uenNo)
        {

            var uenNos = uenNo.Contains(",") ? uenNo.Split(",").ToList() : new List<string> { uenNo };

            var GarmentUEN = dbContext.GarmentUnitExpenditureNotes.Where(x => uenNos.Contains(x.UENNo.ToString())).Select(s => new { Id = s.Id, UENNo = s.UENNo }).ToList();
            //var GarmentUENList = GarmentUEN.ToList();
            var GarmentUENIds = GarmentUEN.Select(s => s.Id);

            var GarmentUENItems = dbContext.GarmentUnitExpenditureNoteItems.Where(x => GarmentUENIds.Contains(x.UENId)).Select(s => new { RONo = s.RONo, UENId = s.UENId }).ToList();



            var data = (from a in GarmentUEN
                        join b in GarmentUENItems on a.Id equals b.UENId

                        //&& uenNos.Contains(a.UENNo.ToString())
                        select new roViewModel
                        {
                            id = a.Id,
                            roNo = b.RONo,
                            uenNo = a.UENNo

                        })
                    .GroupBy(x => new { x.id }, (key, group) => new roViewModel
                    {
                        id = key.id,
                        roNo = group.First().roNo,
                        uenNo = group.First().uenNo
                    }).ToList();



            List<object> ListData = new List<object>(data);

            return ListData;

        }

        public class roViewModel
        {
            public long id { get; set; }
            public string? roNo { get; set; }
            public string? uenNo { get; set; }
        }


        //public List<object> GetBCDOUrnSewing(string contractNo)
        //{

        //    var data = (from a in dbContext.GarmentBeacukais
        //                join b in dbContext.GarmentDeliveryOrders on a.Id equals b.CustomsId
        //                join c in dbContext.GarmentUnitReceiptNotes on b.Id equals c.DOId
        //                join d in dbContext.GarmentUnitReceiptNoteItems on c.Id equals d.URNId
        //                where a.IsDeleted == false && b.IsDeleted == false && a.SubconContractNo == contractNo &&
        //                d.ProductName.ToLower() != "fabric" && d.ProductName.ToLower() != "process"
        //                //&& d.ProductName.ToLower() != "fabric" && d.ProductName.ToLower() != "process"
        //                //subconContractType == "SUBCON GARMENT" && subconCategory == "SUBCON CUTTING SEWING" ?
        //                //d.ProductName != "PROCESS" : d.ProductName.ToLower() != "fabric" && d.ProductName.ToLower() != "process"


        //                select new
        //                {
        //                    bcNoIn = a.BeacukaiNo,
        //                    bcDateIn = a.BeacukaiDate,
        //                    garmentDOId = b.Id,
        //                    subconContractId = a.SubconContractId,
        //                    subconConrtacNo = a.SubconContractNo,
        //                    urnNo = c.URNNo,
        //                    productName = d.ProductName,
        //                    quantityIn = d.SmallQuantity,
        //                    uomUnitIn = d.SmallUomUnit
        //                    //quantityIn = b.TotalQty,
        //                    //fintype = a.FinishedGoodType,
        //                    //garmentDONo = b.GarmentDONo
        //                }).GroupBy(x => new { x.bcDateIn, x.bcNoIn, x.garmentDOId, x.subconContractId, x.urnNo, x.productName }, (key, group) => new
        //                {
        //                    bcNoIn = key.bcNoIn,
        //                    bcDateIn = key.bcDateIn,
        //                    garmentDOId = key.garmentDOId,
        //                    subconContractId = key.subconContractId,
        //                    subconConrtacNo = group.First().subconConrtacNo,
        //                    urnNo = key.urnNo,
        //                    productName = key.productName,
        //                    quantityIn = group.Sum(x => x.quantityIn),
        //                    uomUnitIn = group.First().uomUnitIn
        //                });

        //    List<object> ListData = new List<object>(data);

        //    return ListData;

        //}
    }
}