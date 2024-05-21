﻿using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports
{
    public class BudgetMasterSampleDisplayFacade : IBudgetMasterSampleDisplayFacade
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IdentityService identityService;
        private readonly PurchasingDbContext dbContext;

        public BudgetMasterSampleDisplayFacade(IServiceProvider serviceProvider, PurchasingDbContext dbContext)
        {
            this.serviceProvider = serviceProvider;
            identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));

            this.dbContext = dbContext;
        }

        public IQueryable<BudgetMasterSampleDisplayViewModel> GetDisplayQuery(long prId)

        {
            var Query = (from a in dbContext.GarmentPurchaseRequests
                         join b in dbContext.GarmentPurchaseRequestItems on a.Id equals b.GarmentPRId
                         where a.Id == prId

                         select new BudgetMasterSampleDisplayViewModel
                         {
                             RO_Number = a.RONo,
                             DeliveryDate = a.ShipmentDate,
                             BuyerCode = a.BuyerCode,
                             BuyerName = a.BuyerName,
                             Article = a.Article,
                             ProductCode = b.ProductCode,
                             Remark = b.ProductRemark,
                             Quantity = b.Quantity * b.PriceConversion,
                             Uom = b.PriceUomUnit,
                             Price = b.BudgetPrice,
                             POSerialNumber = b.PO_SerialNumber
                         });
            return Query;
        }

        public Tuple<List<BudgetMasterSampleDisplayViewModel>, int> GetMonitoring(long prId, string Order)

        {
            var Query = GetDisplayQuery(prId);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderBy(b => b.POSerialNumber);
            }
            else
            {
                string Key = OrderDictionary.Keys.First();
                string OrderType = OrderDictionary[Key];
            }

            var data = Query.ToList();
            int TotalData = data.Count();

            return Tuple.Create(data, TotalData);
        }

        public MemoryStream GenerateExcel(long prId)
        {
            var Query = GetDisplayQuery(prId);
            Query = Query.OrderBy(b => b.POSerialNumber);
            DataTable result = new DataTable();
            var offset = 7;
            //var garmentpurchaseRequest = dbContext.GarmentPurchaseRequests
            //    .Where(w => w.Id == prId)
            //    .Select(s => new {
            //        s.RONo,
            //        s.PRType
            //    })
            //    .Single();

            result.Columns.Add(new DataColumn() { ColumnName = "Urut", DataType = typeof(int) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor RO", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Buyer", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nama Buyer", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Artikel", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tgl Shipment", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Kode Barang", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Keterangan", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Quantity", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Satuan Barang", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Harga Satuan", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Jumlah Harga", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "Nomor PO", DataType = typeof(string) });

            //if (Query.ToArray().Count() != 0)
            //    result.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", ""); // to allow column name to be generated properly for empty data as template
            //else
            //{
                var index = 0;
                foreach (var item in Query)
                {
                    index++;

                    string ShipDate = item.DeliveryDate == null ? "-" : item.DeliveryDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));
                    string QtyBgt = string.Format("{0:N2}", item.Quantity);
                    string BgtPrc = string.Format("{0:N2}", item.Price);
                    string BgtAmt = string.Format("{0:N2}", item.Quantity * item.Price);

                    result.Rows.Add(index, item.RO_Number, item.BuyerCode, item.BuyerName, item.Article, ShipDate, item.ProductCode, item.Remark, QtyBgt, item.Uom, BgtPrc, BgtAmt, item.POSerialNumber);
                }
            //}
            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Display RO Master") }, true);
        }
    }

    public interface IBudgetMasterSampleDisplayFacade
    {
        Tuple<List<BudgetMasterSampleDisplayViewModel>, int> GetMonitoring(long prId, string Order);
        MemoryStream GenerateExcel(long prId);
    }
}
