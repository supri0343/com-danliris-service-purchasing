using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.MonitoringUnitReceiptAllViewModel
{
   public  class MonitoringUnitReceiptAllDeleted
    {
        public long id { get; set; }
        public string? no { get; set; }
        public string? dateBon { get; set; } // Menggunakan DateTime untuk tanggal
        public string? unit { get; set; }
        public string? doNo { get; set; }
        public string? urnType { get; set; }
        public string? deletedUtc { get; set; } // Menggunakan DateTime untuk tanggal dan waktu
        public string? deletedBy { get; set; }
        public string? supplier { get; set; } // Mengganti menjadi supplierName
        public string? suplayerCd { get; set; } // Mengganti menjadi supplierCode
        public string? uenNo { get; set; }
        public string? drnNo { get; set; }


        public string? roNo { get; set; }
        public string? POSrNo { get; set; }
        public string? PdName { get; set; }
        public decimal RQuantity { get; set; }
        public string? UmUnt { get; set; }
    }
}
