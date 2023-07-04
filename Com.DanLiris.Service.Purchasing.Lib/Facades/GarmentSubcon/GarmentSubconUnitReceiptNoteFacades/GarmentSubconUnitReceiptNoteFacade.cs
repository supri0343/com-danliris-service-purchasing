using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubconDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentUnitReceiptNoteViewModels;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSubcon.GarmentSubconUnitReceiptNoteFacades
{
    public class GarmentSubconUnitReceiptNoteFacade : IGarmentSubconUnitReceiptNoteFacade
    {
        private readonly string USER_AGENT = "Facade";

        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;

        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<GarmentSubconUnitReceiptNote> dbSet;
        private readonly DbSet<GarmentSubconUnitReceiptNoteItem> dbSetGarmentUnitReceiptNoteItem;

        private readonly DbSet<GarmentSubconDeliveryOrder> dbsetGarmentDeliveryOrder;
        private readonly IMapper mapper;
        public GarmentSubconUnitReceiptNoteFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));

            this.dbContext = dbContext;
            dbSet = dbContext.Set<GarmentSubconUnitReceiptNote>();
            dbSetGarmentUnitReceiptNoteItem = dbContext.Set<GarmentSubconUnitReceiptNoteItem>();
            dbsetGarmentDeliveryOrder = dbContext.Set<GarmentSubconDeliveryOrder>();

            mapper = (IMapper)serviceProvider.GetService(typeof(IMapper));
        }

        public ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<GarmentSubconUnitReceiptNote> Query = dbSet;
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<GarmentSubconUnitReceiptNote>.ConfigureFilter(Query, FilterDictionary);

            Query = Query.Select(m => new GarmentSubconUnitReceiptNote
            {
                Id = m.Id,
                URNNo = m.URNNo,
                UnitCode = m.UnitCode,
                UnitId = m.UnitId,
                UnitName = m.UnitName,
                ReceiptDate = m.ReceiptDate,
                ProductOwnerName = m.ProductOwnerName,
                DONo = m.DONo,
                DOId = m.DOId,
                DRNo = m.DRNo,
                URNType = m.URNType,
                RONo = m.RONo,
                Items = m.Items.Select(i => new GarmentSubconUnitReceiptNoteItem
                {
                    Id = i.Id,
                    ProductCode = i.ProductCode,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductRemark = i.ProductRemark,
                    OrderQuantity = i.OrderQuantity,
                    ReceiptQuantity = i.ReceiptQuantity,
                    SmallQuantity = i.SmallQuantity,
                    UomId = i.UomId,
                    UomUnit = i.UomUnit,
                    Conversion = i.Conversion,
                    DOItemId = i.DOItemId,
                    POSerialNumber = i.POSerialNumber,
                    SmallUomId = i.SmallUomId,
                    SmallUomUnit = i.SmallUomUnit,
                    PricePerDealUnit = i.PricePerDealUnit,
                    DesignColor = i.DesignColor,
                    ReceiptCorrection = i.ReceiptCorrection,
                    CorrectionConversion = i.CorrectionConversion
                }).ToList(),
                CreatedBy = m.CreatedBy,
                LastModifiedUtc = m.LastModifiedUtc,
                CreatedUtc = m.CreatedUtc
            });

            List<string> searchAttributes = new List<string>()
            {
                "URNNo", "UnitName", "ProductOwnerName", "DONo","URNType", "DRNo","RONo"
            };

            Query = QueryHelper<GarmentSubconUnitReceiptNote>.ConfigureSearch(Query, searchAttributes, Keyword);



            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<GarmentSubconUnitReceiptNote>.ConfigureOrder(Query, OrderDictionary);

            Pageable<GarmentSubconUnitReceiptNote> pageable = new Pageable<GarmentSubconUnitReceiptNote>(Query, Page - 1, Size);
            List<GarmentSubconUnitReceiptNote> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            List<object> ListData = new List<object>();
            ListData.AddRange(Data.Select(s => new
            {
                s.Id,
                s.URNNo,
                s.DOId,
                s.DRNo,
                s.URNType,
                s.RONo,
                Unit = new { Name = s.UnitName, Id = s.UnitId, Code = s.UnitCode },
                s.ReceiptDate,
                Supplier = new { Name = s.ProductOwnerName },
                s.DONo,
                Items = new List<GarmentSubconUnitReceiptNoteItem>(s.Items),
                s.CreatedBy,
                s.LastModifiedUtc,
                s.CreatedUtc
            }));

            return new ReadResponse<object>(ListData, TotalData, OrderDictionary);
        }

        public GarmentSubconUnitReceiptNote ReadById(int id)
        {
            var model = dbSet.Where(m => m.Id == id)
               .Include(m => m.Items)
               .FirstOrDefault();
            return model;
        }

        public async Task<int> Create(GarmentSubconUnitReceiptNote garmentUnitReceiptNote)
        {
            int Created = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(garmentUnitReceiptNote, identityService.Username, USER_AGENT);
                    garmentUnitReceiptNote.URNNo = await GenerateNo(garmentUnitReceiptNote);
                    garmentUnitReceiptNote.IsStorage = true;


                    if (garmentUnitReceiptNote.URNType == "TERIMA SUBCON")
                    {
                        var garmentDeliveryOrder = dbsetGarmentDeliveryOrder.First(d => d.Id == garmentUnitReceiptNote.DOId);
                        EntityExtension.FlagForUpdate(garmentDeliveryOrder, identityService.Username, USER_AGENT);
                        garmentDeliveryOrder.IsReceived = true;
                    }

                    foreach (var garmentUnitReceiptNoteItem in garmentUnitReceiptNote.Items)
                    {
                        EntityExtension.FlagForCreate(garmentUnitReceiptNoteItem, identityService.Username, USER_AGENT);

                        if (garmentUnitReceiptNote.URNType == "PROSES")
                        {
                            //await UpdateDR(garmentUnitReceiptNote.DRId, true, garmentUnitReceiptNote.UnitCode);
                        }
                    }

                    dbSet.Add(garmentUnitReceiptNote);
                    Created = await dbContext.SaveChangesAsync();

                    #region Proses dari DR
                    //if (garmentUnitReceiptNote.URNType == "PROSES")
                    //{
                    //    //await UpdateDR(garmentUnitReceiptNote.DRId, true);
                    //    var GarmentDR = GetDR(garmentUnitReceiptNote.DRId, garmentUnitReceiptNote.UnitCode);
                    //    var GarmentUnitDO = dbContext.GarmentUnitDeliveryOrders.AsNoTracking().Single(a => a.Id == GarmentDR.UnitDOId);
                    //    List<GarmentUnitDeliveryOrderItem> unitDOItems = new List<GarmentUnitDeliveryOrderItem>();
                    //    foreach (var garmentUnitReceiptNoteItem in garmentUnitReceiptNote.Items)
                    //    {
                    //        GarmentDOItems garmentDOItems = new GarmentDOItems
                    //        {
                    //            DOItemNo = await GenerateNoDOItems(garmentUnitReceiptNote),
                    //            UnitId = garmentUnitReceiptNote.UnitId,
                    //            UnitCode = garmentUnitReceiptNote.UnitCode,
                    //            UnitName = garmentUnitReceiptNote.UnitName,
                    //            StorageCode = garmentUnitReceiptNote.StorageCode,
                    //            StorageId = garmentUnitReceiptNote.StorageId,
                    //            StorageName = garmentUnitReceiptNote.StorageName,
                    //            POId = garmentUnitReceiptNoteItem.POId,
                    //            POItemId = garmentUnitReceiptNoteItem.POItemId,
                    //            POSerialNumber = garmentUnitReceiptNoteItem.POSerialNumber,
                    //            ProductCode = garmentUnitReceiptNoteItem.ProductCode,
                    //            ProductId = garmentUnitReceiptNoteItem.ProductId,
                    //            ProductName = garmentUnitReceiptNoteItem.ProductName,
                    //            DesignColor = garmentUnitReceiptNoteItem.DesignColor,
                    //            SmallQuantity = garmentUnitReceiptNoteItem.SmallQuantity,
                    //            SmallUomId = garmentUnitReceiptNoteItem.SmallUomId,
                    //            SmallUomUnit = garmentUnitReceiptNoteItem.SmallUomUnit,
                    //            RemainingQuantity = GarmentUnitDO.UnitDOFromId != 0 ? 0 : garmentUnitReceiptNoteItem.SmallQuantity,
                    //            DetailReferenceId = garmentUnitReceiptNoteItem.DODetailId,
                    //            URNItemId = garmentUnitReceiptNoteItem.Id,
                    //            DOCurrencyRate = garmentUnitReceiptNoteItem.DOCurrencyRate,
                    //            EPOItemId = garmentUnitReceiptNoteItem.EPOItemId,
                    //            PRItemId = garmentUnitReceiptNoteItem.PRItemId,
                    //            RO = garmentUnitReceiptNoteItem.RONo,
                    //            Rack = garmentUnitReceiptNoteItem.Rack,
                    //            Level = garmentUnitReceiptNoteItem.Level,
                    //            Box = garmentUnitReceiptNoteItem.Box,
                    //            Area = garmentUnitReceiptNoteItem.Area,
                    //            Colour = garmentUnitReceiptNoteItem.Colour,
                    //            SplitQuantity = GarmentUnitDO.UnitDOFromId != 0 ? 0 : garmentUnitReceiptNoteItem.SmallQuantity,

                    //        };
                    //        EntityExtension.FlagForCreate(garmentDOItems, identityService.Username, USER_AGENT);
                    //        dbSetGarmentDOItems.Add(garmentDOItems);
                    //        await dbContext.SaveChangesAsync();
                    //    }

                    //    if (GarmentUnitDO.UnitDOFromId != 0)
                    //    {
                    //        GarmentUnitDeliveryOrderFacade garmentUnitDeliveryOrderFacade = new GarmentUnitDeliveryOrderFacade(dbContext, serviceProvider);
                    //        GarmentUnitExpenditureNoteFacade.GarmentUnitExpenditureNoteFacade garmentUnitExpenditureNoteFacade = new GarmentUnitExpenditureNoteFacade.GarmentUnitExpenditureNoteFacade(serviceProvider, dbContext);
                    //        var GarmentUnitDOFrom = dbContext.GarmentUnitDeliveryOrders.AsNoTracking().Single(a => a.Id == GarmentUnitDO.UnitDOFromId);
                    //        foreach (var item in garmentUnitReceiptNote.Items)
                    //        {
                    //            GarmentUnitDeliveryOrderItem garmentUnitDeliveryOrderItem = new GarmentUnitDeliveryOrderItem
                    //            {
                    //                URNId = garmentUnitReceiptNote.Id,
                    //                URNNo = garmentUnitReceiptNote.URNNo,
                    //                URNItemId = item.Id,
                    //                DODetailId = item.DODetailId,
                    //                EPOItemId = item.EPOItemId,
                    //                POItemId = item.POItemId,
                    //                POSerialNumber = item.POSerialNumber,
                    //                PRItemId = item.PRItemId,
                    //                ProductId = item.ProductId,
                    //                ProductCode = item.ProductCode,
                    //                ProductName = item.ProductName,
                    //                ProductRemark = item.ProductRemark,
                    //                RONo = item.RONo,
                    //                Quantity = (double)item.SmallQuantity,
                    //                UomId = item.SmallUomId,
                    //                UomUnit = item.SmallUomUnit,
                    //                PricePerDealUnit = (double)item.PricePerDealUnit,
                    //                DesignColor = item.DesignColor,
                    //                DefaultDOQuantity = (double)item.SmallQuantity,
                    //                DOCurrencyRate = item.DOCurrencyRate,
                    //                ReturQuantity = 0
                    //            };
                    //            unitDOItems.Add(garmentUnitDeliveryOrderItem);
                    //            EntityExtension.FlagForCreate(garmentUnitDeliveryOrderItem, identityService.Username, USER_AGENT);
                    //        }
                    //        var rono = garmentUnitReceiptNote.Items.First().RONo;
                    //        var pr = dbContext.GarmentPurchaseRequests.AsNoTracking().FirstOrDefault(p => p.RONo == rono);
                    //        GarmentUnitDeliveryOrder garmentUnitDeliveryOrder = new GarmentUnitDeliveryOrder
                    //        {
                    //            UnitDOType = "TRANSFER",
                    //            UnitDODate = garmentUnitReceiptNote.ReceiptDate,
                    //            UnitRequestCode = GarmentUnitDOFrom.UnitSenderCode,
                    //            UnitRequestId = GarmentUnitDOFrom.UnitSenderId,
                    //            UnitRequestName = GarmentUnitDOFrom.UnitSenderName,
                    //            UnitSenderId = garmentUnitReceiptNote.UnitId,
                    //            UnitSenderName = garmentUnitReceiptNote.UnitName,
                    //            UnitSenderCode = garmentUnitReceiptNote.UnitCode,
                    //            StorageId = garmentUnitReceiptNote.StorageId,
                    //            StorageCode = garmentUnitReceiptNote.StorageCode,
                    //            StorageName = garmentUnitReceiptNote.StorageName,
                    //            RONo = rono,
                    //            Article = pr.Article,
                    //            IsUsed = true,
                    //            StorageRequestCode = GarmentUnitDOFrom.StorageCode,
                    //            StorageRequestId = GarmentUnitDOFrom.StorageId,
                    //            StorageRequestName = GarmentUnitDOFrom.StorageName,
                    //            Items = unitDOItems
                    //        };
                    //        garmentUnitDeliveryOrder.UnitDONo = await garmentUnitDeliveryOrderFacade.GenerateNo(garmentUnitDeliveryOrder);
                    //        EntityExtension.FlagForCreate(garmentUnitDeliveryOrder, identityService.Username, USER_AGENT);

                    //        dbSetGarmentUnitDeliveryOrder.Add(garmentUnitDeliveryOrder);
                    //        await dbContext.SaveChangesAsync();

                    //        List<GarmentUnitExpenditureNoteItem> uenItems = new List<GarmentUnitExpenditureNoteItem>();
                    //        foreach (var unitDOItem in garmentUnitDeliveryOrder.Items)
                    //        {
                    //            var poItem = dbContext.GarmentInternalPurchaseOrderItems.AsNoTracking().Single(a => a.Id == unitDOItem.POItemId);
                    //            var po = dbContext.GarmentInternalPurchaseOrders.AsNoTracking().Single(a => a.Id == poItem.GPOId);
                    //            var urnItem = dbContext.GarmentUnitReceiptNoteItems.AsNoTracking().Single(a => a.Id == unitDOItem.URNItemId);
                    //            GarmentUnitExpenditureNoteItem garmentUnitExpenditureNoteItem = new GarmentUnitExpenditureNoteItem
                    //            {
                    //                UnitDOItemId = unitDOItem.Id,
                    //                URNItemId = unitDOItem.URNItemId,
                    //                DODetailId = unitDOItem.DODetailId,
                    //                EPOItemId = unitDOItem.EPOItemId,
                    //                POItemId = unitDOItem.POItemId,
                    //                PRItemId = unitDOItem.PRItemId,
                    //                POSerialNumber = unitDOItem.POSerialNumber,
                    //                ProductId = unitDOItem.ProductId,
                    //                ProductName = unitDOItem.ProductName,
                    //                ProductCode = unitDOItem.ProductCode,
                    //                ProductRemark = unitDOItem.ProductRemark,
                    //                RONo = unitDOItem.RONo,
                    //                Quantity = unitDOItem.Quantity,
                    //                UomId = unitDOItem.UomId,
                    //                UomUnit = unitDOItem.UomUnit,
                    //                PricePerDealUnit = unitDOItem.PricePerDealUnit,
                    //                FabricType = unitDOItem.FabricType,
                    //                BuyerId = Convert.ToInt64(po.BuyerId),
                    //                BuyerCode = po.BuyerCode,
                    //                BasicPrice = (decimal)(unitDOItem.PricePerDealUnit * unitDOItem.DOCurrencyRate),
                    //                Conversion = urnItem.Conversion,
                    //                ReturQuantity = 0,
                    //                DOCurrencyRate = unitDOItem.DOCurrencyRate
                    //            };
                    //            uenItems.Add(garmentUnitExpenditureNoteItem);
                    //            EntityExtension.FlagForCreate(garmentUnitExpenditureNoteItem, identityService.Username, USER_AGENT);

                    //        }
                    //        GarmentUnitExpenditureNote garmentUnitExpenditureNote = new GarmentUnitExpenditureNote
                    //        {
                    //            ExpenditureDate = garmentUnitDeliveryOrder.UnitDODate,
                    //            ExpenditureType = "TRANSFER",
                    //            ExpenditureTo = "GUDANG LAIN",
                    //            UnitDOId = garmentUnitDeliveryOrder.Id,
                    //            UnitDONo = garmentUnitDeliveryOrder.UnitDONo,
                    //            UnitSenderId = garmentUnitDeliveryOrder.UnitSenderId,
                    //            UnitSenderCode = garmentUnitDeliveryOrder.UnitSenderCode,
                    //            UnitSenderName = garmentUnitDeliveryOrder.UnitSenderName,
                    //            StorageId = garmentUnitDeliveryOrder.StorageId,
                    //            StorageCode = garmentUnitDeliveryOrder.StorageCode,
                    //            StorageName = garmentUnitDeliveryOrder.StorageName,
                    //            UnitRequestCode = garmentUnitDeliveryOrder.UnitRequestCode,
                    //            UnitRequestId = garmentUnitDeliveryOrder.UnitRequestId,
                    //            UnitRequestName = garmentUnitDeliveryOrder.UnitRequestName,
                    //            StorageRequestCode = garmentUnitDeliveryOrder.StorageRequestCode,
                    //            StorageRequestId = garmentUnitDeliveryOrder.StorageRequestId,
                    //            StorageRequestName = garmentUnitDeliveryOrder.StorageRequestName,
                    //            IsTransfered = true,
                    //            Items = uenItems
                    //        };
                    //        garmentUnitExpenditureNote.UENNo = await garmentUnitExpenditureNoteFacade.GenerateNo(garmentUnitExpenditureNote);
                    //        EntityExtension.FlagForCreate(garmentUnitExpenditureNote, identityService.Username, USER_AGENT);

                    //        dbSetGarmentUnitExpenditureNote.Add(garmentUnitExpenditureNote);
                    //        await dbContext.SaveChangesAsync();

                    //        var garmentInventoryDocumentOut = garmentUnitExpenditureNoteFacade.GenerateGarmentInventoryDocument(garmentUnitExpenditureNote, "OUT");
                    //        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentOut);

                    //        List<GarmentUnitReceiptNoteItem> urnItems = new List<GarmentUnitReceiptNoteItem>();

                    //        foreach (var uenItem in uenItems)
                    //        {
                    //            var garmentInventorySummaryExistingBUK = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == uenItem.ProductId && s.StorageId == garmentUnitExpenditureNote.StorageId && s.UomId == uenItem.UomId);

                    //            var garmentInventoryMovement = garmentUnitExpenditureNoteFacade.GenerateGarmentInventoryMovement(garmentUnitExpenditureNote, uenItem, garmentInventorySummaryExistingBUK, "OUT");
                    //            dbSetGarmentInventoryMovement.Add(garmentInventoryMovement);

                    //            if (garmentInventorySummaryExistingBUK == null)
                    //            {
                    //                var garmentInventorySummary = garmentUnitExpenditureNoteFacade.GenerateGarmentInventorySummary(garmentUnitExpenditureNote, uenItem, garmentInventoryMovement);
                    //                dbSetGarmentInventorySummary.Add(garmentInventorySummary);
                    //            }
                    //            else
                    //            {
                    //                EntityExtension.FlagForUpdate(garmentInventorySummaryExistingBUK, identityService.Username, USER_AGENT);
                    //                garmentInventorySummaryExistingBUK.Quantity = garmentInventoryMovement.After;
                    //            }

                    //            await dbContext.SaveChangesAsync();

                    //            var pritem = dbContext.GarmentPurchaseRequestItems.AsNoTracking().FirstOrDefault(p => p.Id == uenItem.PRItemId);
                    //            var prHeader = dbContext.GarmentPurchaseRequests.AsNoTracking().FirstOrDefault(p => p.Id == pritem.GarmentPRId);
                    //            var poItem = dbContext.GarmentInternalPurchaseOrderItems.AsNoTracking().FirstOrDefault(p => p.Id == uenItem.POItemId);
                    //            var urnitem = dbContext.GarmentUnitReceiptNoteItems.AsNoTracking().FirstOrDefault(a => a.Id == uenItem.URNItemId);
                    //            var unitDOitem = dbContext.GarmentUnitDeliveryOrderItems.AsNoTracking().FirstOrDefault(a => a.Id == uenItem.UnitDOItemId);

                    //            GarmentUnitReceiptNoteItem garmentURNItem = new GarmentUnitReceiptNoteItem
                    //            {
                    //                DODetailId = uenItem.DODetailId,
                    //                EPOItemId = uenItem.EPOItemId,
                    //                PRItemId = uenItem.PRItemId,
                    //                PRId = prHeader.Id,
                    //                PRNo = prHeader.PRNo,
                    //                POId = poItem.GPOId,
                    //                POItemId = uenItem.POItemId,
                    //                POSerialNumber = uenItem.POSerialNumber,
                    //                ProductId = uenItem.ProductId,
                    //                ProductCode = uenItem.ProductCode,
                    //                ProductName = uenItem.ProductName,
                    //                ProductRemark = uenItem.ProductRemark,
                    //                RONo = uenItem.RONo,
                    //                ReceiptQuantity = (decimal)uenItem.Quantity / uenItem.Conversion,
                    //                UomId = urnitem.UomId,
                    //                UomUnit = urnitem.UomUnit,
                    //                PricePerDealUnit = (decimal)uenItem.PricePerDealUnit,
                    //                DesignColor = unitDOitem.DesignColor,
                    //                IsCorrection = false,
                    //                Conversion = uenItem.Conversion,
                    //                SmallQuantity = (decimal)uenItem.Quantity,
                    //                SmallUomId = uenItem.UomId,
                    //                SmallUomUnit = uenItem.UomUnit,
                    //                ReceiptCorrection = (decimal)uenItem.Quantity / uenItem.Conversion,
                    //                CorrectionConversion = uenItem.Conversion,
                    //                OrderQuantity = 0,
                    //                DOCurrencyRate = uenItem.DOCurrencyRate != null ? (double)uenItem.DOCurrencyRate : 0,
                    //                UENItemId = uenItem.Id
                    //            };
                    //            urnItems.Add(garmentURNItem);
                    //            EntityExtension.FlagForCreate(garmentURNItem, identityService.Username, USER_AGENT);
                    //        }

                    //        GarmentUnitReceiptNote garmentUrn = new GarmentUnitReceiptNote
                    //        {
                    //            URNType = "GUDANG LAIN",
                    //            UnitId = garmentUnitExpenditureNote.UnitRequestId,
                    //            UnitCode = garmentUnitExpenditureNote.UnitRequestCode,
                    //            UnitName = garmentUnitExpenditureNote.UnitRequestName,
                    //            UENId = garmentUnitExpenditureNote.Id,
                    //            UENNo = garmentUnitExpenditureNote.UENNo,
                    //            ReceiptDate = garmentUnitExpenditureNote.ExpenditureDate,
                    //            IsStorage = true,
                    //            StorageId = garmentUnitExpenditureNote.StorageRequestId,
                    //            StorageCode = garmentUnitExpenditureNote.StorageRequestCode,
                    //            StorageName = garmentUnitExpenditureNote.StorageRequestName,
                    //            IsCorrection = false,
                    //            IsUnitDO = false,
                    //            Items = urnItems
                    //        };
                    //        garmentUrn.URNNo = await GenerateNo(garmentUrn);
                    //        EntityExtension.FlagForCreate(garmentUrn, identityService.Username, USER_AGENT);

                    //        dbSet.Add(garmentUrn);

                    //        var garmentInventoryDocument2 = GenerateGarmentInventoryDocument(garmentUrn);
                    //        dbSetGarmentInventoryDocument.Add(garmentInventoryDocument2);

                    //        foreach (var gurnItem in urnItems)
                    //        {
                    //            var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == gurnItem.ProductId && s.StorageId == garmentUrn.StorageId && s.UomId == gurnItem.SmallUomId);

                    //            var garmentInventoryMovement = GenerateGarmentInventoryMovement(garmentUrn, gurnItem, garmentInventorySummaryExisting);
                    //            dbSetGarmentInventoryMovement.Add(garmentInventoryMovement);

                    //            if (garmentInventorySummaryExisting == null)
                    //            {
                    //                var garmentInventorySummary = GenerateGarmentInventorySummary(garmentUrn, gurnItem, garmentInventoryMovement);
                    //                dbSetGarmentInventorySummary.Add(garmentInventorySummary);
                    //            }
                    //            else
                    //            {
                    //                EntityExtension.FlagForUpdate(garmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                    //                garmentInventorySummaryExisting.Quantity = garmentInventoryMovement.After;
                    //            }

                    //            await dbContext.SaveChangesAsync();
                    //        }

                    //        foreach (var garmentUnitReceiptNoteItem in garmentUrn.Items)
                    //        {
                    //            GarmentDOItems garmentDOItems = new GarmentDOItems
                    //            {
                    //                DOItemNo = await GenerateNoDOItems(garmentUrn),
                    //                UnitId = garmentUrn.UnitId,
                    //                UnitCode = garmentUrn.UnitCode,
                    //                UnitName = garmentUrn.UnitName,
                    //                StorageCode = garmentUrn.StorageCode,
                    //                StorageId = garmentUrn.StorageId,
                    //                StorageName = garmentUrn.StorageName,
                    //                POId = garmentUnitReceiptNoteItem.POId,
                    //                POItemId = garmentUnitReceiptNoteItem.POItemId,
                    //                POSerialNumber = garmentUnitReceiptNoteItem.POSerialNumber,
                    //                ProductCode = garmentUnitReceiptNoteItem.ProductCode,
                    //                ProductId = garmentUnitReceiptNoteItem.ProductId,
                    //                ProductName = garmentUnitReceiptNoteItem.ProductName,
                    //                DesignColor = garmentUnitReceiptNoteItem.DesignColor,
                    //                SmallQuantity = garmentUnitReceiptNoteItem.SmallQuantity,
                    //                SmallUomId = garmentUnitReceiptNoteItem.SmallUomId,
                    //                SmallUomUnit = garmentUnitReceiptNoteItem.SmallUomUnit,
                    //                RemainingQuantity = garmentUnitReceiptNoteItem.SmallQuantity,
                    //                DetailReferenceId = garmentUnitReceiptNoteItem.DODetailId,
                    //                URNItemId = garmentUnitReceiptNoteItem.Id,
                    //                DOCurrencyRate = garmentUnitReceiptNoteItem.DOCurrencyRate,
                    //                EPOItemId = garmentUnitReceiptNoteItem.EPOItemId,
                    //                PRItemId = garmentUnitReceiptNoteItem.PRItemId,
                    //                RO = garmentUnitReceiptNoteItem.RONo,

                    //                Rack = garmentUnitReceiptNoteItem.Rack,
                    //                Level = garmentUnitReceiptNoteItem.Level,
                    //                Box = garmentUnitReceiptNoteItem.Box,
                    //                Area = garmentUnitReceiptNoteItem.Area,
                    //                Colour = garmentUnitReceiptNoteItem.Colour,
                    //                SplitQuantity = garmentUnitReceiptNoteItem.SmallQuantity,


                    //            };
                    //            EntityExtension.FlagForCreate(garmentDOItems, identityService.Username, USER_AGENT);
                    //            dbSetGarmentDOItems.Add(garmentDOItems);
                    //            await dbContext.SaveChangesAsync();
                    //        }

                    //        await dbContext.SaveChangesAsync();
                    //    }

                    //}
                    #endregion

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

        public async Task<int> Delete(int id, string deletedReason)
        {
            int Deleted = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var garmentUnitReceiptNote = dbSet.Include(m => m.Items).Single(m => m.Id == id);

                    garmentUnitReceiptNote.DeletedReason = deletedReason;
                    EntityExtension.FlagForDelete(garmentUnitReceiptNote, identityService.Username, USER_AGENT);

                    if (garmentUnitReceiptNote.URNType == "TERIMA SUBCON")
                    {
                        var garmentDeliveryOrder = dbsetGarmentDeliveryOrder.First(d => d.Id == garmentUnitReceiptNote.DOId);
                        EntityExtension.FlagForUpdate(garmentDeliveryOrder, identityService.Username, USER_AGENT);
                        garmentDeliveryOrder.IsReceived = false;
                    }

                    foreach(var item in garmentUnitReceiptNote.Items)
                    {
                        EntityExtension.FlagForDelete(item, identityService.Username, USER_AGENT);
                    }

                    #region Proses dari DR
                    //if (garmentUnitReceiptNote.URNType == "PROSES")
                    //{
                    //    await UpdateDR(garmentUnitReceiptNote.DRId, false, garmentUnitReceiptNote.UnitCode);
                    //    var GarmentDR = GetDR(garmentUnitReceiptNote.DRId, garmentUnitReceiptNote.UnitCode);
                    //    var GarmentUnitDO = dbContext.GarmentUnitDeliveryOrders.AsNoTracking().Single(a => a.Id == GarmentDR.UnitDOId);
                    //    if (GarmentUnitDO.UnitDOFromId != 0)
                    //    {
                    //        var garmentUnitDOItem = dbContext.GarmentUnitDeliveryOrderItems.FirstOrDefault(x => x.URNId == garmentUnitReceiptNote.Id);
                    //        var unitDO = dbContext.GarmentUnitDeliveryOrders.Include(m => m.Items).Single(a => a.Id == garmentUnitDOItem.UnitDOId);
                    //        EntityExtension.FlagForDelete(unitDO, identityService.Username, USER_AGENT);
                    //        foreach (var uDOItem in unitDO.Items)
                    //        {
                    //            EntityExtension.FlagForDelete(uDOItem, identityService.Username, USER_AGENT);
                    //        }

                    //        var garmentExpenditureNote = dbContext.GarmentUnitExpenditureNotes.Include(m => m.Items).Single(x => x.UnitDOId == unitDO.Id);
                    //        EntityExtension.FlagForDelete(garmentExpenditureNote, identityService.Username, USER_AGENT);
                    //        GarmentUnitExpenditureNoteFacade.GarmentUnitExpenditureNoteFacade garmentUnitExpenditureNoteFacade = new GarmentUnitExpenditureNoteFacade.GarmentUnitExpenditureNoteFacade(serviceProvider, dbContext);




                    //        //var garmentInventoryDocument = GenerateGarmentInventoryDocument(garmentUnitReceiptNote, "OUT");
                    //        //dbSetGarmentInventoryDocument.Add(garmentInventoryDocument);

                    //        //foreach (var garmentUnitReceiptNoteItem in garmentUnitReceiptNote.Items)
                    //        //{
                    //        //    var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == garmentUnitReceiptNoteItem.ProductId && s.StorageId == garmentUnitReceiptNote.StorageId && s.UomId == garmentUnitReceiptNoteItem.SmallUomId);

                    //        //    var garmentInventoryMovement = GenerateGarmentInventoryMovement(garmentUnitReceiptNote, garmentUnitReceiptNoteItem, garmentInventorySummaryExisting, "OUT");
                    //        //    dbSetGarmentInventoryMovement.Add(garmentInventoryMovement);

                    //        //    if (garmentInventorySummaryExisting != null)
                    //        //    {
                    //        //        EntityExtension.FlagForUpdate(garmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                    //        //        garmentInventorySummaryExisting.Quantity = garmentInventoryMovement.After;
                    //        //    }

                    //        //    await dbContext.SaveChangesAsync();
                    //        //}

                    //        var gURN = dbSet.Include(m => m.Items).Single(x => x.UENId == garmentExpenditureNote.Id);
                    //        EntityExtension.FlagForDelete(gURN, identityService.Username, USER_AGENT);

                    //        var garmentInventoryDocument1 = GenerateGarmentInventoryDocument(gURN, "OUT");
                    //        dbSetGarmentInventoryDocument.Add(garmentInventoryDocument1);

                    //        foreach (var gURNItem in gURN.Items)
                    //        {
                    //            EntityExtension.FlagForDelete(gURNItem, identityService.Username, USER_AGENT);

                    //            var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == gURNItem.ProductId && s.StorageId == gURN.StorageId && s.UomId == gURNItem.SmallUomId);

                    //            var garmentInventoryMovement = GenerateGarmentInventoryMovement(gURN, gURNItem, garmentInventorySummaryExisting, "OUT");
                    //            dbSetGarmentInventoryMovement.Add(garmentInventoryMovement);

                    //            if (garmentInventorySummaryExisting != null)
                    //            {
                    //                EntityExtension.FlagForUpdate(garmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                    //                garmentInventorySummaryExisting.Quantity = garmentInventoryMovement.After;
                    //            }
                    //        }

                    //        var garmentInventoryDocumentOut = garmentUnitExpenditureNoteFacade.GenerateGarmentInventoryDocument(garmentExpenditureNote);
                    //        dbSetGarmentInventoryDocument.Add(garmentInventoryDocumentOut);

                    //        foreach (var uenItem in garmentExpenditureNote.Items)
                    //        {
                    //            EntityExtension.FlagForDelete(uenItem, identityService.Username, USER_AGENT);

                    //            var garmentInventorySummaryExistingBUK = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == uenItem.ProductId && s.StorageId == garmentExpenditureNote.StorageId && s.UomId == uenItem.UomId);

                    //            var garmentInventoryMovement = garmentUnitExpenditureNoteFacade.GenerateGarmentInventoryMovement(garmentExpenditureNote, uenItem, garmentInventorySummaryExistingBUK);
                    //            dbSetGarmentInventoryMovement.Add(garmentInventoryMovement);

                    //            if (garmentInventorySummaryExistingBUK == null)
                    //            {
                    //                var garmentInventorySummary = garmentUnitExpenditureNoteFacade.GenerateGarmentInventorySummary(garmentExpenditureNote, uenItem, garmentInventoryMovement);
                    //                dbSetGarmentInventorySummary.Add(garmentInventorySummary);
                    //            }
                    //            else
                    //            {
                    //                EntityExtension.FlagForUpdate(garmentInventorySummaryExistingBUK, identityService.Username, USER_AGENT);
                    //                garmentInventorySummaryExistingBUK.Quantity = garmentInventoryMovement.After;
                    //            }

                    //            await dbContext.SaveChangesAsync();
                    //        }
                    //    }

                    //    var garmentInventoryDocument = GenerateGarmentInventoryDocument(garmentUnitReceiptNote, "OUT");
                    //    dbSetGarmentInventoryDocument.Add(garmentInventoryDocument);

                    //    foreach (var garmentUnitReceiptNoteItem in garmentUnitReceiptNote.Items)
                    //    {
                    //        var garmentInventorySummaryExisting = dbSetGarmentInventorySummary.SingleOrDefault(s => s.ProductId == garmentUnitReceiptNoteItem.ProductId && s.StorageId == garmentUnitReceiptNote.StorageId && s.UomId == garmentUnitReceiptNoteItem.SmallUomId);

                    //        var garmentInventoryMovement = GenerateGarmentInventoryMovement(garmentUnitReceiptNote, garmentUnitReceiptNoteItem, garmentInventorySummaryExisting, "OUT");
                    //        dbSetGarmentInventoryMovement.Add(garmentInventoryMovement);

                    //        if (garmentInventorySummaryExisting != null)
                    //        {
                    //            EntityExtension.FlagForUpdate(garmentInventorySummaryExisting, identityService.Username, USER_AGENT);
                    //            garmentInventorySummaryExisting.Quantity = garmentInventoryMovement.After;
                    //        }
                    //    }

                    //}
                    #endregion

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


        public async Task<string> GenerateNo(GarmentSubconUnitReceiptNote garmentUnitReceiptNote)
        {
            string Year = garmentUnitReceiptNote.ReceiptDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("yy");
            string Month = garmentUnitReceiptNote.ReceiptDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("MM");
            //string Day = garmentUnitReceiptNote.ReceiptDate.ToOffset(new TimeSpan(identityService.TimezoneOffset, 0, 0)).ToString("dd");

            string no = string.Concat("BUMSUB", garmentUnitReceiptNote.UnitCode, Year, Month);
            int Padding = 3;

            var lastNo = await dbSet.Where(w => w.URNNo.StartsWith(no) && !w.IsDeleted).OrderByDescending(o => o.URNNo).FirstOrDefaultAsync();

            if (lastNo == null)
            {
                return no + "1".PadLeft(Padding, '0');
            }
            else
            {
                int lastNoNumber = Int32.Parse(lastNo.URNNo.Replace(no, string.Empty)) + 1;
                return no + lastNoNumber.ToString().PadLeft(Padding, '0');
            }
        }

        //private async Task UpdateDR(string DRId, bool isUsed, string unitCode)
        //{
        //    string drUri = unitCode == "SMP1" ? "garment-sample-delivery-returns" : "delivery-returns";
        //    IHttpClientService httpClient = (IHttpClientService)serviceProvider.GetService(typeof(IHttpClientService));

        //    var response = await httpClient.GetAsync($"{APIEndpoint.GarmentProduction}{drUri}/{DRId}");
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var content = await response.Content.ReadAsStringAsync();
        //        Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
        //        GarmentDeliveryReturnViewModel viewModel = JsonConvert.DeserializeObject<GarmentDeliveryReturnViewModel>(result.GetValueOrDefault("data").ToString());
        //        viewModel.IsUsed = isUsed;
        //        foreach (var item in viewModel.Items)
        //        {
        //            item.QuantityUENItem = item.Quantity + 1;
        //            item.RemainingQuantityPreparingItem = item.Quantity + 1;
        //            item.IsSave = true;
        //        }

        //        //var httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));
        //        var response2 = await httpClient.PutAsync($"{APIEndpoint.GarmentProduction}{drUri}/{DRId}", new StringContent(JsonConvert.SerializeObject(viewModel).ToString(), Encoding.UTF8, General.JsonMediaType));
        //        var content2 = await response2.Content.ReadAsStringAsync();
        //        response2.EnsureSuccessStatusCode();
        //    }

        //}

        //private GarmentDeliveryReturnViewModel GetDR(string DRId, string UnitCode)
        //{
        //    string drUri = UnitCode == "SMP1" ? "garment-sample-delivery-returns" : "delivery-returns";
        //    IHttpClientService httpClient = (IHttpClientService)serviceProvider.GetService(typeof(IHttpClientService));

        //    var response = httpClient.GetAsync($"{APIEndpoint.GarmentProduction}{drUri}/{DRId}").Result;
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var content = response.Content.ReadAsStringAsync().Result;
        //        Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
        //        GarmentDeliveryReturnViewModel viewModel = JsonConvert.DeserializeObject<GarmentDeliveryReturnViewModel>(result.GetValueOrDefault("data").ToString());

        //        return viewModel;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        #region Function for Unit DO
        public List<object> ReadForUnitDO(string Keyword = null, string Filter = "{}")
        {
            var Query = dbSet.Where(x => x.IsDeleted == false);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            long unitId = 0;
            long storageId = 0;
            long UrnItemsId = 0;
            bool hasUnitFilter = FilterDictionary.ContainsKey("UnitId") && long.TryParse(FilterDictionary["UnitId"], out unitId);
            bool hasStorageFilter = FilterDictionary.ContainsKey("StorageId") && long.TryParse(FilterDictionary["StorageId"], out storageId);
            bool hasRONoFilter = FilterDictionary.ContainsKey("RONo");
            bool hasPOSerialNumberFilter = FilterDictionary.ContainsKey("POSerialNumber");
            string RONo = hasRONoFilter ? (FilterDictionary["RONo"] ?? "").Trim() : "";
            string POSerialNumber = hasPOSerialNumberFilter ? (FilterDictionary["POSerialNumber"] ?? "").Trim() : "";
            bool hasUrnItemIdFilter = FilterDictionary.ContainsKey("URNItemId") && long.TryParse(FilterDictionary["URNItemId"], out UrnItemsId);


            if (hasUnitFilter)
            {
                Query = Query.Where(x => x.UnitId == unitId);
            }
            if (hasRONoFilter)
            {
                Query = Query.Where(x => x.RONo == RONo);
            }
            
            //if (hasPOSerialNumberFilter)
            //{
            //    Query = Query.Where(x => x.Items.Any(s => s.POSerialNumber == POSerialNumber));
            //}

            var data = hasUrnItemIdFilter ? from a in Query
                                           join b in dbSetGarmentUnitReceiptNoteItem on a.Id equals b.URNId
                                           where b.RemainingQuantity > 0 && b.IsDeleted == false && b.Id == UrnItemsId
                                            select new
                                           {
                                               URNId = a.Id,
                                               URNItemId = b.Id,
                                               a.URNNo,
                                               b.ProductId,
                                               b.ProductCode,
                                               b.ProductName,
                                               b.SmallQuantity,
                                               b.SmallUomId,
                                               b.SmallUomUnit,
                                               b.DesignColor,
                                               b.POSerialNumber,
                                               b.RemainingQuantity,
                                               a.RONo,
                                               a.Article,
                                               b.ProductRemark,
                                               b.PricePerDealUnit,
                                               b.ReceiptCorrection,
                                               b.CorrectionConversion,
                                               a.BeacukaiNo,
                                               a.BeacukaiType,
                                               a.BeacukaiDate,
                                               b.DOItemId,
                                            } : from a in Query
                       join b in dbSetGarmentUnitReceiptNoteItem on a.Id equals b.URNId
                       where b.RemainingQuantity > 0 && b.StorageId == (hasStorageFilter != null ? storageId : b.StorageId) && b.IsDeleted== false
                       select new
                       {
                           URNId = a.Id,
                           URNItemId = b.Id,
                           a.URNNo,
                           b.ProductId,
                           b.ProductCode,
                           b.ProductName,
                           b.SmallQuantity,
                           b.SmallUomId,
                           b.SmallUomUnit,
                           b.DesignColor,
                           b.POSerialNumber,
                           b.RemainingQuantity,
                           a.RONo,
                           a.Article,
                           b.ProductRemark,
                           b.PricePerDealUnit,
                           b.ReceiptCorrection,
                           b.CorrectionConversion,
                           a.BeacukaiNo,
                           a.BeacukaiType,
                           a.BeacukaiDate,
                           b.DOItemId,
                       };
            List<object> ListData = new List<object>(data);
            return ListData;
        }

        public List<object> ReadForUnitDOMore(string Keyword = null, string Filter = "{}", int size = 50)
        {
            var Query = dbSet.Where(x => x.IsDeleted == false);

            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            long unitId = 0;
            long storageId = 0;
            bool hasUnitFilter = FilterDictionary.ContainsKey("UnitId") && long.TryParse(FilterDictionary["UnitId"], out unitId);
            bool hasStorageFilter = FilterDictionary.ContainsKey("StorageId") && long.TryParse(FilterDictionary["StorageId"], out storageId);
            bool hasRONoFilter = FilterDictionary.ContainsKey("RONo");
            bool hasPOSerialNumberFilter = FilterDictionary.ContainsKey("POSerialNumber");
            string RONo = hasRONoFilter ? (FilterDictionary["RONo"] ?? "").Trim() : "";
            string POSerialNumber = hasPOSerialNumberFilter ? (FilterDictionary["POSerialNumber"] ?? "").Trim() : "";

            if (hasUnitFilter)
            {
                Query = Query.Where(x => x.UnitId == unitId);
            }
            if (hasRONoFilter)
            {
                Query = Query.Where(x => x.RONo != RONo);
            }

            Keyword = (Keyword ?? "").Trim();
            var data = from a in Query
                       join b in dbSetGarmentUnitReceiptNoteItem on a.Id equals b.URNId
                       where b.RemainingQuantity > 0 && b.StorageId == (hasStorageFilter != null ? storageId : b.StorageId) && b.IsDeleted == false
                       && (a.RONo.Contains(Keyword) || b.POSerialNumber.Contains(Keyword))
                       select new
                       {
                           URNItemId = b.Id,
                           a.RONo,
                           b.ProductName,
                           b.ProductCode,
                           b.POSerialNumber,
                           b.RemainingQuantity,
                       };
    
            List<object> ListData = new List<object>(data.OrderBy(o => o.RONo).Take(size));
            return ListData;
        }

        #endregion
    }
}
