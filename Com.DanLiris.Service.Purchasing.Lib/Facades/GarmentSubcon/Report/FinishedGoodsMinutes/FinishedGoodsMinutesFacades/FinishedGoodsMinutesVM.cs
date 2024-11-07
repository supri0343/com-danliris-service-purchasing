using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSubcon.Report.FinishedGoodsMinutes.FinishedGoodsMinutesFacades
{
    public class FinishedGoodsMinutesVM
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public double UsedQty { get; set; }
        public string UsedUomUnit { get; set; }
        public string DONo { get; set; }
        public string SupplierName { get; set; }
        public double ReceiptQty { get; set; }
        public string ReceiptUomUnit { get; set; }
        public string ReceiptBCNo { get; set; }
        public string ReceiptBCType { get; set; }
        public DateTimeOffset ReceiptBCDate { get; set; }
        public string RONo { get; set; }
        public double PricePerDeal { get; set; }
    }
}
