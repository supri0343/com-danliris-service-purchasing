using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
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

                    foreach (var garmentUnitDeliveryOrderItem in GarmentSubconUnitDeliveryOrder.Items)
                    {
                        EntityExtension.FlagForCreate(garmentUnitDeliveryOrderItem, identityService.Username, USER_AGENT);


                        // GarmentURNItems
                        GarmentSubconUnitReceiptNoteItem garmentUrnItems = dbSetUrnItems.Single(w => w.Id == garmentUnitDeliveryOrderItem.URNItemId);

                        EntityExtension.FlagForUpdate(garmentUrnItems, identityService.Username, USER_AGENT);
                        garmentUrnItems.RemainingQuantity = garmentUrnItems.RemainingQuantity - (decimal)garmentUnitDeliveryOrderItem.Quantity;
                        garmentUrnItems.OrderQuantity = garmentUrnItems.OrderQuantity + (decimal)garmentUnitDeliveryOrderItem.Quantity;
                        //GarmentSubconUnitReceiptNote garmentUnitReceiptNote = dbSetUrn.IgnoreQueryFilters().Single(s => s.Id == garmentUnitDeliveryOrderItem.URNId);
                        //garmentUnitReceiptNote.IsUnitDO = true;

                        //GarmentSubconUnitReceiptNoteItem GarmentSubconUnitReceiptNoteItem = dbContext.GarmentUnitReceiptNoteItems.IgnoreQueryFilters().Single(s => s.Id == garmentUnitDeliveryOrderItem.URNItemId);


                        //EntityExtension.FlagForUpdate(GarmentSubconUnitReceiptNoteItem, identityService.Username, USER_AGENT);

                        //garmentUnitDeliveryOrderItem.DOCurrencyRate = GarmentSubconUnitReceiptNoteItem.DOCurrencyRate;
                        //if (garmentUnitDeliveryOrderItem.DOCurrencyRate == 0 || garmentUnitDeliveryOrderItem.DOCurrencyRate == null)
                        //{
                        //    throw new Exception("garmentUnitDeliveryOrderItem.DOCurrencyRate tidak boleh 0");
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
                    foreach (var garmentUnitDeliveryOrderItem in GarmentSubconUnitDeliveryOrder.Items)
                    {
                        EntityExtension.FlagForDelete(garmentUnitDeliveryOrderItem, identityService.Username, USER_AGENT);

                        GarmentSubconUnitReceiptNoteItem GarmentSubconUnitReceiptNoteItem = dbSetUrnItems.Single(s => s.Id == garmentUnitDeliveryOrderItem.URNItemId);
                        EntityExtension.FlagForUpdate(GarmentSubconUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                        GarmentSubconUnitReceiptNoteItem.OrderQuantity = GarmentSubconUnitReceiptNoteItem.OrderQuantity - (decimal)garmentUnitDeliveryOrderItem.Quantity;
                        GarmentSubconUnitReceiptNoteItem.RemainingQuantity = GarmentSubconUnitReceiptNoteItem.RemainingQuantity + (decimal)garmentUnitDeliveryOrderItem.Quantity;
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

        public async Task<int> Update(int id, GarmentSubconUnitDeliveryOrder garmentUnitDeliveryOrder)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    garmentUnitDeliveryOrder.Items = garmentUnitDeliveryOrder.Items.Where(x => x.IsSave).ToList();

                    var oldGarmentUnitDeliveryOrder = dbSet
                        .Include(d => d.Items)
                        //.AsNoTracking()
                        .Single(m => m.Id == id);
                    //if (oldGarmentUnitDeliveryOrder.UnitDOType == "MARKETING")
                    //{
                    //    oldGarmentUnitDeliveryOrder.UnitDODate = garmentUnitDeliveryOrder.UnitDODate;
                    //}
                    EntityExtension.FlagForUpdate(oldGarmentUnitDeliveryOrder, identityService.Username, USER_AGENT);

                    foreach (var garmentUnitDeliveryOrderItem in garmentUnitDeliveryOrder.Items)
                    {
                        if (garmentUnitDeliveryOrderItem.Id != 0)
                        {
                            var oldGarmentUnitDeliveryOrderItem = oldGarmentUnitDeliveryOrder.Items.FirstOrDefault(i => i.Id == garmentUnitDeliveryOrderItem.Id);

                            EntityExtension.FlagForUpdate(oldGarmentUnitDeliveryOrderItem, identityService.Username, USER_AGENT);

                            GarmentSubconUnitReceiptNoteItem garmentUnitReceiptNoteItem = dbSetUrnItems.Single(s => s.Id == oldGarmentUnitDeliveryOrderItem.URNItemId);
                            EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                            garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - (decimal)oldGarmentUnitDeliveryOrderItem.Quantity + (decimal)garmentUnitDeliveryOrderItem.Quantity;
                            garmentUnitReceiptNoteItem.RemainingQuantity = garmentUnitReceiptNoteItem.RemainingQuantity + (decimal)oldGarmentUnitDeliveryOrderItem.Quantity - (decimal)garmentUnitDeliveryOrderItem.Quantity;
                           
                            oldGarmentUnitDeliveryOrderItem.Quantity = garmentUnitDeliveryOrderItem.Quantity;
                            oldGarmentUnitDeliveryOrderItem.DefaultDOQuantity = garmentUnitDeliveryOrderItem.Quantity; // Jumlah DO awal mengikuti Jumlah yang diubah (reset)
                            oldGarmentUnitDeliveryOrderItem.FabricType = garmentUnitDeliveryOrderItem.FabricType;
                        }
                        else
                        {
                            EntityExtension.FlagForCreate(garmentUnitDeliveryOrderItem, identityService.Username, USER_AGENT);
                            oldGarmentUnitDeliveryOrder.Items.Add(garmentUnitDeliveryOrderItem);

                            GarmentSubconUnitReceiptNoteItem garmentUnitReceiptNoteItem = dbSetUrnItems.Single(s => s.Id == garmentUnitDeliveryOrderItem.URNItemId);
                            EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                            garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity + (decimal)garmentUnitDeliveryOrderItem.Quantity;
                            garmentUnitReceiptNoteItem.RemainingQuantity = garmentUnitReceiptNoteItem.RemainingQuantity - (decimal)garmentUnitDeliveryOrderItem.Quantity;

                           
                        }
                    }

                    foreach (var oldGarmentUnitDeliveryOrderItem in oldGarmentUnitDeliveryOrder.Items)
                    {
                        var newGarmentUnitDeliveryOrderItem = garmentUnitDeliveryOrder.Items.FirstOrDefault(i => i.Id == oldGarmentUnitDeliveryOrderItem.Id);
                        if (newGarmentUnitDeliveryOrderItem == null)
                        {
                            EntityExtension.FlagForDelete(oldGarmentUnitDeliveryOrderItem, identityService.Username, USER_AGENT);

                            GarmentSubconUnitReceiptNoteItem garmentUnitReceiptNoteItem = dbSetUrnItems.Single(s => s.Id == oldGarmentUnitDeliveryOrderItem.URNItemId);

                            EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                            garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - (decimal)oldGarmentUnitDeliveryOrderItem.Quantity;
                            garmentUnitReceiptNoteItem.RemainingQuantity = garmentUnitReceiptNoteItem.RemainingQuantity + (decimal)oldGarmentUnitDeliveryOrderItem.Quantity;
                           
                        }
                    }

                    // dbSet.Update(garmentUnitDeliveryOrder);

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
