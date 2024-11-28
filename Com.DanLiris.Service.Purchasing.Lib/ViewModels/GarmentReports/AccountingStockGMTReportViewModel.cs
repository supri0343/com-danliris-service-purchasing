using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports
{
    public class AccountingStockGMTReportViewModel
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string RO { get; set; }
        public string Buyer { get; set; }
        public string PlanPo { get; set; }
        public string NoArticle { get; set; }
        public decimal BeginningBalanceQty { get; set; }
        public string BeginningBalanceUom { get; set; }
        public double BeginningBalancePrice { get; set; }
        public decimal ReceiptCorrectionQty { get; set; }
        public decimal ReceiptPurchaseQty { get; set; }
        public decimal ReceiptProcessQty { get; set; }
        public decimal ReceiptSubconPrice { get; set; }
        public decimal ReceiptSubconQty { get; set; }
        
        public decimal ReceiptCorrectionPrice { get; set; }
        public decimal ReceiptPurchasePrice { get; set; }
        public decimal ReceiptProcessPrice { get; set; }

        public double ExpendReturQty { get; set; }
        public double ExpendRestQty { get; set; }
        public double ExpendProcessQty { get; set; }
        public double ExpendSampleQty { get; set; }

        public double ExpendReturPrice { get; set; }
        public double ExpendRestPrice { get; set; }
        public double ExpendProcessPrice { get; set; }
        public double ExpendSamplePrice { get; set; }
        public decimal EndingBalanceQty { get; set; }
        public double EndingBalancePrice { get; set; }

        public double ExpendSubconPrice { get; set; }
        public double ExpendOtherPrice { get; set; }
        public double ExpendSubconQty { get; set; }
        public double ExpendOtherQty { get; set; }
    }

    public class AccountingStockGMTTempViewModel
    {
        public string ProductCode { get; set; }
        public string RO { get; set; }
        public string Buyer { get; set; }
        public string PlanPo { get; set; }
        public string NoArticle { get; set; }
        public decimal BeginningBalanceQty { get; set; }
        public string BeginningBalanceUom { get; set; }
        public decimal BeginningBalancePrice { get; set; }
        public decimal ReceiptCorrectionQty { get; set; }
        public decimal ReceiptPurchaseQty { get; set; }
        public decimal ReceiptProcessQty { get; set; }
        public decimal ReceiptSubconQty { get; set; }

        public decimal ReceiptCorrectionPrice { get; set; }
        public decimal ReceiptPurchasePrice { get; set; }
        public decimal ReceiptProcessPrice { get; set; }
        public decimal ReceiptSubconPrice { get; set; }

        public decimal ExpendReturQty { get; set; }
        public double ExpendRestQty { get; set; }
        public double ExpendProcessQty { get; set; }
        public double ExpendSampleQty { get; set; }

        public decimal ExpendReturPrice { get; set; }
        public double ExpendRestPrice { get; set; }
        public double ExpendProcessPrice { get; set; }
        public double ExpendSamplePrice { get; set; }

        public decimal EndingBalanceQty { get; set; }
        public double EndingBalancePrice { get; set; }

        public double ExpendSubconPrice { get; set; }
        public double ExpendOtherPrice { get; set; }
        public double ExpendSubconQty { get; set; }
        public double ExpendOtherQty { get; set; }
    }
}
