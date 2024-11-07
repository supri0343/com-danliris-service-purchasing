using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderNonPOViewModel
{
   public class GarmentDeliveryOrderReportViewModel
    {
        public string no { get; set; }
        public DateTimeOffset doDate { get; set; } // DODate
        public DateTimeOffset arrivalDate { get; set; } // ArrivalDate
        public string roNo { get; set; }
        public string poNo { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public double dOQuantity { get; set; }
        public string doUom { get; set; }

        public string URNNo { get; set; }
        public DateTimeOffset URNDate { get; set; }
        public decimal urnQuantity { get; set; }
        public string urnUom { get; set; }

        public string UDONo { get; set; }
        public DateTimeOffset UDODate { get; set; }
        public double udoQuantity { get; set; }
        public string udoUom { get; set; }

        public string UENNo { get; set; }
        public DateTimeOffset UENDate { get; set; }
        public double uenQuantity { get; set; }
        public string uenUom { get; set; }
        public string BeacukaiNo { get; set; }
        public string BeacukaiType { get; set; }
        public DateTimeOffset BeacukaiDate { get; set; }

    }
}
