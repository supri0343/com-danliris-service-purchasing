using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels.DOItems
{
    public class DOItemsViewModels
    {
        public long Id { get; set; }
        public string? POSerialNumber { get; set; }
        public string? RO { get; set; }
        public string? UnitName { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public decimal RemainingQuantity { get; set; }
        public string? SmallUomUnit { get; set; }
        public string? Colour { get; set; }
        public string? Rack { get; set; }
        public string? Level { get; set; }
        public string? Box { get; set; }
        public string? Area { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModyfiedBy { get; set; }
    }
}
