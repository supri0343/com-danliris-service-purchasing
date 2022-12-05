using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels.DOItems
{
    public class StellingViewModels
    {
        public string POSerialNumber { get; set; }
        public decimal? Quantity { get; set; }
        public string Uom { get; set; }
        public string Colour { get; set; }
        public string Rack { get; set; }
        public string Level { get; set; }
        public string Box { get; set; }
        public string Area { get; set; }

        public DateTime? ReceiptDate { get; set; }
        public decimal? QuantityReceipt { get; set; }
        public DateTime? ExpenditureDate { get; set; }
        public double? QtyExpenditure { get; set; }
        public double? Remaining { get; set; }
        public string Remark { get; set; }
        public string User { get; set; }

    }

    public class StellingEndViewModels
    {
        public string POSerialNumber { get; set; }
        public decimal? Quantity { get; set; }
        public string Uom { get; set; }
        public string Colour { get; set; }
        public string Rack { get; set; }
        public string Level { get; set; }
        public string Box { get; set; }
        public string Area { get; set; }

        public string ReceiptDate { get; set; }
        public decimal? QuantityReceipt { get; set; }
        public string ExpenditureDate { get; set; }
        public double? QtyExpenditure { get; set; }
        public double? Remaining { get; set; }
        public string Remark { get; set; }
        public string User { get; set; }

    }
}
