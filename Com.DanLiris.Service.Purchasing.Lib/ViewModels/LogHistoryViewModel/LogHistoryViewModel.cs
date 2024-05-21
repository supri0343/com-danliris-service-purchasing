using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.LogHistoryViewModel
{
    public class LogHistoryViewModel
    {
        public string? Division { get; set; }
        public string? Activity { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}
