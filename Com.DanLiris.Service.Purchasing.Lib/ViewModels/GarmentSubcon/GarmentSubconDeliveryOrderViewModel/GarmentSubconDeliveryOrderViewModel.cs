using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentSubconDeliveryOrderViewModel
{
    public class GarmentSubconDeliveryOrderViewModel : BaseViewModel, IValidatableObject
    {
        public long customsId { get; set; }
        public string doNo { get; set; }
        public DateTimeOffset doDate { get; set; }
        public DateTimeOffset? arrivalDate { get; set; }
        public SupplierViewModel supplier { get; set; }
        public string remark { get; set; }
        public long costCalculationId { get; set; }
        public string roNo { get; set; }
        public string article { get; set; }
        public string beacukaiNo { get; set; }
        public DateTimeOffset beacukaiDate { get; set; }
        public string beacukaiType { get; set; }
        public bool IsReceived { get; set; }
        
        public List<GarmentSubconDeliveryOrderItemViewModel> items { get; set; }
        public List<GarmentSubconDeliveryOrderItemViewModel> itemsPR { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(doNo))
            {
                yield return new ValidationResult("DONo is required", new List<string> { "DONo" });
            }
            else
            {
                PurchasingDbContext purchasingDbContext = (PurchasingDbContext)validationContext.GetService(typeof(PurchasingDbContext));
                if (purchasingDbContext.GarmentSubconDeliveryOrders.Where(DO => DO.DONo.Equals(doNo) && DO.Id != Id && DO.DODate.ToOffset((new TimeSpan(7, 0, 0))) == doDate && DO.ProductOwnerId == supplier.Id && DO.ArrivalDate.ToOffset((new TimeSpan(7, 0, 0))) == arrivalDate).Count() > 0)
                {
                    yield return new ValidationResult("DONo is already exist", new List<string> { "DONo" });
                }
            }
            if (arrivalDate.Equals(DateTimeOffset.MinValue) || arrivalDate == null)
            {
                yield return new ValidationResult("ArrivalDate is required", new List<string> { "ArrivalDate" });
            }
            if (doDate.Equals(DateTimeOffset.MinValue) || doDate == null)
            {
                yield return new ValidationResult("DODate is required", new List<string> { "DODate" });
            }
            if (arrivalDate != null && doDate > arrivalDate)
            {
                yield return new ValidationResult("DODate is greater than ArrivalDate", new List<string> { "DODate" });
            }
            if (supplier == null)
            {
                yield return new ValidationResult("Supplier is required", new List<string> { "Supplier" });
            }
            //if (string.IsNullOrEmpty(roNo))
            //{
            //    yield return new ValidationResult("RONo is required", new List<string> { "RONo" });
            //}
            int itemErrorCount = 0;

            if ((this.items == null || items.Count <= 0) && (this.itemsPR == null || itemsPR.Count <= 0))
            {
                yield return new ValidationResult("Item is required", new List<string> { "itemscount" });
            }
            else if ((this.items != null || items.Count != 0) && (this.itemsPR == null || itemsPR.Count <= 0))
            {
                string itemError = "[";

                foreach (var item in items)
                {
                    itemError += "{";

                    if (item.EPONo == null)
                    {
                        itemErrorCount++;
                        itemError += "EPONo: 'EPONo harus diisi', ";
                    }

                    if (item.Product == null)
                    {
                        itemErrorCount++;
                        itemError += "Product: 'Barang harus diisi', ";
                    }

                    if (string.IsNullOrEmpty(item.POSerialNumber))
                    {
                        itemErrorCount++;
                        itemError += "POSerialNumber: 'POSerialNumber harus diisi', ";
                    }

                    if (item.DOQuantity <= 0)
                    {
                        itemErrorCount++;
                        itemError += "DOQuantity: 'DOQuantity harus lebih dari 0', ";
                    }
                     if (item.DOQuantity > item.BudgetQuantity)
                    {
                        itemErrorCount++;
                        itemError += "DOQuantity: 'DOQuantity tidak boleh lebih dari Budget Qty', ";
                    }

                    itemError += "}, ";
                }

                itemError += "]";

                if (itemErrorCount > 0)
                    yield return new ValidationResult(itemError, new List<string> { "Items" });
            }

            if ((this.items == null || items.Count <= 0) && (this.itemsPR == null || itemsPR.Count <= 0))
            {
                yield return new ValidationResult("ItemPR is required", new List<string> { "itemsPRcount" });
            }
            else if ((this.items == null || items.Count == 0) && (this.itemsPR != null || itemsPR.Count != 0))
            {
                string itemError = "[";

                foreach (var item in itemsPR)
                {
                    itemError += "{";

                    if (item.Product == null)
                    {
                        itemErrorCount++;
                        itemError += "Product: 'Barang harus diisi', ";
                    }

                    if (string.IsNullOrEmpty(item.POSerialNumber))
                    {
                        itemErrorCount++;
                        itemError += "POSerialNumber: 'POSerialNumber harus diisi', ";
                    }

                    if (item.DOQuantity <= 0)
                    {
                        itemErrorCount++;
                        itemError += "DOQuantity: 'DOQuantity harus lebih dari 0', ";
                    }
                    else if (item.DOQuantity > item.BudgetQuantity)
                    {
                        itemErrorCount++;
                        itemError += "DOQuantity: 'DOQuantity tidak boleh lebih dari Budget Qty', ";
                    }

                    itemError += "}, ";
                }

                itemError += "]";

                if (itemErrorCount > 0)
                    yield return new ValidationResult(itemError, new List<string> { "ItemsPR" });
            }
            if (( items.Count > 0) && ( itemsPR.Count > 0))
            {
                yield return new ValidationResult("Item yang terisi hanya boleh salah satu", new List<string> { "itemscount" });
            }
        }
    }
    
}
