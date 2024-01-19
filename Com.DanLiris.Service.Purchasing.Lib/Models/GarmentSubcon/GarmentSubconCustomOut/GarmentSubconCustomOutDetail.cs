using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconCustomOut
{
    public class GarmentSubconCustomOutDetail : StandardEntity<long>
    {
        [MaxLength(255)]
        public long ProductId { get; set; }
        [MaxLength(255)]
        public string ProductCode { get; set; }
        [MaxLength(1000)]
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public long UomId { get; set; }
        [MaxLength(1000)]
        public string UomUnit { get; set; }
        public virtual long CustomsItemId { get; set; }
        [ForeignKey("CustomsItemId")]
        public virtual GarmentSubconCustomOutItem GarmentSubconCustomOutItem { get; set; }
    }
}
