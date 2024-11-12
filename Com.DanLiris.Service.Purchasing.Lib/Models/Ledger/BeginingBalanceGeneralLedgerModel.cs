using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.Ledger
{
    public class BeginingBalanceGeneralLedgerModel : StandardEntity<long>
    {
        [MaxLength(10)]
        public string? COANo { get; set; }
        [MaxLength(100)]
        public string JournalType { get; set; }
        public double BeginingTextileDebit { get; set; }
        public double BeginingTextileCredit { get; set; }
        public double BeginingFinishingPrintingDebit { get; set; }
        public double BeginingFinishingPrintingCredit { get; set; }
        public double BeginingGarmentDebit { get; set; }
        public double BeginingGarmentCredit { get; set; }

    }
}
