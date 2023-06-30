using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconUnitReceiptNoteModel
{
    public class GarmentSubconUnitReceiptNote : BaseModel
    {
        [MaxLength(255)]
        public string URNNo { get; set; }

        public long UnitId { get; set; }
        [MaxLength(255)]
        public string UnitCode { get; set; }
        [MaxLength(1000)]
        public string UnitName { get; set; }

        public long ProductOwnerId { get; set; }
        [MaxLength(255)]
        public string ProductOwnerCode { get; set; }
        [MaxLength(1000)]
        public string ProductOwnerName { get; set; }

        public long DOId { get; set; }
        [MaxLength(255)]
        public string DONo { get; set; }

        public DateTimeOffset ReceiptDate { get; set; }

        public bool IsStorage { get; set; }
     
        public string Remark { get; set; }

        public bool IsCorrection { get; set; }

        public bool IsUnitDO { get; set; }

        public string DeletedReason { get; set; }

        public double? DOCurrencyRate { get; set; }

        public string URNType { get; set; }
        //public long UENId { get; set; }
        //public string UENNo { get; set; }
        public string  DRId { get; set; }
        public string DRNo { get; set; }

        //public long ExpenditureId { get; set; }
        //[MaxLength(20)]
        //public string ExpenditureNo { get; set; }
        //[MaxLength(20)]
        public string Category { get; set; }

        //Enhance Subcon
        [MaxLength(255)]
        public string RONo { get; set; }
        public string Article { get; set; }
        public string BeacukaiNo { get; set; }
        public DateTimeOffset BeacukaiDate { get; set; }
        public string BeacukaiType { get; set; }

        public virtual ICollection<GarmentSubconUnitReceiptNoteItem> Items { get; set; }
    }
}
