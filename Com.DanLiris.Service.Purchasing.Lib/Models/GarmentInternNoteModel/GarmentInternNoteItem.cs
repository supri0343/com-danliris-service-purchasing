﻿using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.InternNoteModel
{
    public class GarmentInternNoteItem : StandardEntity<long>
    {
        public string InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTimeOffset InvoiceDate { get; set; }
        public double TotalAmount { get; set; }
        public bool IsPayTax { get; set; }
        public virtual long INNo { get; set; }
        public virtual ICollection<GarmentInternNoteDetail> Details { get; set; }

        public virtual long GarmentINId { get; set; }
        [ForeignKey("GarmentINId")]
        public virtual GarmentInternNote InternNote { get; set; }
    }
}
