using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentSubconCustomOutVM
{
    public class GarmentSubconCustomOutDetailVM
    {
        public ProductViewModel product { get; set; }
       
        public double quantity { get; set; }
        public UomViewModel uom { get; set; }
    }
}
