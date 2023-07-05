using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentUnitDeliveryOrderViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSubcon.GarmentSubconUnitDeliveryOrderFacades
{
    public class GarmentSubconUnitDeliveryOrderFacades : IGarmentSubconUnitDeliveryOrderFacade
    {
        private string USER_AGENT = "Facade";

        public readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentSubconUnitDeliveryOrder> dbSet;
        private readonly DbSet<GarmentSubconUnitReceiptNote> dbSetUrn;
        private readonly DbSet<GarmentSubconUnitReceiptNoteItem> dbSetUrnItems;
        private readonly IMapper mapper;

        public GarmentSubconUnitDeliveryOrderFacades(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));

            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentSubconUnitDeliveryOrder>();
            dbSetUrn = dbContext.Set<GarmentSubconUnitReceiptNote>();
            dbSetUrnItems = dbContext.Set<GarmentSubconUnitReceiptNoteItem>();
            mapper = serviceProvider == null ? null : (IMapper)serviceProvider.GetService(typeof(IMapper));
        }

        public ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentSubconUnitDeliveryOrder> Query = dbSet;

            if (!string.IsNullOrWhiteSpace(identityService.Username))
            {
                Query = Query.Where(x => x.CreatedBy == identityService.Username);
            }

            Query = Query.Where(m => m.UnitDOType != "RETUR").Select(m => new GarmentSubconUnitDeliveryOrder
            {
                Id = m.Id,
                UnitDONo = m.UnitDONo,
                UnitDODate = m.UnitDODate,
                UnitDOType = m.UnitDOType,
                UnitRequestCode = m.UnitRequestCode,
                UnitRequestName = m.UnitRequestName,
                UnitSenderCode = m.UnitSenderCode,
                UnitSenderName = m.UnitSenderName,
                StorageName = m.StorageName,
                StorageCode = m.StorageCode,
                StorageRequestCode = m.StorageRequestCode,
                StorageRequestName = m.StorageRequestName,
                IsUsed = m.IsUsed,
                RONo = m.RONo,
                Article = m.Article,
                CreatedBy = m.CreatedBy,
                LastModifiedUtc = m.LastModifiedUtc,
                UENFromNo = m.UENFromNo,
                UENFromId = m.UENFromId,
                Items = m.Items.Select(i => new GarmentSubconUnitDeliveryOrderItem
                {
                    Id = i.Id,
                    DesignColor = i.DesignColor,
                    ProductId = i.ProductId,
                    ProductCode = i.ProductCode,
                    ProductName = i.ProductName,
                }).ToList()
            });

            List<string> searchAttributes = new List<string>()
            {
                "UnitDONo", "RONo", "UnitDOType", "Article","UnitRequestName","StorageName","UnitSenderName","CreatedBy"
            };

            Query = QueryHelper<GarmentSubconUnitDeliveryOrder>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentSubconUnitDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentSubconUnitDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentSubconUnitDeliveryOrder> pageable = new Pageable<GarmentSubconUnitDeliveryOrder>(Query, Page - 1, Size);
            int TotalData = pageable.TotalCount;

            List<object> ListData = new List<object>();
            ListData.AddRange(pageable.Data.Select(s => new
            {
                s.Id,
                s.UnitDONo,
                s.UnitDODate,
                s.UnitDOType,
                s.RONo,
                s.Article,
                s.UnitRequestName,
                s.StorageName,
                s.CreatedBy,
                s.LastModifiedUtc,
                s.UnitSenderName,
                s.Items
            }));

            return new ReadResponse<object>(ListData, TotalData, OrderDictionary);
        }

        public GarmentSubconUnitDeliveryOrder ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                .Include(m => m.Items)
                .FirstOrDefault();
            return model;
        }

        public async Task<int> Create(GarmentSubconUnitDeliveryOrder GarmentSubconUnitDeliveryOrder)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    GarmentSubconUnitDeliveryOrder.Items = GarmentSubconUnitDeliveryOrder.Items.Where(x => x.IsSave).ToList();

                    EntityExtension.FlagForCreate(GarmentSubconUnitDeliveryOrder, identityService.Username, USER_AGENT);

                    GarmentSubconUnitDeliveryOrder.UnitDONo = await GenerateNo(GarmentSubconUnitDeliveryOrder);

                    foreach (var GarmentSubconUnitDeliveryOrderItem in GarmentSubconUnitDeliveryOrder.Items)
                    {
                        EntityExtension.FlagForCreate(GarmentSubconUnitDeliveryOrderItem, identityService.Username, USER_AGENT);


                        // GarmentURNItems
                        GarmentSubconUnitReceiptNoteItem garmentUrnItems = dbSetUrnItems.Single(w => w.Id == GarmentSubconUnitDeliveryOrderItem.URNItemId);

                        EntityExtension.FlagForUpdate(garmentUrnItems, identityService.Username, USER_AGENT);
                        garmentUrnItems.RemainingQuantity = garmentUrnItems.RemainingQuantity - (decimal)GarmentSubconUnitDeliveryOrderItem.Quantity;
                        garmentUrnItems.OrderQuantity = garmentUrnItems.OrderQuantity + (decimal)GarmentSubconUnitDeliveryOrderItem.Quantity;
                        //GarmentSubconUnitReceiptNote garmentUnitReceiptNote = dbSetUrn.IgnoreQueryFilters().Single(s => s.Id == GarmentSubconUnitDeliveryOrderItem.URNId);
                        //garmentUnitReceiptNote.IsUnitDO = true;

                        //GarmentSubconUnitReceiptNoteItem GarmentSubconUnitReceiptNoteItem = dbContext.GarmentUnitReceiptNoteItems.IgnoreQueryFilters().Single(s => s.Id == GarmentSubconUnitDeliveryOrderItem.URNItemId);


                        //EntityExtension.FlagForUpdate(GarmentSubconUnitReceiptNoteItem, identityService.Username, USER_AGENT);

                        //GarmentSubconUnitDeliveryOrderItem.DOCurrencyRate = GarmentSubconUnitReceiptNoteItem.DOCurrencyRate;
                        //if (GarmentSubconUnitDeliveryOrderItem.DOCurrencyRate == 0 || GarmentSubconUnitDeliveryOrderItem.DOCurrencyRate == null)
                        //{
                        //    throw new Exception("GarmentSubconUnitDeliveryOrderItem.DOCurrencyRate tidak boleh 0");
                        //}

                    }

                    dbSet.Add(GarmentSubconUnitDeliveryOrder);

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

        public async Task<int> Delete(int id)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var GarmentSubconUnitDeliveryOrder = dbSet
                        .Include(m => m.Items)
                        .SingleOrDefault(m => m.Id == id);

                    EntityExtension.FlagForDelete(GarmentSubconUnitDeliveryOrder, identityService.Username, USER_AGENT);
                    foreach (var GarmentSubconUnitDeliveryOrderItem in GarmentSubconUnitDeliveryOrder.Items)
                    {
                        EntityExtension.FlagForDelete(GarmentSubconUnitDeliveryOrderItem, identityService.Username, USER_AGENT);

                        GarmentSubconUnitReceiptNoteItem GarmentSubconUnitReceiptNoteItem = dbSetUrnItems.Single(s => s.Id == GarmentSubconUnitDeliveryOrderItem.URNItemId);
                        EntityExtension.FlagForUpdate(GarmentSubconUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                        GarmentSubconUnitReceiptNoteItem.OrderQuantity = GarmentSubconUnitReceiptNoteItem.OrderQuantity - (decimal)GarmentSubconUnitDeliveryOrderItem.Quantity;
                        GarmentSubconUnitReceiptNoteItem.RemainingQuantity = GarmentSubconUnitReceiptNoteItem.RemainingQuantity + (decimal)GarmentSubconUnitDeliveryOrderItem.Quantity;
                    }

                    Deleted = await dbContext.SaveChangesAsync();
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

        public async Task<int> Update(int id, GarmentSubconUnitDeliveryOrder GarmentSubconUnitDeliveryOrder)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    GarmentSubconUnitDeliveryOrder.Items = GarmentSubconUnitDeliveryOrder.Items.Where(x => x.IsSave).ToList();

                    var oldGarmentUnitDeliveryOrder = dbSet
                        .Include(d => d.Items)
                        //.AsNoTracking()
                        .Single(m => m.Id == id);
                    //if (oldGarmentUnitDeliveryOrder.UnitDOType == "MARKETING")
                    //{
                    //    oldGarmentUnitDeliveryOrder.UnitDODate = GarmentSubconUnitDeliveryOrder.UnitDODate;
                    //}
                    EntityExtension.FlagForUpdate(oldGarmentUnitDeliveryOrder, identityService.Username, USER_AGENT);

                    foreach (var GarmentSubconUnitDeliveryOrderItem in GarmentSubconUnitDeliveryOrder.Items)
                    {
                        if (GarmentSubconUnitDeliveryOrderItem.Id != 0)
                        {
                            var oldGarmentUnitDeliveryOrderItem = oldGarmentUnitDeliveryOrder.Items.FirstOrDefault(i => i.Id == GarmentSubconUnitDeliveryOrderItem.Id);

                            EntityExtension.FlagForUpdate(oldGarmentUnitDeliveryOrderItem, identityService.Username, USER_AGENT);

                            GarmentSubconUnitReceiptNoteItem garmentUnitReceiptNoteItem = dbSetUrnItems.Single(s => s.Id == oldGarmentUnitDeliveryOrderItem.URNItemId);
                            EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                            garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - (decimal)oldGarmentUnitDeliveryOrderItem.Quantity + (decimal)GarmentSubconUnitDeliveryOrderItem.Quantity;
                            garmentUnitReceiptNoteItem.RemainingQuantity = garmentUnitReceiptNoteItem.RemainingQuantity + (decimal)oldGarmentUnitDeliveryOrderItem.Quantity - (decimal)GarmentSubconUnitDeliveryOrderItem.Quantity;
                           
                            oldGarmentUnitDeliveryOrderItem.Quantity = GarmentSubconUnitDeliveryOrderItem.Quantity;
                            oldGarmentUnitDeliveryOrderItem.DefaultDOQuantity = GarmentSubconUnitDeliveryOrderItem.Quantity; // Jumlah DO awal mengikuti Jumlah yang diubah (reset)
                            oldGarmentUnitDeliveryOrderItem.FabricType = GarmentSubconUnitDeliveryOrderItem.FabricType;
                        }
                        else
                        {
                            EntityExtension.FlagForCreate(GarmentSubconUnitDeliveryOrderItem, identityService.Username, USER_AGENT);
                            oldGarmentUnitDeliveryOrder.Items.Add(GarmentSubconUnitDeliveryOrderItem);

                            GarmentSubconUnitReceiptNoteItem garmentUnitReceiptNoteItem = dbSetUrnItems.Single(s => s.Id == GarmentSubconUnitDeliveryOrderItem.URNItemId);
                            EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                            garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity + (decimal)GarmentSubconUnitDeliveryOrderItem.Quantity;
                            garmentUnitReceiptNoteItem.RemainingQuantity = garmentUnitReceiptNoteItem.RemainingQuantity - (decimal)GarmentSubconUnitDeliveryOrderItem.Quantity;

                           
                        }
                    }

                    foreach (var oldGarmentUnitDeliveryOrderItem in oldGarmentUnitDeliveryOrder.Items)
                    {
                        var newGarmentUnitDeliveryOrderItem = GarmentSubconUnitDeliveryOrder.Items.FirstOrDefault(i => i.Id == oldGarmentUnitDeliveryOrderItem.Id);
                        if (newGarmentUnitDeliveryOrderItem == null)
                        {
                            EntityExtension.FlagForDelete(oldGarmentUnitDeliveryOrderItem, identityService.Username, USER_AGENT);

                            GarmentSubconUnitReceiptNoteItem garmentUnitReceiptNoteItem = dbSetUrnItems.Single(s => s.Id == oldGarmentUnitDeliveryOrderItem.URNItemId);

                            EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                            garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - (decimal)oldGarmentUnitDeliveryOrderItem.Quantity;
                            garmentUnitReceiptNoteItem.RemainingQuantity = garmentUnitReceiptNoteItem.RemainingQuantity + (decimal)oldGarmentUnitDeliveryOrderItem.Quantity;
                           
                        }
                    }

                    // dbSet.Update(GarmentSubconUnitDeliveryOrder);

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

        public ReadResponse<object> ReadForUnitExpenditureNote(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            var username = identityService.Username;

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            IQueryable<GarmentSubconUnitDeliveryOrder> Query = dbSet
                .Where(x => x.UnitDONo.Contains(Keyword ?? ""))
                .Select(m => new GarmentSubconUnitDeliveryOrder
                {
                    Id = m.Id,
                    UnitDONo = m.UnitDONo,
                    UnitDOType = m.UnitDOType,
                    UnitDODate = m.UnitDODate,
                    UnitSenderId = m.UnitSenderId,
                    UnitSenderCode = m.UnitSenderCode,
                    UnitSenderName = m.UnitSenderName,
                    UnitRequestId = m.UnitRequestId,
                    UnitRequestCode = m.UnitRequestCode,
                    UnitRequestName = m.UnitRequestName,
                    StorageId = m.StorageId,
                    StorageCode = m.StorageCode,
                    StorageName = m.StorageName,
                    StorageRequestId = m.StorageRequestId,
                    StorageRequestCode = m.StorageRequestCode,
                    StorageRequestName = m.StorageRequestName,
                    IsUsed = m.IsUsed,
                    LastModifiedUtc = m.LastModifiedUtc,
                    CreatedBy = m.CreatedBy,
                    Items = m.Items.Select(i => new GarmentSubconUnitDeliveryOrderItem
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductCode = i.ProductCode,
                        ProductName = i.ProductName,
                        ProductRemark = i.ProductRemark,
                        PRItemId = i.PRItemId,
                        EPOItemId = i.EPOItemId,
                        DODetailId = i.DODetailId,
                        POItemId = i.POItemId,
                        POSerialNumber = i.POSerialNumber,
                        PricePerDealUnit = i.PricePerDealUnit,
                        Quantity = i.Quantity,
                        DefaultDOQuantity = i.DefaultDOQuantity,
                        RONo = i.RONo,
                        URNItemId = i.URNItemId,
                        URNId = i.URNId,
                        UomId = i.UomId,
                        UomUnit = i.UomUnit,
                        FabricType = i.FabricType,
                        DesignColor = i.DesignColor,
                        DOCurrencyRate = i.DOCurrencyRate,
                        BeacukaiDate =i.BeacukaiDate,
                        BeacukaiNo = i.BeacukaiNo,
                        BeacukaiType = i.BeacukaiType,
                    }).ToList()
                });

            Query = QueryHelper<GarmentSubconUnitDeliveryOrder>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentSubconUnitDeliveryOrder>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentSubconUnitDeliveryOrder> pageable = new Pageable<GarmentSubconUnitDeliveryOrder>(Query, Page - 1, Size);
            List<GarmentSubconUnitDeliveryOrder> DataModel = pageable.Data.ToList();
            int Total = pageable.TotalCount;

            List<GarmentSubconUnitDeliveryOrderViewModel> DataViewModel = mapper.Map<List<GarmentSubconUnitDeliveryOrderViewModel>>(DataModel);

            List<dynamic> listData = new List<dynamic>();
            listData.AddRange(
                DataViewModel.Select(s => new
                {
                    s.Id,
                    s.UnitDONo,
                    s.UnitDOType,
                    s.IsUsed,
                    s.Storage,
                    s.UnitDODate,
                    s.StorageRequest,
                    s.UnitRequest,
                    s.UnitSender,
                    s.CreatedBy,
                    s.LastModifiedUtc,
                    Items = s.Items.Select(i => new
                    {
                        i.Id,
                        i.ProductId,
                        i.ProductCode,
                        i.ProductName,
                        i.ProductRemark,
                        i.Quantity,
                        i.DefaultDOQuantity,
                        i.DODetailId,
                        i.EPOItemId,
                        i.FabricType,
                        i.PricePerDealUnit,
                        i.POSerialNumber,
                        i.POItemId,
                        i.PRItemId,
                        i.UomId,
                        i.UomUnit,
                        i.RONo,
                        i.URNItemId,
                        i.DesignColor,
                        i.DOCurrency,
                        i.URNId,
                        Buyer = new
                        {
                            Id = dbContext.GarmentSubconUnitReceiptNotes.Where(m => m.Id == i.URNId).Select(m => m.ProductOwnerId).FirstOrDefault(),
                            Code = dbContext.GarmentSubconUnitReceiptNotes.Where(m => m.Id == i.URNId).Select(m => m.ProductOwnerCode).FirstOrDefault()
                        },
                        i.BeacukaiNo,
                        i.BeacukaiType,
                        i.BeacukaiDate
                    }).ToList()
                }).ToList()
            );
            return new ReadResponse<object>(listData, Total, OrderDictionary);
        }
        public async Task<string> GenerateNo(GarmentSubconUnitDeliveryOrder model)
        {
            DateTimeOffset dateTimeOffset = model.UnitDODate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0));
            string Month = dateTimeOffset.ToString("MM");
            string Year = dateTimeOffset.ToString("yy");
            string Day = dateTimeOffset.ToString("dd");

            string pre = model.UnitDOType == "MARKETING" ? "DOM" : "DO";
            string unitCode = model.UnitDOType == "MARKETING" ? model.UnitSenderCode : model.UnitRequestCode;

            string no = string.Concat(pre, unitCode, Year, Month, Day);
            int Padding = 3;

            var lastDataByNo = await dbSet.Where(w => w.UnitDONo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.UnitDONo).FirstOrDefaultAsync();

            if (lastDataByNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int.TryParse(lastDataByNo.UnitDONo.Replace(no, ""), out int lastNoNumber);
                return string.Concat(no, (lastNoNumber + 1).ToString().PadLeft(Padding, '0'));
            }
        }

    }
}
