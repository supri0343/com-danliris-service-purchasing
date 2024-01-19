using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentSubconCustomOutVM
{
    public class GarmentSubconCustomOutItemVM : BaseViewModel
    {
        //LeftOver
        public long? uenId { get; set; }
        public string uenNo { get; set; }


        //Service
        public long? localSalesNoteId { get; set; }
        public string localSalesNoteNo { get; set; }
        public UomViewModel packageUom { get; set; }
        public double? packageQuantity { get; set; }


        //All
        public UomViewModel uom { get; set; }
        public double? quantity { get; set; }

        public List<GarmentSubconCustomOutDetailVM> details { get; set; }

    }
}
