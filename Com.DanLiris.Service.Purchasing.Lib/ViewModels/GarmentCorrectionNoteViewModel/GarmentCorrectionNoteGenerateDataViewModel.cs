using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentCorrectionNoteViewModel
{
    public class GarmentCorrectionNoteGenerateDataViewModel : BaseViewModel
    {
        public string? CorrectionNo { get; set; }
        public DateTimeOffset? CorrectionDate { get; set; }
        public string? CorrectionType { get; set; }
        public string? SupplierCode { get; set; }
        public string? SupplierName { get; set; }
        public string? DONo { get; set; }
        public DateTimeOffset? DODate { get; set; }
        public string? PaymentBill { get; set; }
        public string? BillNo { get; set; }
        public string? NKPN { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public string? PaymentDueDays { get; set; }       
        public string? UseVat { get; set; }
        public string? VatRate { get; set; }      
        public string? UseIncomeTax { get; set; }
        public string? IncomeTaxName { get; set; }
        public decimal IncomeTaxRate { get; set; }
        public string? CurrencyCode { get; set; }
        public double? CurrencyRate { get; set; }
        public decimal Amount { get; set; }
    }
}
