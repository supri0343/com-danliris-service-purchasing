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
        public long ProductOwnerId { get; set; }
        [MaxLength(255)]
        public string ProductOwnerCode { get; set; }
        [MaxLength(1000)]
        public string ProductOwnerName { get; set; }

        public string Remark { get; set; }

        //Enhance Subcon
        //CC
        public long CostCalculationId { get; set; }
        public string RONo { get; set; }
        public string Article { get; set; }
        //BC
        public string BeacukaiNo { get; set; }
        public DateTimeOffset BeacukaiDate { get; set; }
        public string BeacukaiType { get; set; }
        //
        public bool IsReceived { get; set; }
        public virtual IEnumerable<GarmentSubconDeliveryOrderItem> Items { get; set; }
    }
}
