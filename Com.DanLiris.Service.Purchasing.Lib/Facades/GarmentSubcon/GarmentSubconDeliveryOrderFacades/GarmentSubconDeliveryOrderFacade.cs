using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubconDeliveryOrderModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSubconDeliveryOrderFacades
{
    public class GarmentSubconDeliveryOrderFacade : IGarmentSubconDeliveryOrderFacades
    {
        private string USER_AGENT = "Facade";

        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentSubconDeliveryOrder> dbSet;
        private readonly DbSet<GarmentSubconDeliveryOrderItem> dbSetItem;
        private readonly IMapper mapper;
        public GarmentSubconDeliveryOrderFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentSubconDeliveryOrder>();
            dbSetItem = dbContext.Set<GarmentSubconDeliveryOrderItem>();
            this.serviceProvider = serviceProvider;

            mapper = serviceProvider == null ? null : (IMapper)serviceProvider.GetService(typeof(IMapper));
        }

        public Tuple<List<GarmentSubconDeliveryOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentSubconDeliveryOrder> Query = this.dbSet.AsNoTracking().Include(x => x.Items)
                .Select(x => new GarmentSubconDeliveryOrder
                {
                    Id = x.Id,
                    DONo = x.DONo,
                    DODate = x.DODate,
                    ArrivalDate = x.ArrivalDate,
                    SupplierId = x.SupplierId,
                    SupplierCode = x.SupplierCode,
                    SupplierName = x.SupplierName,
                    CreatedBy = x.CreatedBy,
                    LastModifiedUtc = x.LastModifiedUtc,
                    RONo = x.RONo,
                    Items = x.Items.Select(y => new GarmentSubconDeliveryOrderItem
                    {
                        Id = y.Id,
                        POSerialNumber = y.POSerialNumber,
                        CurrencyCode = y.CurrencyCode,
                        UomId = y.UomId,
                        UomUnit = y.UomUnit,
                    }),
                });

            List<string> searchAttributes = new List<string>()
            {
                "DONo", "RONo","SupplierName"
            };

            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentSubconDeliveryOrder> pageable = new Pageable<GarmentSubconDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentSubconDeliveryOrder> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public GarmentSubconDeliveryOrder ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                .FirstOrDefault();
            return model;
        }

        public async Task<int> Create(GarmentSubconDeliveryOrder m, string user, int clientTimeZoneOffset = 7)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(m, user, USER_AGENT);

                    foreach (var item in m.Items)
                    {
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

        public async Task<int> Update(int id, GarmentSubconDeliveryOrder newModel, string user, int clientTimeZoneOffset = 7)
        {
            int Updated = 0;
            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var oldModel = this.dbSet
                              .Include(m => m.Items)
                              .SingleOrDefault(m => m.Id == id && !m.IsDeleted);


                    //Delete Item or Update Item Old Data
                    foreach(var item in oldModel.Items)
                    {
                        var existItem = newModel.Items.FirstOrDefault(x => x.Id == item.Id);

                        if(existItem == null)
                        {
                            EntityExtension.FlagForDelete(item, user, USER_AGENT);
                        }
                        else
                        {
                            if(item.DOQuantity != existItem.DOQuantity)
                            {
                                item.DOQuantity = existItem.DOQuantity;
                                EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                            }          
                        }
                    }

                    //Add new Item
                    foreach(var item in newModel.Items)
                    {
                        if(item.Id == 0)
                        {
                            item.GarmentDOId = oldModel.Id;
                            EntityExtension.FlagForCreate(item, user, USER_AGENT);
                            EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                            dbSetItem.Add(item);

                        }
                    }

                    //Update Header Statement
                    if(oldModel.DONo != newModel.DONo)
                    {
                        oldModel.DONo = newModel.DONo;
                    }
                    if (oldModel.DODate != newModel.DODate)
                    {
                        oldModel.DODate = newModel.DODate;
                    }
                    if (oldModel.ArrivalDate != newModel.ArrivalDate)
                    {
                        oldModel.ArrivalDate = newModel.ArrivalDate;
                    }
                    if (oldModel.Remark != newModel.Remark)
                    {
                        oldModel.Remark = newModel.Remark;
                    }

                    EntityExtension.FlagForUpdate(oldModel, user, USER_AGENT);

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

        public IQueryable<GarmentSubconDeliveryOrder> DOForCustoms(string Keyword, string Filter, string currencycode = null)
        {
            IQueryable<GarmentSubconDeliveryOrder> Query = this.dbSet.Include(s => s.Items);

            List<string> searchAttributes = new List<string>()
            {
                "DONo"
            };

            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword); // kalo search setelah Select dengan .Where setelahnya maka case sensitive, kalo tanpa .Where tidak masalah
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);
            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>("{}");

            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureOrder(Query, OrderDictionary).Include(m => m.Items)
                .Where(s => s.CustomsId == 0 && s.Items.Any(x => x.CurrencyCode == currencycode));

            return Query;
        }
    }
}
