using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconCustomOut
{
    public class GarmentSubconCustomOutItem : StandardEntity<long>
    {
        //LeftOver
        public long? UENId { get; set; }
        [MaxLength(255)]
        public string UENNo { get; set; }
        

        //Service
        public long? LocalSalesNoteId { get; set; }
        public string LocalSalesNoteNo { get; set; }
        public long? PackageUomId { get; set; }
        [MaxLength(1000)]
        public string PackageUomUnit { get; set; }
        public double? PackageQuantity { get; set; }


        //All
        public double Quantity { get; set; }
        public long UomId { get; set; }
        [MaxLength(1000)]
        public string UomUnit { get; set; }

        public virtual long CustomsId { get; set; }
        [ForeignKey("CustomsId")]
        public virtual GarmentSubconCustomOut GarmentSubconCustomOut { get; set; }
        public virtual IEnumerable<GarmentSubconCustomOutDetail> Details { get; set; }
        


    }
}
