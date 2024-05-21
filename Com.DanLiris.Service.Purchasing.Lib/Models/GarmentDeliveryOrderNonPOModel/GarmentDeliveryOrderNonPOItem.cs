using Com.Moonlay.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderNonPOModel
{
    public class GarmentDeliveryOrderNonPOItem : StandardEntity<long>
    {
        public long CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
		public string? UId { get; set; }

        public virtual long GarmentDeliveryOrderNonPOId { get; set; }
        [ForeignKey("GarmentDeliveryOrderNonPOId")]
        public double PricePerDealUnit { get; set; }
        public double Quantity { get; set; }
        public long UomId { get; set; }
        [MaxLength(100)]
        public string? UomUnit { get; set; }
        public string? ProductRemark { get; set; }
    }
}
