﻿using Com.DanLiris.Service.Purchasing.Lib.Facades.LogHistoryFacade;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentExternalPurchaseOrderViewModel;
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
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades
{
    public class GarmentExternalPurchaseOrderFacade : IGarmentExternalPurchaseOrderFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentExternalPurchaseOrder> dbSet;
        public readonly IServiceProvider serviceProvider;
        private readonly ILogHistoryFacades logHistoryFacades;

        public GarmentExternalPurchaseOrderFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentExternalPurchaseOrder>();
            this.serviceProvider = serviceProvider;
            logHistoryFacades = serviceProvider.GetService<ILogHistoryFacades>();
        }

        public Tuple<List<GarmentExternalPurchaseOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentExternalPurchaseOrder> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "EPONo", "Items.PRNo", "SupplierName"
            };

            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

            Query = Query.Select(s => new GarmentExternalPurchaseOrder
            {
                Id = s.Id,
                UId = s.UId,
                IsPosted = s.IsPosted,
                SupplierName = s.SupplierName,
                SupplierCode = s.SupplierCode,
                OrderDate = s.OrderDate,
                EPONo = s.EPONo,
                SupplierImport = s.SupplierImport,
                IsOverBudget = s.IsOverBudget,
                IsApproved = s.IsApproved,
                Items = s.Items.Select(a => new GarmentExternalPurchaseOrderItem
                {
                    PRNo = a.PRNo,
                    PRId = a.PRId
                }).ToList(),
                CreatedBy = s.CreatedBy,
                LastModifiedUtc = s.LastModifiedUtc
            });


            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentExternalPurchaseOrder> pageable = new Pageable<GarmentExternalPurchaseOrder>(Query, Page - 1, Size);
            List<GarmentExternalPurchaseOrder> Data = pageable.Data.ToList<GarmentExternalPurchaseOrder>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentExternalPurchaseOrder ReadById(int id)
        {
            var a = this.dbSet.Where(p => p.Id == id)
                .Include(p => p.Items)
                .FirstOrDefault();
            return a;
        }

        public async Task<int> Create(GarmentExternalPurchaseOrder m, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in m.Items)
                    {
                        if (item.IsOverBudget)
                        {
                            m.IsOverBudget = true;
                            break;
                        }
                    }

                    m.EPONo = await GenerateNo(m, clientTimeZoneOffset);

                    EntityExtension.FlagForCreate(m, user, USER_AGENT);

                    foreach (var item in m.Items)
                    {

                        GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(item.POId));
                        internalPurchaseOrder.IsPosted = true;

                        GarmentInternalPurchaseOrderItem IPOItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                        if (item.ProductId.ToString() == IPOItem.ProductId)
                        {

                            IPOItem.Status = "Sudah dibuat PO Eksternal";
                        }

                        if ((m.PaymentMethod == "CMT" || m.PaymentMethod == "FREE FROM BUYER") && (m.PaymentType == "FREE" || m.PaymentType == "EX MASTER FREE"))
                        {
                            m.IsOverBudget = false;
                            item.IsOverBudget = false;
                            item.UsedBudget = 0;
                        }
                        else
                        {
                            var ipoItems = this.dbContext.GarmentInternalPurchaseOrderItems.Where(a => a.GPRItemId.Equals(IPOItem.GPRItemId) && a.ProductId.Equals(item.ProductId.ToString())).ToList();

                            foreach (var a in ipoItems)
                            {
                                a.RemainingBudget -= item.UsedBudget;
                            }
                        }

                        EntityExtension.FlagForCreate(item, user, USER_AGENT);
                    }

                    this.dbSet.Add(m);

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Create Purchase Order External - " + m.EPONo);

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

        public async Task<int> Update(int id, GarmentExternalPurchaseOrder m, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldM = this.dbSet.AsNoTracking()
                        .Include(d => d.Items)
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    m.IsOverBudget = false;
                    foreach (var item in m.Items)
                    {
                        if (item.IsOverBudget)
                        {
                            m.IsOverBudget = true;
                            break;
                        }
                    }

                    if (oldM != null && oldM.Id == id)
                    {
                        EntityExtension.FlagForUpdate(m, user, USER_AGENT);
                        foreach (var Olditem in oldM.Items)
                        {
                            GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(Olditem.POId));
                            //internalPurchaseOrder.IsPosted = true;

                            GarmentInternalPurchaseOrderItem IPOItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(Olditem.POId));

                            var ipoItems = this.dbContext.GarmentInternalPurchaseOrderItems.Where(a => a.GPRItemId.Equals(IPOItem.GPRItemId) && a.ProductId.Equals(Olditem.ProductId.ToString())).ToList();
                            //returning Values
                            foreach (var a in ipoItems)
                            {
                                a.RemainingBudget += Olditem.UsedBudget;
                            }
                        }

                        foreach (var item in m.Items)
                        {
                            var existingItem = oldM.Items.SingleOrDefault(d => d.Id == item.Id);
                            if (item.Id == 0)
                            {
                                GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(item.POId));
                                internalPurchaseOrder.IsPosted = true;

                                GarmentInternalPurchaseOrderItem IPOItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                                if (item.ProductId.ToString() == IPOItem.ProductId)
                                {

                                    IPOItem.Status = "Sudah dibuat PO Eksternal";
                                }

                                if ((m.PaymentMethod == "CMT" || m.PaymentMethod == "FREE FROM BUYER") && (m.PaymentType == "FREE" || m.PaymentType == "EX MASTER FREE"))
                                {
                                    m.IsOverBudget = false;
                                    item.IsOverBudget = false;
                                    item.UsedBudget = 0;
                                }
                                else
                                {
                                    var ipoItems = this.dbContext.GarmentInternalPurchaseOrderItems.Where(a => a.GPRItemId.Equals(IPOItem.GPRItemId) && a.ProductId.Equals(item.ProductId.ToString())).ToList();

                                    foreach (var a in ipoItems)
                                    {
                                        a.RemainingBudget -= item.UsedBudget;
                                    }
                                }

                                EntityExtension.FlagForCreate(item, user, USER_AGENT);
                                oldM.Items.Add(item);
                            }
                            else
                            {
                                GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(item.POId));
                                internalPurchaseOrder.IsPosted = true;

                                GarmentInternalPurchaseOrderItem IPOItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                                var ipoItems = this.dbContext.GarmentInternalPurchaseOrderItems.Where(a => a.GPRItemId.Equals(IPOItem.GPRItemId) && a.ProductId.Equals(item.ProductId.ToString())).ToList();
                                if ((m.PaymentMethod == "CMT" || m.PaymentMethod == "FREE FROM BUYER") && (m.PaymentType == "FREE" || m.PaymentType == "EX MASTER FREE"))
                                {
                                    m.IsOverBudget = false;
                                    item.IsOverBudget = false;
                                    item.UsedBudget = 0;
                                }
                                else
                                {
                                    foreach (var a in ipoItems)
                                    {
                                        a.RemainingBudget -= item.UsedBudget;
                                    }
                                }
                                EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                                dbContext.Entry(existingItem).CurrentValues.SetValues(item);
                            }
                        }
                        dbContext.Entry(oldM).CurrentValues.SetValues(m);
                        dbSet.Update(oldM);

                        foreach (var oldItem in oldM.Items)
                        {
                            var newItem = m.Items.FirstOrDefault(i => i.Id.Equals(oldItem.Id));
                            if (newItem == null)
                            {
                                GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id.Equals(oldItem.POId));
                                internalPurchaseOrder.IsPosted = false;

                                GarmentInternalPurchaseOrderItem IPOItems = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(oldItem.POId));

                                if (oldItem.ProductId.ToString() == IPOItems.ProductId)
                                {
                                    IPOItems.Status = "PO Internal belum diorder";
                                }

                                EntityExtension.FlagForDelete(oldItem, user, USER_AGENT);
                                dbContext.GarmentExternalPurchaseOrderItems.Update(oldItem);
                            }
                        }

                        //Create Log History
                        logHistoryFacades.Create("PEMBELIAN", "Update Purchase Order External - " + m.EPONo);

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

        async Task<string> GenerateNo(GarmentExternalPurchaseOrder model, int clientTimeZoneOffset)
        {
            DateTimeOffset Now = model.OrderDate;
            string Year = Now.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("yy"); ;
            string Month = Now.ToOffset(new TimeSpan(clientTimeZoneOffset, 0, 0)).ToString("MM"); ;

            string no = $"PO{Year}{Month}";
            int Padding = 5;

            var lastNo = await this.dbSet.Where(w => w.EPONo.StartsWith(no) && !w.IsDeleted && !w.EPONo.Contains("R")).OrderByDescending(o => o.EPONo).FirstOrDefaultAsync();
            no = $"{no}";

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.EPONo.Replace(no, "")) + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }

        public int Delete(int id, string user)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .SingleOrDefault(epo => epo.Id == id && !epo.IsDeleted);

                    EntityExtension.FlagForDelete(m, user, "Facade");

                    foreach (var item in m.Items)
                    {

                        GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id == item.POId);
                        internalPurchaseOrder.IsPosted = false;

                        GarmentInternalPurchaseOrderItem IPOItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                        var ipoItems = this.dbContext.GarmentInternalPurchaseOrderItems.Where(a => a.GPRItemId.Equals(IPOItem.GPRItemId) && a.ProductId.Equals(item.ProductId.ToString())).ToList();
                        //returning Values
                        foreach (var a in ipoItems)
                        {
                            a.RemainingBudget += item.UsedBudget;
                            a.Status = "PO Internal belum diorder";
                        }

                        EntityExtension.FlagForDelete(item, user, "Facade");
                    }

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Delete Purchase Order External - " + m.EPONo);

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

        public int EPOPost(List<GarmentExternalPurchaseOrder> ListEPO, string user)
        {
            int Updated = 0;
            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var Ids = ListEPO.Select(d => d.Id).ToList();
                    var listData = this.dbSet
                        .Where(m => Ids.Contains(m.Id) && !m.IsDeleted)
                        .Include(d => d.Items)
                        .ToList();
                    listData.ForEach(m =>
                    {
                        EntityExtension.FlagForUpdate(m, user, "Facade");
                        m.IsPosted = true;

                        //Create Log History
                        logHistoryFacades.Create("PEMBELIAN", "Post Purchase Order External - " + m.EPONo);

                        foreach (var item in m.Items)
                        {
                            GarmentInternalPurchaseOrderItem IPOItems = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                            if (item.ProductId.ToString() == IPOItems.ProductId)
                            {
                                //IPOItems.RemainingBudget += item.UsedBudget;
                                IPOItems.Status = "Sudah diorder ke Supplier";
                            }

                            GarmentPurchaseRequestItem PRItems = this.dbContext.GarmentPurchaseRequestItems.FirstOrDefault(a => a.Id.Equals(IPOItems.GPRItemId));
                            PRItems.Status = "Sudah diorder ke Supplier";

                            EntityExtension.FlagForUpdate(item, user, "Facade");
                        }
                        //foreach (var item in m.Items)
                        //{
                        //    EntityExtension.FlagForUpdate(item, user, "Facade");

                        //}
                    });



                    Updated = dbContext.SaveChanges();
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

        public int EPOUnpost(int id, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .SingleOrDefault(epo => epo.Id == id && !epo.IsDeleted);

                    EntityExtension.FlagForUpdate(m, user, "Facade");
                    m.IsPosted = false;
                    m.IsApproved = false;

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "UnPost Purchase Order External - " + m.EPONo);

                    foreach (var item in m.Items)
                    {
                        GarmentInternalPurchaseOrderItem IPOItems = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                        if (item.ProductId.ToString() == IPOItems.ProductId)
                        {
                            //IPOItems.RemainingBudget += item.UsedBudget;
                            IPOItems.Status = "Sudah dibuat PO Eksternal";
                        }

                        GarmentPurchaseRequestItem PRItems = this.dbContext.GarmentPurchaseRequestItems.FirstOrDefault(a => a.Id.Equals(IPOItems.GPRItemId));
                        PRItems.Status = "Sudah diterima Pembelian";

                        EntityExtension.FlagForUpdate(item, user, "Facade");

                    }

                    Updated = dbContext.SaveChanges();
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

        public int EPOCancel(int id, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .SingleOrDefault(epo => epo.Id == id && !epo.IsDeleted);

                    EntityExtension.FlagForUpdate(m, user, "Facade");
                    m.IsCanceled = true;

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Cancel Purchase Order External - " + m.EPONo);

                    foreach (var item in m.Items)
                    {
                        GarmentInternalPurchaseOrder internalPurchaseOrder = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(s => s.Id == item.POId);
                        internalPurchaseOrder.IsPosted = false;

                        GarmentInternalPurchaseOrderItem IPOItem = this.dbContext.GarmentInternalPurchaseOrderItems.FirstOrDefault(a => a.GPOId.Equals(item.POId));

                        if (item.ProductId.ToString() == IPOItem.ProductId)
                        {
                            IPOItem.Status = "Dibatalkan";
                        }

                        var ipoItems = this.dbContext.GarmentInternalPurchaseOrderItems.Where(a => a.GPRItemId.Equals(IPOItem.GPRItemId) && a.ProductId.Equals(item.ProductId.ToString())).ToList();
                        //returning Values
                        foreach (var a in ipoItems)
                        {
                            a.RemainingBudget += item.UsedBudget;
                        }

                        EntityExtension.FlagForUpdate(item, user, "Facade");

                    }

                    Updated = dbContext.SaveChanges();
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

        public int EPOClose(int id, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var m = this.dbSet
                        .Include(d => d.Items)
                        .SingleOrDefault(epo => epo.Id == id && !epo.IsDeleted);

                    EntityExtension.FlagForUpdate(m, user, "Facade");
                    m.IsClosed = true;

                    //Create Log History
                    logHistoryFacades.Create("PEMBELIAN", "Close Purchase Order External - " + m.EPONo);

                    foreach (var item in m.Items)
                    {
                        GarmentInternalPurchaseOrder IPO = this.dbContext.GarmentInternalPurchaseOrders.FirstOrDefault(a => a.Id.Equals(item.POId));

                        IPO.IsClosed = true;

                        EntityExtension.FlagForUpdate(item, user, "Facade");
                    }

                    Updated = dbContext.SaveChanges();
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



        public SupplierViewModel GetSupplier(long supplierId)
        {
            string supplierUri = "master/garment-suppliers";
            IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            if (httpClient != null)
            {
                var response = httpClient.GetAsync($"{APIEndpoint.Core}{supplierUri}/{supplierId}").Result.Content.ReadAsStringAsync();
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
                SupplierViewModel viewModel = JsonConvert.DeserializeObject<SupplierViewModel>(result.GetValueOrDefault("data").ToString());
                return viewModel;
            }
            else
            {
                SupplierViewModel viewModel = null;
                return viewModel;
            }

        }

        public GarmentProductViewModel GetProduct(long productId)
        {
            string productUri = "master/garmentProducts";
            IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
            if (httpClient != null)
            {
                var response = httpClient.GetAsync($"{APIEndpoint.Core}{productUri}/{productId}").Result.Content.ReadAsStringAsync();
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Result);
                GarmentProductViewModel viewModel = JsonConvert.DeserializeObject<GarmentProductViewModel>(result.GetValueOrDefault("data").ToString());
                return viewModel;
            }
            else
            {
                GarmentProductViewModel viewModel = null;
                return viewModel;
            }

        }

        public int EPOApprove(List<GarmentExternalPurchaseOrder> ListEPO, string user)
        {
            int Updated = 0;
            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var Ids = ListEPO.Select(d => d.Id).ToList();
                    var listData = this.dbSet
                        .Where(m => Ids.Contains(m.Id) && !m.IsDeleted)
                        .Include(d => d.Items)
                        .ToList();
                    listData.ForEach(m =>
                    {
                        EntityExtension.FlagForUpdate(m, user, "Facade");
                        m.IsApproved = true;

                        if (m.IsOverBudget)
                        {
                            //Create Log History
                            logHistoryFacades.Create("PEMBELIAN", "Approve Over Budget Purchase Order External - " + m.EPONo);
                        }
                        else
                        {
                            //Create Log History
                            logHistoryFacades.Create("PEMBELIAN", "Approve Purchase Order External - " + m.EPONo);
                        }


                        foreach (var item in m.Items)
                        {
                            EntityExtension.FlagForUpdate(item, user, "Facade");
                        }
                    });

                    Updated = dbContext.SaveChanges();
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
        public List<GarmentExternalPurchaseOrder> ReadBySupplier(string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentExternalPurchaseOrder> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "EPONo",
            };

            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, Keyword); // kalo search setelah Select dengan .Where setelahnya maka case sensitive, kalo tanpa .Where tidak masalah

            Query = Query
                .Where(m => m.IsPosted == true && m.IsClosed == false && m.IsDeleted == false && m.IsCanceled == false && ((m.IsOverBudget == true && m.IsApproved == true) || m.IsOverBudget == false))
                .Select(s => new GarmentExternalPurchaseOrder
                {
                    Id = s.Id,
                    UId = s.UId,
                    EPONo = s.EPONo,
                    OrderDate = s.OrderDate,
                    DeliveryDate = s.DeliveryDate,
                    SupplierName = s.SupplierName,
                    SupplierId = s.SupplierId,
                    SupplierCode = s.SupplierCode,
                    SupplierImport = s.SupplierImport,
                    CurrencyId = s.CurrencyId,
                    CurrencyCode = s.CurrencyCode,
                    PaymentMethod = s.PaymentMethod,
                    PaymentType = s.PaymentType,
                    PaymentDueDays = s.PaymentDueDays,
                    IncomeTaxId = s.IncomeTaxId,
                    IncomeTaxName = s.IncomeTaxName,
                    IncomeTaxRate = s.IncomeTaxRate,
                    VatId = s.VatId,
                    VatRate = s.VatRate,
                    IsUseVat = s.IsUseVat,
                    IsIncomeTax = s.IsIncomeTax,
                    IsClosed = s.IsClosed,
                    CreatedBy = s.CreatedBy,
                    LastModifiedUtc = s.LastModifiedUtc,
                    IsPayVAT = s.IsPayVAT,
                    IsPayIncomeTax = s.IsPayIncomeTax,
                    Items = s.Items.Select(i => new GarmentExternalPurchaseOrderItem
                    {
                        Id = i.Id,
                        POId = i.POId,
                        PONo = i.PONo,
                        RONo = i.RONo,
                        PRId = i.PRId,
                        PRNo = i.PRNo,
                        ProductCode = i.ProductCode,
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        PO_SerialNumber = i.PO_SerialNumber,
                        DefaultQuantity = i.DefaultQuantity,
                        DefaultUomId = i.DefaultUomId,
                        DefaultUomUnit = i.DefaultUomUnit,
                        DealQuantity = i.DealQuantity,
                        DealUomId = i.DealUomId,
                        DealUomUnit = i.DealUomUnit,
                        DOQuantity = i.DOQuantity,
                        PricePerDealUnit = i.PricePerDealUnit,
                        BudgetPrice = i.BudgetPrice,
                        Conversion = i.Conversion,
                        SmallQuantity = i.SmallQuantity,
                        SmallUomId = i.SmallUomId,
                        SmallUomUnit = i.SmallUomUnit,
                        Remark = i.Remark
                    })
                    .Where(i => (i.DealQuantity - i.DOQuantity) > 0)
                    .ToList()
                })
                .Where(m => m.Items.Count > 0);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

            return Query.ToList();
        }

        #region Duration ExternalPO-DO
        public IQueryable<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel> GetEPODODurationReportQuery(string unit, string supplier, string duration, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;
            int start = 0;
            int end = 0;
            if (duration == "0-30 hari")
            {
                start = 0;
                end = 30;
            }
            else if (duration == "31-60 hari")
            {
                start = 31;
                end = 60;
            }
            else
            {
                start = 61;
                end = 1000;
            }
            List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel> listEPODUration = new List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel>();
            var Query = (from a in dbContext.GarmentExternalPurchaseOrders
                         join b in dbContext.GarmentExternalPurchaseOrderItems on a.Id equals b.GarmentEPOId
                         join d in dbContext.GarmentPurchaseRequests on b.PRId equals d.Id
                         join e in dbContext.GarmentInternalPurchaseOrders on b.POId equals e.Id
                         join f in dbContext.GarmentInternalPurchaseOrderItems on e.Id equals f.GPOId
                         join g in dbContext.GarmentDeliveryOrderItems on a.Id equals g.EPOId
                         join h in dbContext.GarmentDeliveryOrders on g.GarmentDOId equals h.Id
                         //Conditions
                         where a.IsDeleted == false
                            && b.IsDeleted == false
                            && d.IsDeleted == false
                            && e.IsDeleted == false
                            && f.IsDeleted == false
                            && g.IsDeleted == false
                            && h.IsDeleted == false
                            && e.UnitId == (string.IsNullOrWhiteSpace(unit) ? e.UnitId : unit)
                            && a.SupplierId.ToString() == (string.IsNullOrWhiteSpace(supplier) ? a.SupplierId.ToString() : supplier)
                            && a.CreatedUtc.AddHours(offset).Date >= DateFrom.Date
                            && a.CreatedUtc.AddHours(offset).Date <= DateTo.Date
                         select new GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel
                         {
                             artikelNo = e.Article,
                             buyerName = e.BuyerName,
                             unit = d.UnitName,
                             category = a.Category,
                             productCode = b.ProductCode,
                             productName = b.ProductName,
                             productQuantity = b.DealQuantity,
                             productUom = b.DealUomUnit,
                             productPrice = b.PricePerDealUnit,
                             supplierCode = a.SupplierCode,
                             supplierName = a.SupplierName,
                             poIntCreatedDate = e.CreatedUtc,
                             expectedDate = a.DeliveryDate,
                             poEksCreatedDate = a.CreatedUtc,
                             deliveryOrderNo = h.DONo,
                             poEksNo = a.EPONo,
                             supplierDoDate = h.DODate,
                             planPO = b.PO_SerialNumber,
                             doCreatedDate = h.CreatedUtc,
                             poIntNo = e.PONo,
                             roNo = e.RONo,
                             staff = a.CreatedBy,
                         }).Distinct();
            foreach (var item in Query)
            {
                var ePODate = new DateTimeOffset(item.poEksCreatedDate.Date, TimeSpan.Zero);
                var doDate = new DateTimeOffset(item.supplierDoDate.Date, TimeSpan.Zero);

                var datediff = (((TimeSpan)(doDate - ePODate)).Days) + 1;
                GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel _new = new GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel
                {
                    artikelNo = item.artikelNo,
                    buyerName = item.buyerName,
                    unit = item.unit,
                    category = item.category,
                    productCode = item.productCode,
                    productName = item.productName,
                    productQuantity = item.productQuantity,
                    productUom = item.productUom,
                    productPrice = item.productPrice,
                    supplierCode = item.supplierCode,
                    supplierName = item.supplierName,
                    poIntCreatedDate = item.poIntCreatedDate,
                    expectedDate = item.expectedDate,
                    poEksCreatedDate = item.poEksCreatedDate,
                    deliveryOrderNo = item.deliveryOrderNo,
                    poEksNo = item.poEksNo,
                    supplierDoDate = item.supplierDoDate,
                    planPO = item.planPO,
                    doCreatedDate = item.doCreatedDate,
                    poIntNo = item.poIntNo,
                    roNo = item.roNo,
                    dateDiff = datediff,
                    staff = item.staff,
                };
                listEPODUration.Add(_new);
            }
            return listEPODUration.Where(s => s.dateDiff >= start && s.dateDiff <= end).AsQueryable();

        }


        public Tuple<List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel>, int> GetEPODODurationReport(string unit, string supplier, string duration, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
        {
            var Query = GetEPODODurationReportQuery(unit, supplier, duration, dateFrom, dateTo, offset);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderByDescending(b => b.poIntCreatedDate);
            }

            Pageable<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel> pageable = new Pageable<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel>(Query, page - 1, size);
            List<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel> Data = pageable.Data.ToList<GarmentExternalPurchaseOrderDeliveryOrderDurationReportViewModel>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }
        public MemoryStream GenerateExcelEPODODuration(string unit, string supplier, string duration, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetEPODODurationReportQuery(unit, supplier, duration, dateFrom, dateTo, offset);
            Query = Query.OrderByDescending(b => b.poIntCreatedDate);
            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No RO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Plan PO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Article / Style", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Buyer", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kategori", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Deskripsi Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PO Internal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl PO Internal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PO Eksternal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl PO Eksternal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Target Datang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Surat Jalan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Selisih Tanggal PO Eksternal - Surat Jalan (hari)", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Staff Pembelian", DataType = typeof(string) });

            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;
                    string doCreatedDate = item.doCreatedDate == null ? "-" : item.doCreatedDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string poIntCreatedDate = item.poIntCreatedDate == new DateTime(1970, 1, 1) ? "-" : item.poIntCreatedDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string expectedDate = item.expectedDate == new DateTime(1970, 1, 1) ? "-" : item.expectedDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string poEksCreatedDate = item.poEksCreatedDate == new DateTime(1970, 1, 1) ? "-" : item.poEksCreatedDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string supplierDoDate = item.supplierDoDate == new DateTime(1970, 1, 1) ? "-" : item.supplierDoDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(index, item.roNo, item.planPO, item.artikelNo, item.buyerName, item.unit, item.category, item.productCode, item.productName, item.productQuantity, item.productUom, item.productPrice, item.supplierCode, item.supplierName, item.poIntNo, poIntCreatedDate, item.poEksNo, poEksCreatedDate, expectedDate, item.deliveryOrderNo, supplierDoDate, item.dateDiff, item.staff);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }
        #endregion
        #region Monitoring Over Budget
        public IQueryable<GarmentExternalPurchaseOrderOverBudgetMonitoringViewModel> GetEPOOverBudgetReportQuery(string epono, string unit, string supplier, string status, DateTime? dateFrom, DateTime? dateTo, int offset)
        {


            DateTime d1 = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime d2 = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            bool _status;
            if (status == "BELUM")
            {
                _status = false;
            }
            else
            {
                _status = true;
            }
            List<GarmentExternalPurchaseOrderOverBudgetMonitoringViewModel> listEPO = new List<GarmentExternalPurchaseOrderOverBudgetMonitoringViewModel>();
            var Query = (from a in dbContext.GarmentExternalPurchaseOrders
                         join b in dbContext.GarmentExternalPurchaseOrderItems on a.Id equals b.GarmentEPOId
                         join d in dbContext.GarmentPurchaseRequests on b.PRId equals d.Id
                         join c in dbContext.GarmentPurchaseRequestItems on b.PO_SerialNumber equals c.PO_SerialNumber

                         //Conditions
                         where b.IsOverBudget == true && a.IsOverBudget == true && a.IsDeleted == false
                            && b.IsDeleted == false && c.IsDeleted == false && d.IsDeleted == false
                            && a.IsApproved == (string.IsNullOrWhiteSpace(status) ? a.IsApproved : _status)
                            && d.UnitId == (string.IsNullOrWhiteSpace(unit) ? d.UnitId : unit)
                            && a.EPONo == (string.IsNullOrWhiteSpace(epono) ? a.EPONo : epono)
                            && a.SupplierId.ToString() == (string.IsNullOrWhiteSpace(supplier) ? a.SupplierId.ToString() : supplier)
                            && ((d1 != new DateTime(1970, 1, 1)) ? (a.OrderDate.Date >= d1 && a.OrderDate.Date <= d2) : true)

                         select new GarmentExternalPurchaseOrderOverBudgetMonitoringViewModel
                         {

                             poExtNo = a.EPONo,
                             poExtDate = a.OrderDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
                             supplierCode = a.SupplierCode,
                             supplierName = a.SupplierName,
                             prNo = b.PRNo,
                             prRefNo = b.PO_SerialNumber,
                             prDate = d.Date.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture),
                             unit = d.UnitName,
                             productCode = b.ProductCode,
                             productName = b.ProductName,
                             productDesc = b.Remark,
                             budgetqty = c.Quantity,
                             quantity = b.DealQuantity,
                             diffqty = b.DealQuantity - c.Quantity,
                             uom = b.DealUomUnit,
                             budgetPrice = b.BudgetPrice,
                             price = b.PricePerDealUnit,
                             totalBudgetPrice = Convert.ToDouble(string.Format("{0:f4}", b.DealQuantity * b.BudgetPrice)),
                             totalPrice = Convert.ToDouble(string.Format("{0:f4}", b.DealQuantity * b.PricePerDealUnit)),
                             overBudgetValue = Convert.ToDouble(string.Format("{0:f4}", (b.DealQuantity * b.PricePerDealUnit) - (b.DealQuantity * b.BudgetPrice))),
                             overBudgetValuePercentage = Convert.ToDouble(string.Format("{0:f4}", ((b.DealQuantity * b.PricePerDealUnit) - (b.DealQuantity * b.BudgetPrice)) * 100 / ((b.DealQuantity * b.BudgetPrice)))),
                             status = a.IsApproved.Equals(true) ? "SUDAH" : "BELUM",
                             overBudgetRemark = b.OverBudgetRemark

                         }).Distinct().ToList().OrderByDescending(s => s.poExtDate);
            int i = 1;
            foreach (var item in Query)
            {
                listEPO.Add(
                    new GarmentExternalPurchaseOrderOverBudgetMonitoringViewModel
                    {
                        no = i,
                        poExtNo = item.poExtNo,
                        poExtDate = item.poExtDate,
                        supplierCode = item.supplierCode,
                        supplierName = item.supplierName,
                        prNo = item.prNo,
                        prRefNo = item.prRefNo,
                        prDate = item.prDate,
                        unit = item.unit,
                        productCode = item.productCode,
                        productName = item.productName,
                        productDesc = item.productDesc,
                        budgetqty = item.budgetqty,
                        quantity = item.quantity,
                        diffqty = item.diffqty,
                        uom = item.uom,
                        budgetPrice = item.budgetPrice,
                        price = item.price,
                        totalBudgetPrice = item.totalBudgetPrice,
                        totalPrice = item.totalPrice,
                        overBudgetValue = item.overBudgetValue,
                        overBudgetValuePercentage = item.overBudgetValuePercentage,
                        status = item.status,
                        overBudgetRemark = item.overBudgetRemark
                    }

                    );
                i++;
            }
            return listEPO.AsQueryable();

        }


        public Tuple<List<GarmentExternalPurchaseOrderOverBudgetMonitoringViewModel>, int> GetEPOOverBudgetReport(string epono, string unit, string supplier, string status, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
        {
            var Query = GetEPOOverBudgetReportQuery(epono, unit, supplier, status, dateFrom, dateTo, offset);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            //if (OrderDictionary.Count.Equals(0))
            //{
            //	Query = Query.OrderByDescending(b => b.poExtDate);
            //}

            Pageable<GarmentExternalPurchaseOrderOverBudgetMonitoringViewModel> pageable = new Pageable<GarmentExternalPurchaseOrderOverBudgetMonitoringViewModel>(Query, page - 1, size);
            List<GarmentExternalPurchaseOrderOverBudgetMonitoringViewModel> Data = pageable.Data.ToList<GarmentExternalPurchaseOrderOverBudgetMonitoringViewModel>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }

        public MemoryStream GenerateExcelEPOOverBudget(string epono, string unit, string supplier, string status, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset)
        {
            var Query = GetEPOOverBudgetReportQuery(epono, unit, supplier, status, dateFrom, dateTo, offset);
            Query = Query.OrderByDescending(b => b.poExtDate);
            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No. PO Eksternal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal PO Eksternal", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No. Purchase Request", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Purchase Request", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No. Ref. Purchase Request", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Unit", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Budget", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Selisih", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga Budget", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga  Beli", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Total Harga Budget", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Total Harga Beli", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nilai Over Budget", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nilai Over Budget (%)", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Status Approve", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan Over Budget", DataType = typeof(String) });

            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;

                    result.Rows.Add(item.no, item.poExtNo, item.poExtDate, item.supplierCode, item.supplierName, item.prNo, item.prDate, item.prRefNo, item.unit, item.productName, item.productCode, item.productDesc, item.budgetqty, item.quantity, item.diffqty, item.uom, item.price, item.budgetPrice, item.totalBudgetPrice, item.totalPrice, item.overBudgetValue, item.overBudgetValuePercentage, item.status, item.overBudgetRemark);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }


        #endregion

        public List<GarmentExternalPurchaseOrderItem> ReadItemByRO(string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentExternalPurchaseOrder> Query = this.dbSet.Where(m => m.IsPosted == true && m.IsClosed == false && m.IsDeleted == false && m.IsCanceled == false);

            List<string> searchAttributes = new List<string>()
            {
                "RONo","PO_SerialNumber"
            };

            IQueryable<GarmentExternalPurchaseOrderItem> QueryItem = dbContext.GarmentExternalPurchaseOrderItems;

            QueryItem = QueryHelper<GarmentExternalPurchaseOrderItem>.ConfigureSearch(QueryItem, searchAttributes, Keyword);

            QueryItem = (from i in QueryItem
                         join b in Query on i.GarmentEPOId equals b.Id
                         // where i.ProductName.ToUpper() == "PROCESS"
                         select new GarmentExternalPurchaseOrderItem
                         {
                             Id = i.Id,
                             GarmentEPOId = i.GarmentEPOId,
                             RONo = i.RONo,
                             ProductCode = i.ProductCode,
                             ProductId = i.ProductId,
                             ProductName = i.ProductName,
                             PO_SerialNumber = i.PO_SerialNumber,
                             DealQuantity = i.DealQuantity,
                             DealUomId = i.DealUomId,
                             DealUomUnit = i.DealUomUnit,
                             Article = i.Article,
                             CreatedUtc = i.CreatedUtc
                         });



            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            QueryItem = QueryHelper<GarmentExternalPurchaseOrderItem>.ConfigureFilter(QueryItem, FilterDictionary);

            return QueryItem.ToList();
        }

        public List<GarmentExternalPurchaseOrder> ReadItemByEPONo(string EPONo = null, string Filter = "{}")
        {
            IQueryable<GarmentExternalPurchaseOrder> Query = this.dbSet.Include(s => s.Items).Where(m => m.IsClosed == false && m.IsDeleted == false && m.IsCanceled == false);

            List<string> searchAttributes = new List<string>()
            {
                "EPONo"
            };

            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, EPONo);


            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

            return Query.ToList();
        }
        public Tuple<List<GarmentExternalPurchaseOrder>, int, Dictionary<string, string>> ReadItemByEPONoSimply(string EPONo = null, string Filter = "{}", int supplierId = 0, int currencyId = 0, int Page = 1, int Size = 10)
        {
            //IQueryable<GarmentExternalPurchaseOrder> Query = this.dbSet.Include(s => s.Items).Where(m =>m.IsPosted && m.IsClosed == false && m.IsDeleted == false && m.IsCanceled == false && m.IsDispositionPaidCreatedAll == false && m.Items.Any(t=> t.IsDispositionCreatedAll == false));

            //List<string> searchAttributes = new List<string>()
            //{
            //    "EPONo"
            //};

            //Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, EPONo);

            //if (supplierId != 0)
            //    Query = Query.Where(s => s.SupplierId == supplierId);

            //if (currencyId != 0)
            //    Query = Query.Where(s => s.CurrencyId == currencyId);

            //Query = Query.Select(s => new GarmentExternalPurchaseOrder
            //{
            //    Id = s.Id,
            //    EPONo = s.EPONo
            //});

            //Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            //Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

            //return Query.ToList();
            IQueryable<GarmentExternalPurchaseOrder> Query = this.dbSet.Include(s => s.Items).Where(m => m.IsPosted && m.IsClosed == false && m.IsDeleted == false && m.IsCanceled == false && m.IsDispositionPaidCreatedAll == false && m.Items.Any(t => t.IsDispositionCreatedAll == false)); ;

            List<string> searchAttributes = new List<string>()
            {
                "EPONo"
            };

            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, EPONo);

            Query = Query
            .Where(entity => (entity.IsOverBudget == true && entity.IsApproved == true) || (entity.IsOverBudget == false))
            .Select(s => new GarmentExternalPurchaseOrder
            {
                Id = s.Id,
                UId = s.UId,
                IsPosted = s.IsPosted,
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                SupplierCode = s.SupplierCode,
                Category = s.Category,
                PaymentType = s.PaymentType,
                CurrencyId = s.CurrencyId,
                OrderDate = s.OrderDate,
                EPONo = s.EPONo,
                SupplierImport = s.SupplierImport,
                IsOverBudget = s.IsOverBudget,
                IsApproved = s.IsApproved,
                Items = s.Items.Select(a => new GarmentExternalPurchaseOrderItem
                {
                    PRNo = a.PRNo,
                    PRId = a.PRId
                }).ToList(),
                CreatedBy = s.CreatedBy,
                LastModifiedUtc = s.LastModifiedUtc
            });


            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

            //Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            //Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureOrder(Query, OrderDictionary);

            Dictionary<string, string> OrderDictionary = new Dictionary<string, string>();
            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentExternalPurchaseOrder> pageable = new Pageable<GarmentExternalPurchaseOrder>(Query, Page - 1, Size);
            List<GarmentExternalPurchaseOrder> Data = pageable.Data.ToList<GarmentExternalPurchaseOrder>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public Tuple<List<GarmentExternalPurchaseOrder>, int, Dictionary<string, string>> ReadItemByEPONoSimply(string EPONo = null, int supplierId = 0, string currencyCode = null, string paymentType = null, string category = null, int Page = 1, int Size = 10)
        {
            IQueryable<GarmentExternalPurchaseOrder> Query = this.dbSet.Include(s => s.Items).Where(m => m.IsPosted && m.IsClosed == false && m.IsDeleted == false && m.IsCanceled == false && m.IsDispositionPaidCreatedAll == false && m.Items.Any(t => t.IsDispositionCreatedAll == false && t.IsDeleted == false));

            List<string> searchAttributes = new List<string>()
            {
                "EPONo"
            };

            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, EPONo);

            if (!string.IsNullOrEmpty(currencyCode))
            {
                Query = Query.Where(s => s.CurrencyCode == currencyCode);
            }

            if (supplierId > 0)
                Query = Query.Where(entity => entity.SupplierId == supplierId);

            if (!string.IsNullOrWhiteSpace(paymentType))
                Query = Query.Where(entity => entity.PaymentType == paymentType);

            if (!string.IsNullOrWhiteSpace(category))
                Query = Query.Where(entity => entity.Category == category);

            Query = Query.Select(s => new GarmentExternalPurchaseOrder
            {
                Id = s.Id,
                UId = s.UId,
                IsPosted = s.IsPosted,
                SupplierId = s.SupplierId,
                SupplierName = s.SupplierName,
                SupplierCode = s.SupplierCode,
                Category = s.Category,
                PaymentType = s.PaymentType,
                CurrencyId = s.CurrencyId,
                OrderDate = s.OrderDate,
                EPONo = s.EPONo,
                SupplierImport = s.SupplierImport,
                IsOverBudget = s.IsOverBudget,
                IsApproved = s.IsApproved,
                Items = s.Items.Select(a => new GarmentExternalPurchaseOrderItem
                {
                    PRNo = a.PRNo,
                    PRId = a.PRId
                }).ToList(),
                CreatedBy = s.CreatedBy,
                LastModifiedUtc = s.LastModifiedUtc
            });


            //Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            //Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

            //Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            //Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureOrder(Query, OrderDictionary);

            Dictionary<string, string> OrderDictionary = new Dictionary<string, string>();
            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentExternalPurchaseOrder> pageable = new Pageable<GarmentExternalPurchaseOrder>(Query, Page - 1, Size);
            List<GarmentExternalPurchaseOrder> Data = pageable.Data.ToList<GarmentExternalPurchaseOrder>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public List<GarmentExternalPurchaseOrderItem> ReadItemByPOSerialNumberLoader(string Keyword = null, string Filter = "{}", int size = 50)
        {
            // IQueryable<GarmentExternalPurchaseOrder> Query = this.dbSet.IgnoreQueryFilters().Where(m => m.IsPosted == true && m.IsClosed == false && m.IsCanceled == false);
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            bool hasACCFilter = FilterDictionary.ContainsKey("PO_SerialNumber");
            string POSerialNumber = hasACCFilter ? (FilterDictionary["PO_SerialNumber"] ?? "").Trim() : "";
            IQueryable<GarmentExternalPurchaseOrderItem> QueryItem = dbContext.GarmentExternalPurchaseOrderItems.IgnoreQueryFilters().Where(i => (i.IsDeleted == true && i.DeletedAgent == "LUCIA") || (i.IsDeleted == false));
            List<string> searchAttributes = new List<string>()
            {
                "PO_SerialNumber"
            };

            QueryItem = QueryHelper<GarmentExternalPurchaseOrderItem>.ConfigureSearch(QueryItem, searchAttributes, Keyword);
            if (hasACCFilter)
            {
                QueryItem = QueryItem.Where(a => a.PO_SerialNumber.Contains(POSerialNumber));
            }

            QueryItem = (from i in QueryItem
                         where i.PO_SerialNumber.Contains(Keyword)
                         select new GarmentExternalPurchaseOrderItem
                         {
                             Id = i.Id,
                             PO_SerialNumber = i.PO_SerialNumber,
                             UENItemId = i.UENItemId,
                             ProductCode = i.ProductCode,
                             ProductId = i.ProductId,
                             ProductName = i.ProductName,
                             Remark = i.Remark,
                             Article = i.Article,
                             RONo = i.RONo
                         });

            List<GarmentExternalPurchaseOrderItem> ListData = new List<GarmentExternalPurchaseOrderItem>(QueryItem.OrderBy(o => o.PO_SerialNumber).Take(size));
            return ListData;
        }

        public List<GarmentExternalPurchaseOrderItem> ReadItemByROLoader(string Keyword = null, string Filter = "{}", int size = 50)
        {
            IQueryable<GarmentExternalPurchaseOrderItem> QueryItem = dbContext.GarmentExternalPurchaseOrderItems.IgnoreQueryFilters().Where(i => (i.IsDeleted == true && i.DeletedAgent == "LUCIA") || (i.IsDeleted == false));
            List<string> searchAttributes = new List<string>()
            {
                "RONo"
            };

            QueryItem = QueryHelper<GarmentExternalPurchaseOrderItem>.ConfigureSearch(QueryItem, searchAttributes, Keyword);

            QueryItem = (from i in QueryItem
                         where i.RONo.Contains(Keyword)
                         select new GarmentExternalPurchaseOrderItem
                         {
                             Id = i.Id,
                             PO_SerialNumber = i.PO_SerialNumber,
                             UENItemId = i.UENItemId,
                             RONo = i.RONo
                         });

            List<GarmentExternalPurchaseOrderItem> ListData = new List<GarmentExternalPurchaseOrderItem>(QueryItem.OrderBy(o => o.RONo).Take(size));
            return ListData;
        }
        public List<GarmentExternalPurchaseOrderItem> ReadItemForUnitDOByRO(string Keyword = null, string Filter = "{}")
        {
            //var Query = this.dbSet.Where(entity => entity.IsPosted && !entity.IsClosed && !entity.IsCanceled).Select(entity => new { entity.Id});

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            string RONo = (FilterDictionary["RONo"] ?? "").Trim();
            //IQueryable<GarmentExternalPurchaseOrderItem> QueryItem = dbContext.GarmentExternalPurchaseOrderItems.Where(entity=>entity.RONo==RONo ); //CreatedUtc > DateTime(2018, 12, 31)

            var QueryItem = (from i in dbContext.GarmentExternalPurchaseOrderItems.IgnoreQueryFilters().Where(i => (i.IsDeleted == true && i.DeletedAgent == "LUCIA") || (i.IsDeleted == false))
                             join b in this.dbSet.IgnoreQueryFilters().Where(i => (i.IsDeleted == true && i.DeletedAgent == "LUCIA") || (i.IsDeleted == false)) on i.GarmentEPOId equals b.Id
                             where i.RONo == RONo
                             && b.IsPosted && !b.IsClosed && !b.IsCanceled
                             select new GarmentExternalPurchaseOrderItem
                             {
                                 Id = i.Id,
                                 GarmentEPOId = i.GarmentEPOId,
                                 RONo = i.RONo,
                                 Article = i.Article
                             });

            return QueryItem.ToList();
        }

        public bool GetIsUnpost(int Id)
        {
            bool response = true;

            var searchDisposition = this.dbContext.GarmentDispositionPurchases
                        .Include(s => s.GarmentDispositionPurchaseItems)
                        .ThenInclude(s => s.GarmentDispositionPurchaseDetails)
                        .Where(s => s.GarmentDispositionPurchaseItems.Any(t => t.EPOId == Id)
                        ).ToList();

            if (searchDisposition.Count == 0)
            {
                response = false;
            }

            return response;
        }

        public List<object> DynamicSubcon(string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentExternalPurchaseOrder> Query = this.dbSet.Include(x => x.Items);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureFilter(Query, FilterDictionary);

            List<string> searchAttributes = new List<string>()
            {
                "EPONo",
            };

            Query = QueryHelper<GarmentExternalPurchaseOrder>.ConfigureSearch(Query, searchAttributes, Keyword); // kalo search setelah Select dengan .Where setelahnya maka case sensitive, kalo tanpa .Where tidak masalah

            List<object> ListData = new List<object>();

            var res = from m in Query
                      join b in dbContext.GarmentExternalPurchaseOrderItems on m.Id equals b.GarmentEPOId
                      where m.IsPosted == true && m.IsClosed == false && m.IsDeleted == false && m.IsCanceled == false && ((m.IsOverBudget == true && m.IsApproved == true) || m.IsOverBudget == false)
                      && b.IsDeleted == false && (b.DealQuantity - b.DOQuantity > 0)
                      select new
                      {
                          EPOId = m.Id,
                          m.EPONo,
                          m.CurrencyRate,
                          EPOItemId = b.Id,
                          Product = new GarmentProductViewModel { Id = b.ProductId, Code = b.ProductCode, Name = b.ProductName },
                          Uom = new UomViewModel { Id = b.DealUomId.ToString(), Unit = b.DealUomUnit },
                          BudgetQuantity = b.DealQuantity - b.DOQuantity,
                          b.PO_SerialNumber,
                          b.PricePerDealUnit,
                          RONoMaster = b.RONo,
                          b.Article
                      };
            //var res = Query
            //    .Where(m => m.IsPosted == true && m.IsClosed == false && m.IsDeleted == false && m.IsCanceled == false && ((m.IsOverBudget == true && m.IsApproved == true) || m.IsOverBudget == false))
            //    .Select(s => new 
            //    {
            //        Id = s.Id,
            //        UId = s.UId,
            //        EPONo = s.EPONo,
            //        OrderDate = s.OrderDate,
            //        DeliveryDate = s.DeliveryDate,
            //        SupplierName = s.SupplierName,
            //        SupplierId = s.SupplierId,
            //        SupplierCode = s.SupplierCode,
            //        SupplierImport = s.SupplierImport,
            //        CurrencyId = s.CurrencyId,
            //        CurrencyCode = s.CurrencyCode,
            //        PaymentMethod = s.PaymentMethod,
            //        PaymentType = s.PaymentType,
            //        PaymentDueDays = s.PaymentDueDays,
            //        IncomeTaxId = s.IncomeTaxId,
            //        IncomeTaxName = s.IncomeTaxName,
            //        IncomeTaxRate = s.IncomeTaxRate,
            //        VatId = s.VatId,
            //        VatRate = s.VatRate,
            //        IsUseVat = s.IsUseVat,
            //        IsIncomeTax = s.IsIncomeTax,
            //        IsClosed = s.IsClosed,
            //        CreatedBy = s.CreatedBy,
            //        LastModifiedUtc = s.LastModifiedUtc,
            //        IsPayVAT = s.IsPayVAT,
            //        IsPayIncomeTax = s.IsPayIncomeTax,
            //        Items = s.Items.Select(i => new GarmentExternalPurchaseOrderItem
            //        {
            //            Id = i.Id,
            //            POId = i.POId,
            //            PONo = i.PONo,
            //            RONo = i.RONo,
            //            PRId = i.PRId,
            //            PRNo = i.PRNo,
            //            ProductCode = i.ProductCode,
            //            ProductId = i.ProductId,
            //            ProductName = i.ProductName,
            //            PO_SerialNumber = i.PO_SerialNumber,
            //            DefaultQuantity = i.DefaultQuantity,
            //            DefaultUomId = i.DefaultUomId,
            //            DefaultUomUnit = i.DefaultUomUnit,
            //            DealQuantity = i.DealQuantity,
            //            DealUomId = i.DealUomId,
            //            DealUomUnit = i.DealUomUnit,
            //            DOQuantity = i.DOQuantity,
            //            PricePerDealUnit = i.PricePerDealUnit,
            //            BudgetPrice = i.BudgetPrice,
            //            Conversion = i.Conversion,
            //            SmallQuantity = i.SmallQuantity,
            //            SmallUomId = i.SmallUomId,
            //            SmallUomUnit = i.SmallUomUnit,
            //            Remark = i.Remark
            //        })
            //        .Where(i => (i.DealQuantity - i.DOQuantity) > 0)
            //        .ToList()
            //    })
            //    .Where(m => m.Items.Count > 0);

            foreach (var a in res)
            {
                ListData.Add(a);
            }

            return ListData;
        }

        public IQueryable<GarmentMonitoringPriceGarmentProductViewModel> GetReportPriceGmtProduct(string category, string productCode, string productName, string supplierCode, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

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

                          where (string.IsNullOrWhiteSpace(category) ? true : (category == "BAHAN PENDUKUNG" ? (b.ProductName != "FABRIC" && b.ProductName != "INTERLINING") : b.ProductName == category))
                                && b.ProductCode == (string.IsNullOrWhiteSpace(productCode) ? b.ProductCode : productCode)
                                && b.ProductName == (string.IsNullOrWhiteSpace(productName) ? b.ProductName : productName)
                                && a.SupplierCode == (string.IsNullOrWhiteSpace(supplierCode) ? a.SupplierCode : supplierCode)
                                && a.OrderDate.AddHours(offset).Date >= DateFrom.Date
                                && a.OrderDate.AddHours(offset).Date <= DateTo.Date

                          select new GarmentMonitoringPriceGarmentProductViewModel
                          {
                              epoNo = a.EPONo,
                              orderDate = a.OrderDate,
                              supplierCode = a.SupplierCode,
                              supplierName = a.SupplierName,
                              prNo = b.PRNo,
                              poNo = b.PO_SerialNumber,
                              invoiceNo = Inv == null ? "-" : Inv.InvoiceNo,
                              invoiceDate = Inv == null ? new DateTime(1970, 1, 1) : Inv.InvoiceDate,
                              inNo = IN == null ? "-" : IN.INNo,
                              inDate = IN == null ? new DateTime(1970, 1, 1) : IN.INDate,
                              productCode = b.ProductCode,
                              productName = b.ProductName,
                              productRemark = b.Remark,
                              epoQuantity = b.DealQuantity,
                              uomunit     = b.DealUomUnit,
                              currencyCode = e.DOCurrencyCode,
                              epoprice     = b.PricePerDealUnit,
                              rate         = e.DOCurrencyRate,
                              amount       = b.DealQuantity * b.PricePerDealUnit * e.DOCurrencyRate

                          }).OrderBy(b => b.supplierCode).ThenBy(b => b.productCode).ThenBy(b => b.orderDate);

            return Query1;
        }

        public List<GarmentMonitoringPriceGarmentProductViewModel> GetReportPriceProduct(string category, string productCode, string productName, string supplierCode, DateTime? dateFrom, DateTime? dateTo, int offset, string Order = "{}")
        {

            var Query = GetReportPriceGmtProduct(category, productCode, productName, supplierCode, dateFrom, dateTo, offset);

            Query = Query.OrderBy(b => b.supplierCode).ThenBy(b => b.productCode).ThenBy(b => b.orderDate);

            return Query.ToList();
        }

        public MemoryStream GenerateExcelPriceProduct(string category, string productCode, string productName, string supplierCode, DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetReportPriceGmtProduct(category, productCode, productName, supplierCode, dateFrom, dateTo, offset);

            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "No", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PO External", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl PO External", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No PR", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Ref PO", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Supplier", DataType = typeof(String) });            
        
            result.Columns.Add(new DataColumn() { ColumnName = "No Invoice", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Invoice", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No Nota Intern", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Nota Intern", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Barang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(String) });

            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah PO", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga Satuan", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Rate", DataType = typeof(Double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Total", DataType = typeof(Double) });
         

            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", 0, "", "", 0, 0, 0);
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;

                    string EPODate = item.orderDate == new DateTime(1970, 1, 1) ? "-" : item.orderDate.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd-MM-yyyy", new CultureInfo("id-ID"));
                    string InvDate = item.invoiceDate == new DateTime(1970, 1, 1) ? "-" : item.invoiceDate.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd-MM-yyyy", new CultureInfo("id-ID"));
                    string NIDate = item.inDate == new DateTime(1970, 1, 1) ? "-" : item.inDate.ToOffset(new TimeSpan(7, 0, 0)).ToString("dd-MM-yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(index, item.epoNo, EPODate, item.prNo, item.poNo, item.supplierCode, item.supplierName, item.invoiceNo, InvDate, item.inNo, NIDate, item.productCode, item.productName, item.productRemark, item.epoQuantity, item.uomunit, item.currencyCode, item.epoprice, item.rate, item.amount);

                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }

    }
}
