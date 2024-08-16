using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDispositionPurchase
{
    public class GarmentDispositionByInvoiceReportDto
    {
        public int DispositionId { get; set; }
        public string DispositionNo { get; set; }
        public string InvoiceNo { get; set; }
        public double Amount { get; set; }
        public string SupplierName { get; set; }
        public string CurrencyCode { get; set; }
        public DateTimeOffset SendToCashierDate { get; set; }
        public DateTimeOffset ReceiptCashierDate { get; set; }
        public int ItemId { get; set; }
        public int Count { get; set; }
    }

    public class GarmentDispositionIdDto
    {
        public int Id { get; set; }
        public int DispositionNoteId { get; set; }
        public DateTimeOffset SendToCashierDate { get; set; }
        public DateTimeOffset ReceiptCashierDate { get; set; }

    }
    
}
