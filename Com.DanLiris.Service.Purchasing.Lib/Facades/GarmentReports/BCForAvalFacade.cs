using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class BCForAvalFacade : IBCForAval
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;

        public BCForAvalFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
        }
        public class ViewModel
        {
            public string? bcno { get; internal set; }
            public DateTime? bcdate { get; internal set; }
            public long uenitemId { get; internal set; }
            public string? poSerialNumber { get; internal set; }
            public string? bctype { get; internal set; }
        }

        public List<ViewModel> GetQuery(string buk)
        {
            var listBuk = buk.Split(",").ToList();

            List<long> listItemId = listBuk.Select(x => long.Parse(x)).ToList();

            var Query = (from b in (from aa in dbContext.GarmentUnitExpenditureNoteItems
                                    where aa.IsDeleted == false && listItemId.Contains(aa.Id)
                                    select aa)
                         //join b in dbContext.GarmentUnitExpenditureNoteItems on a.Id equals b.UENId
                         join c in dbContext.GarmentDeliveryOrderDetails on b.DODetailId equals c.Id into doDet
                         from doDetail in doDet.DefaultIfEmpty()
                         join d in dbContext.GarmentDeliveryOrderItems on doDetail.GarmentDOItemId equals d.Id into doIte
                         from doItem in doIte.DefaultIfEmpty()
                         join e in dbContext.GarmentDeliveryOrders on doItem.GarmentDOId equals e.Id into doOrd
                         from DoOrder in doOrd.DefaultIfEmpty()
                         join f in dbContext.GarmentBeacukaiItems on DoOrder.Id equals f.GarmentDOId into bcIte
                         from bcItem in bcIte.DefaultIfEmpty()
                         join g in dbContext.GarmentBeacukais on bcItem.BeacukaiId equals g.Id into bc
                         from beacukais in bc.DefaultIfEmpty()
                         where  doDetail.IsDeleted == false &&
                         doItem.IsDeleted == false && DoOrder.IsDeleted == false && bcItem.IsDeleted == false && beacukais.IsDeleted == false
                         select new ViewModel
                         {
                             bcno = beacukais != null ? beacukais.BeacukaiNo : "-",
                             bcdate = beacukais != null ? beacukais.BeacukaiDate.Date : DateTime.MinValue,
                             uenitemId = b.Id,
                             poSerialNumber = b.POSerialNumber,
                             bctype = beacukais != null ? beacukais.CustomsType : "-"
                         }).Distinct();

            return Query.ToList();
        }
    }
}
