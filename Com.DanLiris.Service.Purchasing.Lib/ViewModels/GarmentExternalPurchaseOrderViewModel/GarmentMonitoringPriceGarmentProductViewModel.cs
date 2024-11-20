using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentExternalPurchaseOrderViewModel
{
   public class GarmentMonitoringPriceGarmentProductViewModel
    {
        public string epoNo { get; set; }
        public DateTimeOffset orderDate { get; set; }
        public string supplierCode { get; set; }
        public string supplierName { get; set; }
        public string prNo { get; set; }
        public string poNo { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public string productRemark { get; set; }

        public string invoiceNo { get; set; }
        public DateTimeOffset invoiceDate { get; set; }
        public string inNo { get; set; }
        public DateTimeOffset inDate { get; set; }

        public double epoQuantity { get; set; }
        public string uomunit { get; set; }
        public string currencyCode { get; set; }

        public double epoprice { get; set; }

        public double? rate { get; set; }
        public double? amount { get; set; }
      
    }
}
