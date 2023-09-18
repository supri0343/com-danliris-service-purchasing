using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubconDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentSubconDeliveryOrderViewModel;
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
                    ProductOwnerId = x.ProductOwnerId,
                    ProductOwnerCode = x.ProductOwnerCode,
                    ProductOwnerName = x.ProductOwnerName,
                    CreatedBy = x.CreatedBy,
                    LastModifiedUtc = x.LastModifiedUtc,
                    RONo = x.RONo,
                    BeacukaiNo = x.BeacukaiNo,
                    BeacukaiType =x.BeacukaiType,
                    BeacukaiDate =x.BeacukaiDate ,
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

        public ReadResponse<object> ReadForUnitReceiptNote(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            long filterSupplierId = FilterDictionary.ContainsKey("SupplierId") ? long.Parse(FilterDictionary["SupplierId"]) : 0;
            FilterDictionary.Remove("SupplierId");

            IQueryable<GarmentSubconDeliveryOrder> Query = dbSet
                .Where(m => m.DONo.Contains(Keyword ?? "") && (filterSupplierId == 0 ? true : m.ProductOwnerId == filterSupplierId) && m.CustomsId != 0 && m.IsReceived == false)
                .Select(m => new GarmentSubconDeliveryOrder
                {
                    Id = m.Id,
                    DONo = m.DONo,
                    RONo =m.RONo,
                    ProductOwnerId = m.ProductOwnerId,
                    ProductOwnerName = m.ProductOwnerName,
                    ProductOwnerCode = m.ProductOwnerCode,
                    BeacukaiNo = m.BeacukaiNo,
                    BeacukaiType = m.BeacukaiType,
                    BeacukaiDate = m.BeacukaiDate,
                    LastModifiedUtc = m.LastModifiedUtc,
                    Article = m.Article,
                    Items = m.Items.Select(i => new GarmentSubconDeliveryOrderItem
                    {
                        Id = i.Id,
                        POSerialNumber =i.POSerialNumber,
                        DOQuantity = i.DOQuantity,
                        PricePerDealUnit = i.PricePerDealUnit,
                        ProductId = i.ProductId,
                        ProductCode = i.ProductCode,
                        ProductName = i.ProductName,
                        ProductRemark = i.ProductRemark,
                        UomId = i.UomId,
                        UomUnit =i.UomUnit,
                    }).ToList()
                });

            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentSubconDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentSubconDeliveryOrder> pageable = new Pageable<GarmentSubconDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentSubconDeliveryOrder> DataModel = pageable.Data.ToList();
            int Total = pageable.TotalCount;

            List<GarmentSubconDeliveryOrderViewModel> DataViewModel = mapper.Map<List<GarmentSubconDeliveryOrderViewModel>>(DataModel);

            List<dynamic> listData = new List<dynamic>();
            listData.AddRange(
                DataViewModel.Select(s => new
                {
                    s.Id,
                    s.doNo,
                    s.roNo,
                    s.article,
                    s.supplier,
                    s.LastModifiedUtc,
                    s.beacukaiDate,
                    s.beacukaiNo,
                    s.beacukaiType,
                    items = s.items.Select(i => new
                    {
                        i.Id,
                        i.POSerialNumber,
                        i.Product,
                        i.DOQuantity,
                        i.PricePerDealUnit,
                        i.Uom,
                    }).ToList()
                }).ToList()
            );

            return new ReadResponse<object>(listData, Total, OrderDictionary);
        }
    }
}
