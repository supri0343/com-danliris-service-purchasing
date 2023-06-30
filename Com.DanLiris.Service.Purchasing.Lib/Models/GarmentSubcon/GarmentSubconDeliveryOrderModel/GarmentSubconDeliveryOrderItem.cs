using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubconDeliveryOrderModel
{
    public class GarmentSubconDeliveryOrderItem : StandardEntity<long>
    {
        public string POSerialNumber { get; set; }
        [MaxLength(100)]
        public long ProductId { get; set; }
        [MaxLength(100)]
        public string ProductCode { get; set; }
        [MaxLength(1000)]
        public string ProductName { get; set; }
        public string ProductRemark { get; set; }
        [MaxLength(100)]
        public string UomId { get; set; }
        [MaxLength(100)]
        public string UomUnit { get; set; }
        public double BudgetQuantity { get; set; }
        public double DOQuantity { get; set; }
        public double PricePerDealUnit { get; set; }
        public string CurrencyCode { get; set; }
        public virtual long GarmentDOId { get; set; }
        [ForeignKey("GarmentDOId")]
        public virtual GarmentSubconDeliveryOrder GarmentSubconDeliveryOrder { get; set; }
    }
}
