using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentUnitExpenditureNoteModel
{
    public class GarmentSubconUnitExpenditureNoteItem : StandardEntity<long>
    {
        public long UENId { get; set; }
        [ForeignKey("UENId")]
        public virtual GarmentSubconUnitExpenditureNote GarmentUnitExpenditureNote { get; set; }

        public long UnitDOItemId { get; set; }
        public long URNItemId { get; set; }
        public long DODetailId { get; set; }
        public long EPOItemId { get; set; }
        public long POItemId { get; set; }
        public long PRItemId { get; set; }
        [MaxLength(255)]
        public string POSerialNumber { get; set; }

        public long ProductId { get; set; }
        [MaxLength(255)]
        public string ProductCode { get; set; }
        [MaxLength(255)]
        public string ProductName { get; set; }
        public string ProductRemark { get; set; }

        [MaxLength(255)]
        public string RONo { get; set; }

        public double Quantity { get; set; }

        public long UomId { get; set; }
        [MaxLength(255)]
        public string UomUnit { get; set; }

        public double PricePerDealUnit { get; set; }
        [MaxLength(255)]
        public string FabricType{ get; set; }

        public long ProductOwnerId { get; set; }
        [MaxLength(255)]
        public string ProductOwnerCode { get; set; }
        public double? DOCurrencyRate { get; set; }
        [Column(TypeName = "decimal(38, 20)")]
        public decimal Conversion { get; set; }
        [Column(TypeName = "decimal(38, 4)")]
        public decimal BasicPrice { get; set; }
        public double ReturQuantity { get; set; }

        [NotMapped]
        public bool IsSave { get; set; }

        [MaxLength(25)]
        public string ItemStatus { get; set; }
		[MaxLength(255)]
		public string UId { get; set; }
        public string BeacukaiNo { get; set; }
        public DateTimeOffset BeacukaiDate { get; set; }
        public string BeacukaiType { get; set; }
    }
}
