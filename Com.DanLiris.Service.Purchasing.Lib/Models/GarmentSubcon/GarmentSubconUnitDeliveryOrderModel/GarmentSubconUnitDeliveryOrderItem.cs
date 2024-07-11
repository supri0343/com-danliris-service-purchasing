﻿using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentUnitDeliveryOrderModel
{
    public class GarmentSubconUnitDeliveryOrderItem : StandardEntity<long>
	{
        [MaxLength(255)]
        public string? UId { get; set; }
        public long URNId { get; set; }
        [MaxLength(255)]
        public string? URNNo { get; set; }

        public long URNItemId { get; set; }
        public long DOItemId { get; set; }
        public long EPOItemId { get; set; }
        public long POItemId { get; set; }
        public long PRItemId { get; set; }
        [MaxLength(255)]
        public string? POSerialNumber { get; set; }

        /*Product*/
        public long ProductId { get; set; }
        [MaxLength(255)]
        public string? ProductCode { get; set; }
        [MaxLength(1000)]
        public string? ProductName { get; set; }
        [MaxLength(1000)]
        public string? ProductRemark { get; set; }

        [MaxLength(255)]
        public string? RONo { get; set; }
        public double Quantity { get; set; }
        public double DefaultDOQuantity { get; set; }

        /*UOM*/
        public long UomId { get; set; }
        [MaxLength(255)]
        public string? UomUnit { get; set; }

        public double PricePerDealUnit { get; set; }
        [MaxLength(255)]
        public string? FabricType { get; set; }
        [MaxLength(1000)]
        public string? DesignColor { get; set; }

        public double? DOCurrencyRate { get; set; }

        /*RETUR*/
        public double ReturQuantity { get; set; }
        [MaxLength(255)]
        public string? ReturUomUnit { get; set; }
        public long? ReturUomId { get; set; }
        public string? BeacukaiNo { get; set; }
        public DateTimeOffset BeacukaiDate { get; set; }
        public string? BeacukaiType { get; set; }


        [NotMapped]
        public bool IsSave { get; set; }

        public virtual long UnitDOId { get; set; }
        [ForeignKey("UnitDOId")]
        public virtual GarmentSubconUnitDeliveryOrder GarmentUnitDeliveryOrder { get; set; }
    }
}


