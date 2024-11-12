using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconCustomOut
{
    public class GarmentSubconCustomOut : BaseModel
    {
        [MaxLength(255)]
        public string? BCNo { get; set; }
        public DateTimeOffset BCDate { get; set; }
        [MaxLength(20)]
        public string? BCType { get; set; }

        /* ProductOwner */
        [MaxLength(255)]
        public long ProductOwnerId { get; set; }
        [MaxLength(255)]
        public string? ProductOwnerCode { get; set; }
        [MaxLength(1000)]
        public string? ProductOwnerName { get; set; }

        [MaxLength(255)]
        public string? Category { get; set; }
        [MaxLength(1000)]
        public string? Remark { get; set; }

        public virtual IEnumerable<GarmentSubconCustomOutItem> Items { get; set; }
    }
}
