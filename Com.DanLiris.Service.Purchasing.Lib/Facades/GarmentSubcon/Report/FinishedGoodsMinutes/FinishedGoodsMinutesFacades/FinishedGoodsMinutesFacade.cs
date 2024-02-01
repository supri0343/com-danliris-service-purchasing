using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentUnitExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubconDeliveryOrderModel;
using System.Threading.Tasks;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSubcon.Report.FinishedGoodsMinutes.FinishedGoodsMinutesFacades
{
    public class FinishedGoodsMinutesFacade : IFinishedGoodsMinutesFacade
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;
        private readonly DbSet<GarmentSubconUnitExpenditureNoteItem> uenItem;
        private readonly DbSet<GarmentSubconDeliveryOrder> deliveryOrders;
        private readonly DbSet<GarmentSubconDeliveryOrderItem> deliveryOrderItems;
        public FinishedGoodsMinutesFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.serviceProvider = serviceProvider;
            uenItem = dbContext.Set<GarmentSubconUnitExpenditureNoteItem>();
            deliveryOrderItems = dbContext.Set<GarmentSubconDeliveryOrderItem>();
            deliveryOrders = dbContext.Set<GarmentSubconDeliveryOrder>();
        }

        public async Task<List<FinishedGoodsMinutesVM>> GetByRONo(List<string> Rono)
        {
            var Query = await (from a in uenItem
                         join b in deliveryOrderItems on a.DOItemId equals b.Id
                         join c in deliveryOrders on b.GarmentDOId equals c.Id
                         where a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false
                         && Rono.Contains(a.RONo)
                         select new
                         {
                             RONo = a.RONo,
                             ProductName = a.ProductName,
                             ProductCode = a.ProductCode,
                             UsedQty = a.Quantity,
                             UsedUomUnit = a.UomUnit,
                             DONo = c.DONo,
                             SupplierName = c.ProductOwnerName,
                             ReceiptQty = b.DOQuantity,
                             ReceiptUomUnit = b.UomUnit,
                             ReceiptBCNo = c.BeacukaiNo,
                             ReceiptBCType = c.BeacukaiType,
                             ReceiptBCDate = c.BeacukaiDate
                         }).GroupBy(x => new { x.RONo,x.ProductName,x.ProductCode,x.UsedUomUnit,x.DONo,x.SupplierName,x.ReceiptQty,x.ReceiptUomUnit,x.ReceiptBCNo,x.ReceiptBCType,x.ReceiptBCDate},(key,group) => new FinishedGoodsMinutesVM 
                         {
                             RONo = key.RONo,
                             ProductName = key.ProductName,
                             ProductCode = key.ProductCode,
                             UsedQty = group.Sum(x => x.UsedQty),
                             UsedUomUnit = key.UsedUomUnit,
                             DONo = key.DONo,
                             SupplierName = key.SupplierName,
                             ReceiptQty = key.ReceiptQty,
                             ReceiptUomUnit = key.ReceiptUomUnit,
                             ReceiptBCNo = key.ReceiptBCNo,
                             ReceiptBCType = key.ReceiptBCType,
                             ReceiptBCDate = key.ReceiptBCDate
                         }).ToListAsync();

            return Query;

        }
    }
}
