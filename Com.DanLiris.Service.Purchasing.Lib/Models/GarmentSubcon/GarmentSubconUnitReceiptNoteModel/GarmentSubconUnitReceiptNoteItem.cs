using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconUnitReceiptNoteModel
{
    public class GarmentSubconUnitReceiptNoteItem : StandardEntity<long>
    {
        public long URNId { get; set; }
        [ForeignKey("URNId")]
        public virtual GarmentSubconUnitReceiptNote GarmentUnitReceiptNote { get; set; }

        //public long UENItemId { get; set; }

        public long DOItemId { get; set; }

        //public long EPOItemId { get; set; }
        public string DRItemId { get; set; }

        //public long PRId { get; set; }
        //[MaxLength(255)]
        //public string PRNo { get; set; }
        //public long PRItemId { get; set; }
		public string UId { get; set; }
        //public long POId { get; set; }
        //public long POItemId { get; set; }
        [MaxLength(1000)]
        public string POSerialNumber { get; set; }

        public long ProductId { get; set; }
        [MaxLength(255)]
        public string ProductCode { get; set; }
        [MaxLength(1000)]
        public string ProductName { get; set; }
        public string ProductRemark { get; set; }

        //[MaxLength(255)]
        //public string RONo { get; set; }

        [Column(TypeName = "decimal(20, 4)")]
        public decimal ReceiptQuantity { get; set; }
        public long SmallUomId { get; set; }
        [MaxLength(1000)]
        public string SmallUomUnit { get; set; }


        [Column(TypeName = "decimal(20, 4)")]
        public decimal PricePerDealUnit { get; set; }

        [MaxLength(1000)]
        public string DesignColor { get; set; }

        public bool IsCorrection { get; set; }

        [Column(TypeName = "decimal(38, 20)")]
        public decimal Conversion { get; set; }
        [Column(TypeName = "decimal(38, 20)")]
        public decimal CorrectionConversion { get; set; }

        public decimal SmallQuantity { get; set; }

        public decimal ReceiptCorrection { get; set; }

        public decimal OrderQuantity { get; set; }

      
        //public double DOCurrencyRate { get; set; }
        //public long ExpenditureItemId { get; set; }
        public string Colour { get; set; }
        public string Rack { get; set; }
        public string Level { get; set; }
        public string Box { get; set; }
        public string Area { get; set; }
        public decimal RemainingQuantity { get; set; }
        //enhance subcon
        public long StorageId { get; set; }
        [MaxLength(255)]
        public string StorageCode { get; set; }
        [MaxLength(1000)]
        public string StorageName { get; set; }
        public double DOQuantity { get; set; }
        public long UomId { get; set; }
        [MaxLength(1000)]
        public string UomUnit { get; set; }

    }
}
