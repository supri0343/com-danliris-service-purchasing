﻿using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.UnitPaymentOrderModel
{
    public class UnitPaymentOrderItem : StandardEntity<long>
    {
        public long URNId { get; set; }
        [MaxLength(255)]
        public string? URNNo { get; set; }
        public long DOId { get; set; }
        [MaxLength(255)]
        public string? DONo { get; set; }
        public DateTimeOffset URNDate { get; set; }

        public virtual ICollection<UnitPaymentOrderDetail> Details { get; set; }

        public virtual long UPOId { get; set; }
        [ForeignKey("UPOId")]
        public virtual UnitPaymentOrder UnitPaymentOrder { get; set; }
    }
}
