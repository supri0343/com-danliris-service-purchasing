using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderNonPOViewModel
{
    public class GarmentDeliveryOrderNonPOItemViewModel : BaseViewModel
    {
        public CurrencyViewModel currency { get; set; }
        public UomViewModel uom { get; set; }
        public double PricePerDealUnit { get; set; }
        public double Quantity { get; set; }
        public string? ProductRemark { get; set; }
    }

}
