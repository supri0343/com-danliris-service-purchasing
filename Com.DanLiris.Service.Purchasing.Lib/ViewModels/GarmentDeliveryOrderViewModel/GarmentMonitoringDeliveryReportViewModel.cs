using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel
{
   public class GarmentMonitoringDeliveryReportViewModel
    {
        public string epoNo { get; set; }
        public DateTimeOffset orderDate { get; set; }
        public DateTimeOffset deliveryDate { get; set; }        
        public string createdBy { get; set; }
        public string supplierCode { get; set; }
        public string supplierName { get; set; }
        public string prNo { get; set; }
        public string poNo { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public string productRemark { get; set; }
        public string currencyCode { get; set; }
        public string paymentType { get; set; }
        public string category { get; set; }
        public double dealQuantity { get; set; }
        public string uomUnit { get; set; }
        public double doEpoQuantity { get; set; }
        public double epoPrice { get; set; }
        public string doNo { get; set; }
        public string doDate { get; set; }
        public string arrivalDate { get; set; }
        public double doQuantity { get; set; }
        public int statusDO { get; set; }
        public int diffDay { get; set; }
        public int sevenddaybefore { get; set; }
        public string flagData { get; set; }
    }
}
