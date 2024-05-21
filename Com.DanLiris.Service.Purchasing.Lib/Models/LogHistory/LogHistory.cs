using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.LogHistory
{
    public class LogHistory
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity), Key()]
        public int Id { get; set; }
        [MaxLength(255)]
        public string? Division { get; set; }
        [MaxLength(1000)]
        public string? Activity { get; set; }
        public DateTime CreatedDate { get; set; }
        [MaxLength(255)]
        public string? CreatedBy { get; set; }
    }
}
