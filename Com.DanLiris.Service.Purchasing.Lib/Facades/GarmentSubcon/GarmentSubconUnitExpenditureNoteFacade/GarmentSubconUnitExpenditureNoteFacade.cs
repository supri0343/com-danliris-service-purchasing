using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentUnitExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentUnitExpenditureNoteViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSubcon.GarmentSubconUnitExpenditureNoteFacade
{
    public class GarmentSubconUnitExpenditureNoteFacade: IGarmentSubconUnitExpenditureNoteFacade
    {
        private readonly string USER_AGENT = "Facade";

        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentSubconUnitExpenditureNote> dbSet;
        private readonly DbSet<GarmentSubconUnitExpenditureNoteItem> dbSetItem;
        private readonly DbSet<GarmentSubconUnitDeliveryOrder> dbSetGarmentUnitDeliveryOrder;
        private readonly DbSet<GarmentSubconUnitDeliveryOrderItem> dbSetGarmentUnitDeliveryOrderItem;
        private readonly DbSet<GarmentSubconUnitReceiptNoteItem> dbSetGarmentUnitReceiptNoteItem;
        

        private readonly IMapper mapper;

        public GarmentSubconUnitExpenditureNoteFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentSubconUnitExpenditureNote>();
            dbSetItem = dbContext.Set<GarmentSubconUnitExpenditureNoteItem>();
            dbSetGarmentUnitDeliveryOrder = dbContext.Set<GarmentSubconUnitDeliveryOrder>();
            dbSetGarmentUnitDeliveryOrderItem = dbContext.Set<GarmentSubconUnitDeliveryOrderItem>();
            dbSetGarmentUnitReceiptNoteItem = dbContext.Set<GarmentSubconUnitReceiptNoteItem>();

            mapper = (IMapper)serviceProvider.GetService(typeof(IMapper));
        }

        public ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentSubconUnitExpenditureNote> Query = dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "UENNo", "UnitDONo", "ExpenditureType", "ExpenditureTo", "CreatedBy"
            };

            Query = QueryHelper<GarmentSubconUnitExpenditureNote>.ConfigureSearch(Query, searchAttributes, Keyword);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentSubconUnitExpenditureNote>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentSubconUnitExpenditureNote>.ConfigureOrder(Query, OrderDictionary);

            Query = Query.Select(m => new GarmentSubconUnitExpenditureNote
            {
                Id = m.Id,
                UENNo = m.UENNo,
                UnitDONo = m.UnitDONo,
                ExpenditureDate = m.ExpenditureDate,
                ExpenditureTo = m.ExpenditureTo,
                ExpenditureType = m.ExpenditureType,
                UnitDOId = m.UnitDOId,
                UnitRequestId = m.UnitRequestId,
                UnitRequestCode = m.UnitRequestCode,
                UnitRequestName = m.UnitRequestName,
                UnitSenderId = m.UnitSenderId,
                UnitSenderCode = m.UnitSenderCode,
                UnitSenderName = m.UnitSenderName,
                Items = m.Items.Select(i => new GarmentSubconUnitExpenditureNoteItem
                {
                    Id = i.Id,
                    UENId = i.UENId,
                    ProductId = i.ProductId,
                    ProductCode = i.ProductCode,
                    ProductName = i.ProductName,
                    RONo = i.RONo,
                    Quantity = i.Quantity,
                    UomId = i.UomId,
                    UomUnit = i.UomUnit,
                    ReturQuantity = i.ReturQuantity,
                    UnitDOItemId = i.UnitDOItemId,
                    FabricType = i.FabricType,
                    ProductRemark = i.ProductRemark
                }).ToList(),
                CreatedAgent = m.CreatedAgent,
                CreatedBy = m.CreatedBy,
                LastModifiedUtc = m.LastModifiedUtc,
                CreatedUtc = m.CreatedUtc
            });

            Pageable<GarmentSubconUnitExpenditureNote> pageable = new Pageable<GarmentSubconUnitExpenditureNote>(Query, Page - 1, Size);
            List<GarmentSubconUnitExpenditureNote> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            List<object> ListData = new List<object>();
            ListData.AddRange(Data.Select(s => new
            {
                s.Id,
                s.UENNo,
                s.ExpenditureDate,
                s.ExpenditureTo,
                s.ExpenditureType,
                s.UnitDONo,
                s.CreatedAgent,
                s.CreatedBy,
                s.LastModifiedUtc,
                s.UnitDOId,
                s.CreatedUtc,
                s.UnitRequestId,
                s.UnitRequestCode,
                s.UnitRequestName,
                s.UnitSenderId,
                s.UnitSenderCode,
                s.UnitSenderName,
                s.Items
            }));

            return new ReadResponse<object>(ListData, TotalData, OrderDictionary);
        }

        public GarmentSubconUnitExpenditureNoteViewModel ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
                            .Include(m => m.Items)
                            .FirstOrDefault();
            var viewModel = mapper.Map<GarmentSubconUnitExpenditureNoteViewModel>(model);

            return viewModel;
        }
        public async Task<int> Create(GarmentSubconUnitExpenditureNote garmentUnitExpenditureNote)
        {
            int Created = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    garmentUnitExpenditureNote.IsPreparing = false;
                    EntityExtension.FlagForCreate(garmentUnitExpenditureNote, identityService.Username, USER_AGENT);
                    garmentUnitExpenditureNote.UENNo = await GenerateNo(garmentUnitExpenditureNote);
                    var garmentUnitDeliveryOrder = dbSetGarmentUnitDeliveryOrder.First(d => d.Id == garmentUnitExpenditureNote.UnitDOId);
                    EntityExtension.FlagForUpdate(garmentUnitDeliveryOrder, identityService.Username, USER_AGENT);
                    garmentUnitDeliveryOrder.IsUsed = true;

                    var UnitDO = dbContext.GarmentSubconUnitDeliveryOrders.Include(d => d.Items).FirstOrDefault(d => d.Id.Equals(garmentUnitExpenditureNote.UnitDOId));
                    foreach (var unitDOItem in UnitDO.Items)
                    {
                        var unitExSaved = garmentUnitExpenditureNote.Items.FirstOrDefault(d => d.UnitDOItemId == unitDOItem.Id);
                        if (unitExSaved == null || unitExSaved.IsSave == false)
                        {
                            var garmentUnitReceiptNoteItem = dbSetGarmentUnitReceiptNoteItem.FirstOrDefault(u => u.Id == unitDOItem.URNItemId);
                            EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                            garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - (decimal)unitDOItem.Quantity;
                            garmentUnitReceiptNoteItem.RemainingQuantity +=  (decimal)unitDOItem.Quantity;
                        }
                    }

                    var garmentUnitExpenditureNoteItems = garmentUnitExpenditureNote.Items.Where(x => x.IsSave).ToList();
                    foreach (var garmentUnitExpenditureNoteItem in garmentUnitExpenditureNoteItems)
                    {
                        EntityExtension.FlagForCreate(garmentUnitExpenditureNoteItem, identityService.Username, USER_AGENT);

                        var garmentUnitDeliveryOrderItem = dbSetGarmentUnitDeliveryOrderItem.FirstOrDefault(s => s.Id == garmentUnitExpenditureNoteItem.UnitDOItemId);
                        var garmentUnitReceiptNoteItem = dbSetGarmentUnitReceiptNoteItem.FirstOrDefault(u => u.Id == garmentUnitExpenditureNoteItem.URNItemId);
                        
                        if (garmentUnitDeliveryOrderItem != null && garmentUnitReceiptNoteItem != null)
                        {
                            if (garmentUnitDeliveryOrderItem.Quantity != garmentUnitExpenditureNoteItem.Quantity)
                            {
                                EntityExtension.FlagForUpdate(garmentUnitDeliveryOrderItem, identityService.Username, USER_AGENT);
                                garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - ((decimal)garmentUnitDeliveryOrderItem.Quantity - (decimal)garmentUnitExpenditureNoteItem.Quantity);
                                garmentUnitReceiptNoteItem.RemainingQuantity = garmentUnitReceiptNoteItem.RemainingQuantity + ((decimal)garmentUnitDeliveryOrderItem.Quantity - (decimal)garmentUnitExpenditureNoteItem.Quantity);

                                garmentUnitDeliveryOrderItem.Quantity = garmentUnitExpenditureNoteItem.Quantity;
                            }
                        }

                        garmentUnitExpenditureNoteItem.Conversion = garmentUnitReceiptNoteItem.Conversion;
                        garmentUnitExpenditureNoteItem.BasicPrice = Math.Round((decimal)garmentUnitExpenditureNoteItem.PricePerDealUnit, 4);
                    }

                    dbSet.Add(garmentUnitExpenditureNote);

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

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var garmentUnitExpenditureNote = dbSet.Include(m => m.Items).Single(m => m.Id == id);

                    EntityExtension.FlagForDelete(garmentUnitExpenditureNote, identityService.Username, USER_AGENT);

                    var garmentUnitDeliveryOrder = dbSetGarmentUnitDeliveryOrder.FirstOrDefault(d => d.Id == garmentUnitExpenditureNote.UnitDOId);
                    if (garmentUnitDeliveryOrder != null)
                    {
                        EntityExtension.FlagForUpdate(garmentUnitDeliveryOrder, identityService.Username, USER_AGENT);
                        garmentUnitDeliveryOrder.IsUsed = false;
                    }

                    foreach (var garmentUnitExpenditureNoteItem in garmentUnitExpenditureNote.Items)
                    {
                        EntityExtension.FlagForDelete(garmentUnitExpenditureNoteItem, identityService.Username, USER_AGENT);
                        var garmentUnitDOItem = dbSetGarmentUnitDeliveryOrderItem.FirstOrDefault(d => d.Id == garmentUnitExpenditureNoteItem.UnitDOItemId);
                        if (garmentUnitDOItem != null)
                        {
                            garmentUnitDOItem.Quantity = garmentUnitExpenditureNoteItem.Quantity;
                        }
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

        public async Task<int> Update(int id, GarmentSubconUnitExpenditureNote garmentUnitExpenditureNote)
        {
            int Updated = 0;
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var itemIsSaveFalse = garmentUnitExpenditureNote.Items.Where(i => i.IsSave == false).ToList();
                    garmentUnitExpenditureNote.Items = garmentUnitExpenditureNote.Items.Where(x => x.IsSave).ToList();

                    var oldGarmentUnitExpenditureNote = dbSet
                        .Include(d => d.Items)
                        .Single(m => m.Id == id);

                    EntityExtension.FlagForUpdate(oldGarmentUnitExpenditureNote, identityService.Username, USER_AGENT);

                    foreach (var oldGarmentUnitExpenditureNoteItem in oldGarmentUnitExpenditureNote.Items)
                    {
                        var newGarmentUnitExpenditureNoteItem = garmentUnitExpenditureNote.Items.FirstOrDefault(i => i.Id == oldGarmentUnitExpenditureNoteItem.Id);
                        if (newGarmentUnitExpenditureNoteItem == null)
                        {
                            var coba = dbContext.GarmentSubconUnitExpenditureNotes.AsNoTracking().FirstOrDefault(d => d.Id == id);
                            coba.Items = itemIsSaveFalse;


                            EntityExtension.FlagForDelete(oldGarmentUnitExpenditureNoteItem, identityService.Username, USER_AGENT);

                            var garmentUnitDeliveryOrderItem = dbSetGarmentUnitDeliveryOrderItem.FirstOrDefault(s => s.Id == oldGarmentUnitExpenditureNoteItem.UnitDOItemId);
                            garmentUnitDeliveryOrderItem.Quantity = 0;

                            oldGarmentUnitExpenditureNoteItem.DOCurrencyRate = garmentUnitDeliveryOrderItem.DOCurrencyRate.GetValueOrDefault();

                            GarmentSubconUnitReceiptNoteItem garmentUnitReceiptNoteItem = dbContext.GarmentSubconUnitReceiptNoteItems.Single(s => s.Id == oldGarmentUnitExpenditureNoteItem.URNItemId);
                            EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                            garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - (decimal)oldGarmentUnitExpenditureNoteItem.Quantity;
                            oldGarmentUnitExpenditureNoteItem.Conversion = garmentUnitReceiptNoteItem.Conversion;
                        }
                        else
                        {
                            EntityExtension.FlagForUpdate(oldGarmentUnitExpenditureNoteItem, identityService.Username, USER_AGENT);

                            var garmentUnitDeliveryOrderItem = dbSetGarmentUnitDeliveryOrderItem.FirstOrDefault(s => s.Id == oldGarmentUnitExpenditureNoteItem.UnitDOItemId);
                            var garmentUnitReceiptNoteItem = dbSetGarmentUnitReceiptNoteItem.FirstOrDefault(u => u.Id == oldGarmentUnitExpenditureNoteItem.URNItemId);

                            if (garmentUnitDeliveryOrderItem != null && garmentUnitReceiptNoteItem != null)
                            {
                                EntityExtension.FlagForUpdate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);
                                garmentUnitReceiptNoteItem.OrderQuantity = garmentUnitReceiptNoteItem.OrderQuantity - ((decimal)oldGarmentUnitExpenditureNoteItem.Quantity - (decimal)newGarmentUnitExpenditureNoteItem.Quantity);
                                garmentUnitDeliveryOrderItem.Quantity = newGarmentUnitExpenditureNoteItem.Quantity;
                            }
                            oldGarmentUnitExpenditureNoteItem.Quantity = garmentUnitExpenditureNote.Items.FirstOrDefault(i => i.Id == oldGarmentUnitExpenditureNoteItem.Id).Quantity;
                            
                            oldGarmentUnitExpenditureNoteItem.Conversion = garmentUnitReceiptNoteItem.Conversion;
                            oldGarmentUnitExpenditureNoteItem.ItemStatus = newGarmentUnitExpenditureNoteItem.ItemStatus;
                        }
                        var basicPrice = (oldGarmentUnitExpenditureNoteItem.PricePerDealUnit * Math.Round(oldGarmentUnitExpenditureNoteItem.Quantity / (double)oldGarmentUnitExpenditureNoteItem.Conversion, 2)) / (double)oldGarmentUnitExpenditureNoteItem.Conversion;
                        oldGarmentUnitExpenditureNoteItem.BasicPrice = Math.Round((decimal)basicPrice, 4);
                    }

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


        public async Task<string> GenerateNo(GarmentSubconUnitExpenditureNote garmentUnitExpenditureNote)
        {
            string Year = garmentUnitExpenditureNote.ExpenditureDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("yy");
            string Month = garmentUnitExpenditureNote.ExpenditureDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("MM");
            string Day = garmentUnitExpenditureNote.ExpenditureDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("dd");

            string no = "";
            //if (garmentUnitExpenditureNote.ExpenditureType == "PROSES" || garmentUnitExpenditureNote.ExpenditureType == "SAMPLE" || garmentUnitExpenditureNote.ExpenditureType == "SISA" || garmentUnitExpenditureNote.ExpenditureType == "SUBCON")// || garmentUnitExpenditureNote.ExpenditureType == "EXTERNAL")
            //{
                no = string.Concat("BUK", garmentUnitExpenditureNote.UnitRequestCode, Year, Month, Day);
            //}
          
            int Padding = 3;

            var lastNo = await dbSet.Where(w => w.UENNo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.UENNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.UENNo.Replace(no, string.Empty)) + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }
    }
}
