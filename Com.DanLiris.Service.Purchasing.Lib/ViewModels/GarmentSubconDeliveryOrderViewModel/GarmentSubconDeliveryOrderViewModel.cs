using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubconDeliveryOrderViewModel
{
    public class GarmentSubconDeliveryOrderViewModel : BaseViewModel, IValidatableObject
    {
        public long CustomsId { get; set; }
        public string DONo { get; set; }
        public DateTimeOffset DODate { get; set; }
        public DateTimeOffset? ArrivalDate { get; set; }
        public SupplierViewModel Supplier { get; set; }
        public string Remark { get; set; }
        public long CostCalculationId { get; set; }
        public string RONo { get; set; }
        public List<GarmentSubconDeliveryOrderItemViewModel> Items { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(DONo))
            {
                yield return new ValidationResult("DONo is required", new List<string> { "DONo" });
            }
            else
            {
                PurchasingDbContext purchasingDbContext = (PurchasingDbContext)validationContext.GetService(typeof(PurchasingDbContext));
                if (purchasingDbContext.GarmentSubconDeliveryOrders.Where(DO => DO.DONo.Equals(DONo) && DO.Id != Id && DO.DODate.ToOffset((new TimeSpan(7, 0, 0))) == DODate && DO.SupplierId == Supplier.Id && DO.ArrivalDate.ToOffset((new TimeSpan(7, 0, 0))) == ArrivalDate).Count() > 0)
                {
                    yield return new ValidationResult("DONo is already exist", new List<string> { "DONo" });
                }
            }
            if (ArrivalDate.Equals(DateTimeOffset.MinValue) || ArrivalDate == null)
            {
                yield return new ValidationResult("ArrivalDate is required", new List<string> { "ArrivalDate" });
            }
            if (DODate.Equals(DateTimeOffset.MinValue) || DODate == null)
            {
                yield return new ValidationResult("DODate is required", new List<string> { "DODate" });
            }
            if (ArrivalDate != null && DODate > ArrivalDate)
            {
                yield return new ValidationResult("DODate is greater than ArrivalDate", new List<string> { "DODate" });
            }
            if (Supplier == null)
            {
                yield return new ValidationResult("Supplier is required", new List<string> { "Supplier" });
            }
            if (string.IsNullOrEmpty(RONo))
            {
                yield return new ValidationResult("RONo is required", new List<string> { "RONo" });
            }
            int itemErrorCount = 0;

            if (this.Items == null || Items.Count <= 0)
            {
                yield return new ValidationResult("Item is required", new List<string> { "itemscount" });
            }
            else
            {
                string itemError = "[";

                foreach (var item in Items)
                {
                    itemError += "{";

                    if (item.Product == null )
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
                    }else if (item.DOQuantity > item.BudgetQuantity)
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
        }
    }
    
}
