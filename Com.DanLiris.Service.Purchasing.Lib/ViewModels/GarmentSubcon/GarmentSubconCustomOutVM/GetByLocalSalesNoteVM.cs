using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentSubconCustomOutVM
{
    public class GetByLocalSalesNoteVM
    {
        public string LocalSalesNote { get; set; }
        public string BCNo { get; set; }
        public string BCType { get; set; }
        public DateTimeOffset BCDate { get; set; }
    }
}
