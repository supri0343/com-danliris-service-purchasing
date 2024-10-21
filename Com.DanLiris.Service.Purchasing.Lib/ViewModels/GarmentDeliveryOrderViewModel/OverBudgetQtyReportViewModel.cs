using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel
{
   public class OverBudgetQtyReportViewModel
    {
        public string epoNo { get; set; }
        public DateTimeOffset orderDate { get; set; }
        public DateTimeOffset createdDate { get; set; }
        public string supplierCode { get; set; }
        public string supplierName { get; set; }
        public string poNo { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public string codeRequirement { get; set; }
        public double poQuantity { get; set; }
        public double doQuantity { get; set; }
        public double accmltvQuantity { get; set; }
        public double obQuantity { get; set; }
        public double percentOB { get; set; }
        public string doNo { get; set; }
        public DateTimeOffset doDate { get; set; }
        public string invoiceNo { get; set; }
        public DateTimeOffset invoiceDate { get; set; }
        public string inNo { get; set; }
        public DateTimeOffset inDate { get; set; }
        public string remark { get; set; }
    }

}
