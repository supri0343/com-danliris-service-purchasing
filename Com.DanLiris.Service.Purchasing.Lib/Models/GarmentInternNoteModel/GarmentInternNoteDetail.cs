﻿using Com.Moonlay.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel
{
    public class GarmentInternNoteDetail : StandardEntity<long>
    {
        public long DOId { get; set; }
        public string DONo { get; set; }
        public long EPOId { get; set; }
        public string EPONo { get; set; }
        public string POSerialNumber { get; set; }
        public long ROId { get; set; }
        public string RONo { get; set; }
        public string TermOfPayment { get; set; }
        public string PaymentType { get; set; }
        public double ReceiptQuantity { get; set; }
        public double PaymentDueDays { get; set; }
        public DateTimeOffset DODate { get; set; }

        /*Product*/
        [MaxLength(255)]
        public string ProductCode { get; set; }
        [MaxLength(255)]
        public string ProductId { get; set; }
        [MaxLength(255)]
        public string ProductName { get; set; }

        public long Quantity { get; set; }

        /* Unit */
        [MaxLength(255)]
        public string UnitId { get; set; }
        [MaxLength(255)]
        public string UnitCode { get; set; }
        [MaxLength(255)]
        public string UnitName { get; set; }
        
        public double PricePerDealUnit { get; set; }
        public double PriceTotal { get; set; }

        public virtual long GarmentItemINId { get; set; }
        [ForeignKey("GarmentItemINId")]
        public virtual GarmentInternNoteItem InternNoteItem { get; set; }
    }
}