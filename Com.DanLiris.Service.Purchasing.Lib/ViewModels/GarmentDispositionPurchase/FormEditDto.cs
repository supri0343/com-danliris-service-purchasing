using Com.DanLiris.Service.Purchasing.Lib.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDispositionPurchase
{
    public class FormEditDto : IValidatableObject
    {
        public int Id { get; set; }
        public string? DispositionNo { get; set; }
        public string? Category { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierCode { get; set; }
        public bool SupplierIsImport { get; set; }
        public int CurrencyId { get; set; }
        public string? CurrencyName { get; set; }
        public string? CurrencyCode { get; set; }
        public string? Bank { get; set; }
        public string? ConfirmationOrderNo { get; set; }
        public string? PaymentType { get; set; }
        public DateTimeOffset PaymentDueDate { get; set; }
        public string? Remark { get; set; }
        public string? ProformaNo { get; set; }
        public double DPP { get; set; }
        public double IncomeTaxValue { get; set; }
        public double VatValue { get; set; }
        public double MiscAmount { get; set; }
        public double Amount { get; set; }
        public List<FormItemDto> Items { get; set; }
        //public DateTime CreatedUtc { get; set; }
        public PurchasingGarmentExpeditionPosition Position { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.PaymentDueDate.Equals(DateTimeOffset.MinValue) || this.PaymentDueDate == null)
            {
                yield return new ValidationResult("Tanggal Jatuh Tempo harus diisi", new List<string> { "PaymentDueDate" });
            }

            if (this.SupplierCode == null || SupplierId == 0)
            {
                yield return new ValidationResult("Supplier harus diisi", new List<string> { "Supplier" });
            }


            if (this.CurrencyCode == null || CurrencyId == 0)
            {
                yield return new ValidationResult("Mata Uang harus diisi", new List<string> { "Currency" });
            }

            if (string.IsNullOrWhiteSpace(ConfirmationOrderNo) || ConfirmationOrderNo == null)
            {
                yield return new ValidationResult("No Order Confirmation harus diisi", new List<string> { "ConfirmationOrderNo" });
            }

            if (string.IsNullOrWhiteSpace(PaymentType) || PaymentType == null)
            {
                yield return new ValidationResult("Term Pembayaran harus diisi", new List<string> { "PaymentMethod" });
            }


            if (string.IsNullOrWhiteSpace(Bank) || Bank == null)
            {
                yield return new ValidationResult("Bank harus diisi", new List<string> { "Bank" });
            }

        }
    }
}
