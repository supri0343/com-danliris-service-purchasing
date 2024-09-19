using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports
{
    public class BeacukaiNoFeatureViewModel
    {
        public string BCNo { get; set; }
        public string BCType { get; set; }
        public DateTime BCDate { get; set; }
        public string DONo { get; set; }
        public string PO { get; set; }
        public string ProductCode { get; set; }
        public string RONo { get; set; }
        public string Composition { get; set; }
        public string Construction { get; set; }
        public double QtyBC { get; set; }

        //Enhance 19-09-2024 
        public string SupplierName { get; set; }
        public double Bruto { get; set; }
        public double Netto { get; set; }
        public string URNNo { get; set; }
        public DateTime? URNDate { get; set; }
        public string UENNo { get; set; }
        public DateTime? UENDate { get; set; }
        public double QtyUEN { get; set; }
        public string UENType { get; set; }
        public long UnitDOItemId { get; set; }
    }
}
