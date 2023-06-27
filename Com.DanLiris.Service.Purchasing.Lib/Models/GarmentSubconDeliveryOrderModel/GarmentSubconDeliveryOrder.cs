using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubconDeliveryOrderModel
{
    public class GarmentSubconDeliveryOrder : BaseModel
    {
        public long CustomsId { get; set; }
        [MaxLength(255)]
        public string DONo { get; set; }
        public DateTimeOffset DODate { get; set; }
        public DateTimeOffset ArrivalDate { get; set; }

        /* Supplier */
        [MaxLength(255)]
        public long SupplierId { get; set; }
        [MaxLength(255)]
        public string SupplierCode { get; set; }
        [MaxLength(1000)]
        public string SupplierName { get; set; }

        public string Remark { get; set; }

        public long CostCalculationId { get; set; }
        public string RONo { get; set; }
        public virtual IEnumerable<GarmentSubconDeliveryOrderItem> Items { get; set; }
    }
}
