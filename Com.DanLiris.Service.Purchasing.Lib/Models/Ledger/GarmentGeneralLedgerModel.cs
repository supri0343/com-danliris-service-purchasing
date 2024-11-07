using Com.Moonlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Models.Ledger
{
    public class GarmentGeneralLedgerModel : StandardEntity<long>
    {
        [MaxLength(10)]
        public string COANo { get; set; }
        [MaxLength(100)]
        public string JournalType { get; set; }
        [MaxLength(10)]
        public string AccountNo { get; set; }
        [MaxLength(50)]
        public string ProofNo { get; set; }
        public DateTime? Date { get; set; }
        [MaxLength(500)]
        public string Remark { get; set; }

        public double Debit { get; set; }
        public double Credit { get; set; }
    }
}
