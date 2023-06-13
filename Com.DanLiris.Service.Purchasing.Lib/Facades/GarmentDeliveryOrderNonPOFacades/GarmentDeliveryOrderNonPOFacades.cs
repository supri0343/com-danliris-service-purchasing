using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderNonPOModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderNonPOViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderNonPOFacades
{
    public class GarmentDeliveryOrderNonPOFacades : IGarmentDeliveryOrderNonPOFacade
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentDeliveryOrderNonPO> dbSet;
        private readonly DbSet<GarmentDeliveryOrderNonPOItem> dbSetItem;

        private readonly IMapper mapper;
        public GarmentDeliveryOrderNonPOFacades(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentDeliveryOrderNonPO>();
            dbSetItem = dbContext.Set<GarmentDeliveryOrderNonPOItem>();
            this.serviceProvider = serviceProvider;

            mapper = serviceProvider == null ? null : (IMapper)serviceProvider.GetService(typeof(IMapper));
        }

        public Tuple<List<GarmentDeliveryOrderNonPO>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            //IQueryable<GarmentDeliveryOrderNonPO> Query = this.dbSet.Include(m => m.Items);
            IQueryable<GarmentDeliveryOrderNonPO> Query = this.dbSet.AsNoTracking().Include(x => x.Items)
                .Select(x => new GarmentDeliveryOrderNonPO
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
                    LastModifiedUtc = x.LastModifiedUtc,
                    Items = x.Items.Select(y => new GarmentDeliveryOrderNonPOItem
                    {
                        Id = y.Id,
                        CurrencyId = y.CurrencyId,
                        CurrencyCode = y.CurrencyCode,
                        PricePerDealUnit = y.PricePerDealUnit,
                        Quantity = y.Quantity,
                        UomId =y.UomId,
                        UomUnit=y.UomUnit,
                        ProductRemark = y.ProductRemark
                     
                    }),

                });

            List<string> searchAttributes = new List<string>()
            {
                "DONo", "BillNo", "PaymentBill","SupplierName"//, "Items.EPONo"
            };

            Query = QueryHelper<GarmentDeliveryOrderNonPO>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentDeliveryOrderNonPO>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentDeliveryOrderNonPO>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentDeliveryOrderNonPO> pageable = new Pageable<GarmentDeliveryOrderNonPO>(Query, Page - 1, Size);
            List<GarmentDeliveryOrderNonPO> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }


        public GarmentDeliveryOrderNonPO ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                .FirstOrDefault();
            return model;
        }

        public async Task<int> Create(GarmentDeliveryOrderNonPO m, string user, int clientTimeZoneOffset = 7)
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

                        CurrencyViewModel garmentCurrencyViewModel = GetCurrency(item.CurrencyCode, m.DODate);
                        m.DOCurrencyId = garmentCurrencyViewModel.Id;
                        m.DOCurrencyCode = garmentCurrencyViewModel.Code;
                        m.DOCurrencyRate = garmentCurrencyViewModel.Rate;

                        EntityExtension.FlagForCreate(item, user, USER_AGENT);
                    }

                    this.dbSet.Add(m);

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

        public async Task<int> Delete(int id, string user)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var model = this.dbSet
                                .Include(m => m.Items)
                                .SingleOrDefault(m => m.Id == id && !m.IsDeleted);

                    EntityExtension.FlagForDelete(model, user, USER_AGENT);
                    foreach (var item in model.Items)
                    {
                        EntityExtension.FlagForDelete(item, user, USER_AGENT);

                       
                    }
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

        public async Task<int> Update(int id, GarmentDeliveryOrderNonPO m, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldM = ReadById(id);

                    if (oldM != null && oldM.Id == id)
                    {
                        EntityExtension.FlagForUpdate(m, user, USER_AGENT);
                        foreach (var oldItem in oldM.Items)
                        {
                            var deletedItem = m.Items.Where(e => e.Id == oldItem.Id).FirstOrDefault();
                            if (deletedItem == null)
                            {
                                EntityExtension.FlagForDelete(oldItem, user, USER_AGENT);
                            }
                        }
         
                        foreach (var newItem in m.Items)
                        {
                            var existItem = oldM.Items.Where(e => e.Id == newItem.Id).FirstOrDefault();
                            if (existItem == null)
                            {
                                GarmentDeliveryOrderNonPOItem item = new GarmentDeliveryOrderNonPOItem
                                {
                                    CurrencyId = newItem.CurrencyId,
                                    CurrencyCode = newItem.CurrencyCode,
                                    GarmentDeliveryOrderNonPOId = oldM.Id,
                                    PricePerDealUnit = newItem.PricePerDealUnit,
                                    Quantity = newItem.Quantity,
                                    UomId = newItem.UomId,
                                    UomUnit = newItem.UomUnit,
                                    ProductRemark = newItem.ProductRemark,
                                };
                                EntityExtension.FlagForCreate(item, user, USER_AGENT);
                                dbSetItem.Add(item);
                            }
                            else
                            {
                                existItem.CurrencyId = newItem.CurrencyId;
                                existItem.CurrencyCode = newItem.CurrencyCode;
                                existItem.PricePerDealUnit = newItem.PricePerDealUnit;
                                existItem.Quantity = newItem.Quantity;
                                existItem.UomId = newItem.UomId;
                                existItem.UomUnit = newItem.UomUnit;
                                existItem.ProductRemark = newItem.ProductRemark;
                                EntityExtension.FlagForUpdate(existItem, user, USER_AGENT);

                            }
                        }

                        dbSet.Update(oldM);

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
    }
}
