using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInternNoteViewModel
{
    public class GarmentInternNoteGenerateDataViewModel : BaseViewModel
    {
        public string? INNo { get; set; }
        public DateTimeOffset? INDate { get; set; }
        public string? SupplierCode { get; set; }
        public string? SupplierName { get; set; }
        public string? DONo { get; set; }
        public DateTimeOffset? DODate { get; set; }
        public string? PaymentBill { get; set; }
        public string? BillNo { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public string? PaymentDueDays { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTimeOffset? InvoiceDate { get; set; }
        public string? UseVat { get; set; }
        public string? VatNo { get; set; }
        public double VatRate { get; set; }
        public DateTimeOffset? VatDate { get; set; }
        public string? UseIncomeTax { get; set; }
        public string? IncomeTaxName { get; set; }
        public double IncomeTaxRate { get; set; }
        public string? IncomeTaxNo { get; set; }
        public DateTimeOffset? IncomeTaxDate { get; set; }
        public string? CurrencyCode { get; set; }
        public double? CurrencyRate { get; set; }
        public double Amount { get; set; }
    }
}
