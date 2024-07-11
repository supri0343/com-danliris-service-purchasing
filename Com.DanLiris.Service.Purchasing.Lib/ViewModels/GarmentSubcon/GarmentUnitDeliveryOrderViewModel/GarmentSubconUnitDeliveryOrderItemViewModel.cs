﻿using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentUnitDeliveryOrderViewModel
{
    public class GarmentSubconUnitDeliveryOrderItemViewModel : BaseViewModel
    {
        public long URNId { get; set; }
        public string URNNo { get; set; }

        public long URNItemId { get; set; }
        public long DOItemId { get; set; }
        public long EPOItemId { get; set; }
        public long POItemId { get; set; }
        public long PRItemId { get; set; }
        public string POSerialNumber { get; set; }

        /*Product*/
        public long ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductRemark { get; set; }

        public string RONo { get; set; }
        public double Quantity { get; set; }
        public double DefaultDOQuantity { get; set; }

        /*UOM*/
        public long UomId { get; set; }
        public string UomUnit { get; set; }

        public double PricePerDealUnit { get; set; }
        public string FabricType { get; set; }

        public string DesignColor { get; set; }

        public CurrencyViewModel DOCurrency { get; set; }

        public bool IsSave { get; set; }

        /*RETUR*/
        public double? ReturQuantity { get; set; }
        public string ReturUomUnit { get; set; }
        public long? ReturUomId { get; set; }
        public double ReturQtyCheck { get; set; }

        //public int DOItemsId { get; set; }
        public string BeacukaiNo { get; set; }
        public DateTimeOffset BeacukaiDate { get; set; }
        public string BeacukaiType { get; set; }

        public string Colour { get; set; }
        public string Rack { get; set; }
        public string Level { get; set; }
        public string Box { get; set; }
        public string Area { get; set; }
    }
}
