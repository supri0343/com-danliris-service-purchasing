using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System.Net.Http;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Newtonsoft.Json;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class BeacukaiNoFeature : IBeacukaiNoFeature
    {
        private readonly PurchasingDbContext dbContext;
        public readonly IServiceProvider serviceProvider;
        private readonly DbSet<GarmentDeliveryOrder> dbSet;

        public BeacukaiNoFeature(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<GarmentDeliveryOrder>();
        }

        public Tuple<List<BeacukaiNoFeatureViewModel>, int> GetBeacukaiNoReport(string filter, string keyword)
        {
            //var Query = GetStockQuery(tipebarang, unitcode, dateFrom, dateTo, offset);
            //Query = Query.OrderByDescending(x => x.SupplierName).ThenBy(x => x.Dono);
            List<BeacukaiNoFeatureViewModel> Data = GetBeacukaiNo(filter, keyword);

           // List<BeacukaiNoFeatureViewModel> Data= GetDeletHistory(string filter, string keyword);

            //Data = Data.OrderByDescending(x => x.KodeBarang).ToList();
            //int TotalData = Data.Count();
            return Tuple.Create(Data.OrderByDescending(x=>x.PO).ToList(), Data.OrderByDescending(x => x.PO).Count());
        }

        public List<BeacukaiNoFeatureViewModel> GetBeacukaiNo(string filter, string keyword)
        {
            var Query = filter == "BCNo" ? from a in dbContext.GarmentBeacukais
                                           join b in dbContext.GarmentBeacukaiItems on a.Id equals b.BeacukaiId
                                           join c in dbContext.GarmentDeliveryOrders on b.GarmentDOId equals c.Id
                                           join d in dbContext.GarmentDeliveryOrderItems on c.Id equals d.GarmentDOId
                                           join e in dbContext.GarmentDeliveryOrderDetails on d.Id equals e.GarmentDOItemId
                                           //add join to URNItem
                                           join u in dbContext.GarmentUnitReceiptNoteItems on e.Id equals u.DODetailId
                                           join uh in dbContext.GarmentUnitReceiptNotes on u.URNId equals uh.Id
                                           //disable Join with PO to UnitDo
                                           //join f in dbContext.GarmentUnitDeliveryOrderItems on e.POSerialNumber equals f.POSerialNumber
                                           join f in dbContext.GarmentUnitDeliveryOrderItems on u.Id equals f.URNItemId
                                           join g in dbContext.GarmentUnitDeliveryOrders on f.UnitDOId equals g.Id
                                           //add left join with UEN
                                           join ue in dbContext.GarmentUnitExpenditureNoteItems on f.Id equals ue.UnitDOItemId into uenGroup
                                           from ue in uenGroup.DefaultIfEmpty()
                                           join uei in dbContext.GarmentUnitExpenditureNotes on ue.UENId equals uei.Id into ueiGroup
                                           from uei in ueiGroup.DefaultIfEmpty()
                                           where a.BeacukaiNo == keyword
                                           //&& a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false && d.IsDeleted == false
                                           //&& e.IsDeleted == false
                                           select new BeacukaiNoFeatureViewModel
                                           {
                                               BCType = a.CustomsType,
                                               BCDate = a.BeacukaiDate.DateTime,
                                               ProductCode = f.ProductCode,
                                               PO = f.POSerialNumber,
                                               BCNo = a.BeacukaiNo,
                                               DONo = c.DONo,
                                               QtyBC = (double)(u.ReceiptQuantity * u.Conversion),
                                               RONo = g.RONo,
                                               //Enhance 19-09-2024
                                               Bruto = a.Bruto,
                                               Netto = a.Netto,
                                               URNNo = uh.URNNo,
                                               URNDate = uh.ReceiptDate.AddHours(7).Date,
                                               UENNo = uei != null ? uei.UENNo : null,
                                               UENDate = uei != null ? uei.ExpenditureDate.AddHours(7).Date : (DateTime?)null,
                                               QtyUEN = ue != null ? ue.Quantity : 0,
                                               UENType = uei != null ? uei.ExpenditureType : null,
                                               UnitDOItemId = f.Id,
                                               SupplierName = c.SupplierName,
                                           }
            : filter == "PONo" ? from a in dbContext.GarmentBeacukais
                                 join b in dbContext.GarmentBeacukaiItems on a.Id equals b.BeacukaiId
                                 join c in dbContext.GarmentDeliveryOrders on b.GarmentDOId equals c.Id
                                 join d in dbContext.GarmentDeliveryOrderItems on c.Id equals d.GarmentDOId
                                 join e in dbContext.GarmentDeliveryOrderDetails on d.Id equals e.GarmentDOItemId
                                 //add join to URNItem
                                 join u in dbContext.GarmentUnitReceiptNoteItems on e.Id equals u.DODetailId
                                 join uh in dbContext.GarmentUnitReceiptNotes on u.URNId equals uh.Id
                                 //disable Join with PO to UnitDo
                                 //join f in dbContext.GarmentUnitDeliveryOrderItems on e.POSerialNumber equals f.POSerialNumber
                                 join f in dbContext.GarmentUnitDeliveryOrderItems on u.Id equals f.URNItemId
                                 join g in dbContext.GarmentUnitDeliveryOrders on f.UnitDOId equals g.Id
                                 //add left join with UEN
                                 join ue in dbContext.GarmentUnitExpenditureNoteItems on f.Id equals ue.UnitDOItemId into uenGroup
                                 from ue in uenGroup.DefaultIfEmpty()
                                 join uei in dbContext.GarmentUnitExpenditureNotes on ue.UENId equals uei.Id into ueiGroup
                                 from uei in ueiGroup.DefaultIfEmpty()
                                 where f.POSerialNumber == keyword
                                 //&& a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false && d.IsDeleted == false
                                 //&& e.IsDeleted == false && f.IsDeleted==false &&g.IsDeleted==false && u.IsDeleted==false 
                                 select new BeacukaiNoFeatureViewModel
                                 {
                                     BCType = a.CustomsType,
                                     BCDate = a.BeacukaiDate.DateTime,
                                     ProductCode = f.ProductCode,
                                     PO = f.POSerialNumber,
                                     BCNo = a.BeacukaiNo,
                                     DONo = c.DONo,
                                     QtyBC = (double)(u.ReceiptQuantity * u.Conversion),
                                     RONo = g.RONo,
                                     //Enhance 19-09-2024
                                     Bruto = a.Bruto,
                                     Netto = a.Netto,
                                     URNNo = uh.URNNo,
                                     URNDate = uh.ReceiptDate.AddHours(7).Date,
                                     UENNo = uei != null ? uei.UENNo : null,
                                     UENDate = uei != null ? uei.ExpenditureDate.AddHours(7).Date : (DateTime?)null,
                                     QtyUEN = ue != null ? ue.Quantity : 0,
                                     UENType = uei != null ? uei.ExpenditureType : null,
                                     UnitDOItemId = f.Id,
                                     SupplierName = c.SupplierName,
                                 } :
                                 from a in dbContext.GarmentBeacukais
                                 join b in dbContext.GarmentBeacukaiItems on a.Id equals b.BeacukaiId
                                 join c in dbContext.GarmentDeliveryOrders on b.GarmentDOId equals c.Id
                                 join d in dbContext.GarmentDeliveryOrderItems on c.Id equals d.GarmentDOId
                                 join e in dbContext.GarmentDeliveryOrderDetails on d.Id equals e.GarmentDOItemId
                                 //add join to URNItem
                                 join u in dbContext.GarmentUnitReceiptNoteItems on e.Id equals u.DODetailId
                                 join uh in dbContext.GarmentUnitReceiptNotes on u.URNId equals uh.Id
                                 //disable Join with PO to UnitDo
                                 //join f in dbContext.GarmentUnitDeliveryOrderItems on e.POSerialNumber equals f.POSerialNumber
                                 join f in dbContext.GarmentUnitDeliveryOrderItems on u.Id equals f.URNItemId
                                 join g in dbContext.GarmentUnitDeliveryOrders on f.UnitDOId equals g.Id
                                 //add left join with UEN
                                 join ue in dbContext.GarmentUnitExpenditureNoteItems on f.Id equals ue.UnitDOItemId into uenGroup
                                 from ue in uenGroup.DefaultIfEmpty()
                                 join uei in dbContext.GarmentUnitExpenditureNotes on ue.UENId equals uei.Id into ueiGroup
                                 from uei in ueiGroup.DefaultIfEmpty()
                                 where g.RONo == keyword
                                 //&& a.IsDeleted == false && b.IsDeleted == false && c.IsDeleted == false && d.IsDeleted == false
                                 //&& e.IsDeleted == false
                                 select new BeacukaiNoFeatureViewModel
                                 {
                                     BCType = a.CustomsType,
                                     BCDate = a.BeacukaiDate.DateTime,
                                     ProductCode = f.ProductCode,
                                     PO = f.POSerialNumber,
                                     BCNo = a.BeacukaiNo,
                                     DONo = c.DONo,
                                     QtyBC = (double)(u.ReceiptQuantity * u.Conversion),
                                     RONo = g.RONo,
                                     //Enhance 19-09-2024
                                     Bruto = a.Bruto,
                                     Netto = a.Netto,
                                     URNNo = uh.URNNo,
                                     URNDate = uh.ReceiptDate.AddHours(7).Date,
                                     UENNo = uei != null ? uei.UENNo : null,
                                     UENDate = uei != null ? uei.ExpenditureDate.AddHours(7).Date : (DateTime?)null,
                                     QtyUEN = ue != null ? ue.Quantity : 0,
                                     UENType = uei != null ? uei.ExpenditureType : null,
                                     UnitDOItemId = f.Id,
                                     SupplierName = c.SupplierName,
                                 };

            var ProductCode = string.Join(",", Query.Select(x => x.ProductCode).Distinct().ToList());

            var Code = GetProductCode(ProductCode);

            Query = Query.Distinct().GroupBy(x => new { x.BCType, x.BCDate, x.ProductCode, x.PO, x.BCNo, x.DONo, x.RONo, x.Bruto,x.Netto,x.URNNo,x.URNDate,x.UENNo,x.UENDate,x.UENType,x.SupplierName }, (key, group) => new BeacukaiNoFeatureViewModel
            {
                BCType = key.BCType,
                BCDate = key.BCDate,
                ProductCode = key.ProductCode,
                PO = key.PO,
                BCNo = key.BCNo,
                DONo = key.DONo,
                QtyBC = group.Sum(x=>x.QtyBC),
                RONo = key.RONo,

                //Enhance 19-09-2024
                Bruto = key.Bruto,
                Netto = key.Netto,
                URNNo = key.URNNo,
                URNDate = key.URNDate,
                UENNo = key.UENNo,
                UENDate = key.UENDate,
                QtyUEN = group.Sum(x => x.QtyUEN),
                UENType = key.UENType,
                SupplierName = key.SupplierName,
            });

            var Query2 = from a in Query
                         join b in Code on a.ProductCode equals b.Code into Codes
                         from code in Codes.DefaultIfEmpty()
                         select new BeacukaiNoFeatureViewModel
                         {
                             BCType = a.BCType,
                             BCDate = a.BCDate,
                             ProductCode = a.ProductCode,
                             PO = a.PO,
                             DONo = a.DONo,
                             QtyBC = Math.Round(a.QtyBC,2),
                             Composition = code != null ? code.Composition : "-",
                             Construction = code != null ? code.Const : "-",
                             BCNo = a.BCNo,
                             RONo = a.RONo,

                             //Enhance 19-09-2024
                             Bruto = a.Bruto,
                             Netto = a.Netto,
                             URNNo = a.URNNo,
                             URNDate = a.URNDate,
                             UENNo = a.UENNo,
                             UENDate = a.UENDate,
                             QtyUEN = a.QtyUEN,
                             UENType = a.UENType,
                             SupplierName = a.SupplierName
                         };

            return Query2.ToList();
        }

        private List<GarmentProductViewModel> GetProductCode(string codes)
        {
            IHttpClientService httpClient = (IHttpClientService)this.serviceProvider.GetService(typeof(IHttpClientService));

            var httpContent = new StringContent(JsonConvert.SerializeObject(codes), Encoding.UTF8, "application/json");

            var garmentProductionUri = APIEndpoint.Core + $"master/garmentProducts/byCodes";
            var httpResponse = httpClient.SendAsync(HttpMethod.Get, garmentProductionUri, httpContent).Result;

            List<GarmentProductViewModel> viewModel = new List<GarmentProductViewModel>();

            if (httpResponse.IsSuccessStatusCode)
            {
                var content = httpResponse.Content.ReadAsStringAsync().Result;
                Dictionary<string, object> result = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);

                viewModel = JsonConvert.DeserializeObject<List<GarmentProductViewModel>>(result.GetValueOrDefault("data").ToString());

            }

            return viewModel;

        }
    }
}
