using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentSubconDeliveryOrderViewModel
{
    public class GarmentSubconDeliveryOrderItemViewModel : BaseViewModel
    {
        public GarmentProductViewModel Product { get; set; }
        public UomViewModel Uom { get; set; }
        public string POSerialNumber { get; set; }
        public double BudgetQuantity { get; set; }
        public double DOQuantity { get; set; }
        public double PricePerDealUnit { get; set; }
        public string CurrencyCode { get; set; }
    }
}
