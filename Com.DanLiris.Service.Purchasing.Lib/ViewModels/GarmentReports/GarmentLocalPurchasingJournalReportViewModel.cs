﻿using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports
{
    public class GarmentLocalPurchasingJournalReportTempViewModel
    {
        public string? UnitCode { get; set; }
        public string? Code { get; set; }
        public string? PaymentType { get; set; }
        public string? CurrencyCode { get; set; }
        public string? IsVat { get; set; }
        public double VatRate { get; set; }
        public string? IsTax { get; set; }
        public double TaxRate { get; set; }
        public decimal Amount { get; set; }      
    }

    public class GarmentLocalPurchasingJournalReportTemp1ViewModel
    {
        public string? Code { get; set; }
        public double Amount { get; set; }
    }

    public class GarmentLocalPurchasingJournalReportViewModel
    {
        public string? remark { get; set; }
        public string? account { get; set; }
        public decimal debit { get; set; }
        public decimal credit { get; set; }
    }
}