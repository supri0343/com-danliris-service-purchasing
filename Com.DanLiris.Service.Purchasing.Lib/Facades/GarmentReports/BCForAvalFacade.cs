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
            public string bcno { get; internal set; }
            public DateTime bcdate { get; internal set; }
            public long uenitemId { get; internal set; }
            public string poSerialNumber { get; internal set; }
            public string bctype { get; internal set; }
        }

        public List<ViewModel> GetQuery(string buk)
        {
            var listBuk = buk.Split(",").ToList();

            List<long> listItemId = listBuk.Select(x => long.Parse(x)).ToList();

            var Query = (from b in (from aa in dbContext.GarmentUnitExpenditureNoteItems
                                    where aa.IsDeleted == false && listItemId.Contains(aa.Id)
                                    select aa)
                         //join b in dbContext.GarmentUnitExpenditureNoteItems on a.Id equals b.UENId
                         join c in dbContext.GarmentDeliveryOrderDetails on b.DODetailId equals c.Id
                         join d in dbContext.GarmentDeliveryOrderItems on c.GarmentDOItemId equals d.Id
                         join e in dbContext.GarmentDeliveryOrders on d.GarmentDOId equals e.Id
                         join f in dbContext.GarmentBeacukaiItems on e.Id equals f.GarmentDOId
                         join g in dbContext.GarmentBeacukais on f.BeacukaiId equals g.Id
                         where  c.IsDeleted == false &&
                         d.IsDeleted == false && e.IsDeleted == false && f.IsDeleted == false && g.IsDeleted == false
                         select new ViewModel
                         {
                             bcno = g.BeacukaiNo,
                             bcdate = g.BeacukaiDate.Date,
                             uenitemId = b.Id,
                             poSerialNumber = b.POSerialNumber,
                             bctype = g.CustomsType
                         }).Distinct();

            return Query.ToList();
        }
    }
}
