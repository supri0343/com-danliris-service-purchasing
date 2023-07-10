using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports
{
    //public class GarmentDetailLocalPurchasingJournalReportTempViewModel
    //{
    //    public string urnno { get; set; }
    //    public DateTimeOffset urndate { get; set; }
    //    public string remark { get; set; }
    //    public string supplier { get; set; }
    //    public string inno { get; set; }
    //    public string billno { get; set; }
    //    public string code { get; set; }
    //    public string paymentyype { get; set; }
    //    public string currencycode { get; set; }
    //    public double? rate { get; set; }
    //    public string isvat { get; set; }
    //    public double vatrate { get; set; }       
    //    public string istax { get; set; }
    //    public double taxrate { get; set; }
    //    public decimal amount { get; set; }     
    //}

    public class GarmentDetailLocalPurchasingJournalReportViewModel
    {
        public string urnno { get; set; }
        public DateTimeOffset urndate { get; set; }
        public string remark { get; set; }
        public string supplier { get; set; }
        public string inno { get; set; }
        public string billno { get; set; }
        public string code { get; set; }
        public string paymentyype { get; set; }
        public string currencycode { get; set; }
        public double? rate { get; set; }
        public string isvat { get; set; }
        public double vatrate { get; set; }
        public string istax { get; set; }
        public double taxrate { get; set; }
        public decimal amount { get; set; }
        public string coaname { get; set; }
        public string account { get; set; }
        public decimal debit { get; set; }
        public decimal credit { get; set; }
    }
}