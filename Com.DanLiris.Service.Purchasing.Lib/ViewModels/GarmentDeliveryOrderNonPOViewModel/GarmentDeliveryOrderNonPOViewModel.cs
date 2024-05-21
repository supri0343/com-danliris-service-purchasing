﻿using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderNonPOViewModel
{
    public class GarmentDeliveryOrderNonPOViewModel : BaseViewModel, IValidatableObject
	{
        public string? UId { get; set; }
        public long customsId { get; set; }
		public string? doNo { get; set; }
        public DateTimeOffset doDate { get; set; }
        public DateTimeOffset? arrivalDate { get; set; }

        public SupplierViewModel supplier { get; set; }

        public string? shipmentType { get; set; }
        public string? shipmentNo { get; set; }

        public string? remark { get; set; }
        public bool isClosed { get; set; }
        public bool isCustoms { get; set; }
        public bool isInvoice { get; set; }
        public string? internNo { get; set; }
        public string? billNo { get; set; }
        public string? paymentBill { get; set; }
        public double totalAmount { get; set; }

        public bool isCorrection { get; set; }

        public bool useVat { get; set; }
        public bool useIncomeTax { get; set; }
        public bool isPayVAT { get; set; }
        public bool isPayIncomeTax { get; set; }

        public VatViewModel vat { get; set; }
        public IncomeTaxViewModel incomeTax { get; set; }

        public string? paymentType { get; set; }
        public string? paymentMethod { get; set; }
        public CurrencyViewModel docurrency { get; set; }
        public bool IsPO { get; set; }
        public bool IsReceived { get; set; }
        public List<GarmentDeliveryOrderNonPOItemViewModel> items { get; set; }

        //public List<long> unitReceiptNoteIds { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(doNo))
            {
                yield return new ValidationResult("DoNo is required", new List<string> { "doNo" });
            }
            else
            {
                PurchasingDbContext purchasingDbContext = (PurchasingDbContext)validationContext.GetService(typeof(PurchasingDbContext));
                if (purchasingDbContext.GarmentDeliveryOrders.AsNoTracking().ToList().Where(DO => DO.DONo.Equals(doNo) && DO.Id != Id && DO.DODate.ToOffset((new TimeSpan(7, 0, 0))) == doDate && DO.SupplierId == supplier.Id && DO.ArrivalDate.ToOffset((new TimeSpan(7, 0, 0))) == arrivalDate).Count() > 0)
                {
                    yield return new ValidationResult("DoNo is already exist", new List<string> { "doNo" });
                }
            }
            if (arrivalDate.Equals(DateTimeOffset.MinValue) || arrivalDate == null)
            {
                yield return new ValidationResult("ArrivalDate is required", new List<string> { "arrivalDate" });
            }
            if (doDate.Equals(DateTimeOffset.MinValue) || doDate == null)
            {
                yield return new ValidationResult("DoDate is required", new List<string> { "doDate" });
            }
            if (arrivalDate != null && doDate > arrivalDate)
            {
                yield return new ValidationResult("DoDate is greater than ArrivalDate", new List<string> { "doDate" });
            }
            if (supplier == null)
            {
                yield return new ValidationResult("Supplier is required", new List<string> { "supplier" });
            }
            if (supplier.Import==true && shipmentNo == null )
            {
                yield return new ValidationResult("ShipmentNo is required", new List<string> { "shipmentNo" });
            }


            int itemErrorCount = 0;
            int detailErrorCount = 0;

            if (this.items == null || items.Count <= 0)
            {
                yield return new ValidationResult("Item is required", new List<string> { "itemscount" });
            }
            else
            {
                string itemError = "[";

                foreach (var item in items)
                {
                    itemError += "{";

                    if (string.IsNullOrEmpty(item.ProductRemark))
                    {
                        itemErrorCount++;
                        itemError += "ProductRemark: 'Keterangan barang harus diisi', ";
                    }

                    

                    itemError += "}, ";
                }

                itemError += "]";

                if (itemErrorCount > 0)
                    yield return new ValidationResult(itemError, new List<string> { "items" });
            }
        }
    }
}
