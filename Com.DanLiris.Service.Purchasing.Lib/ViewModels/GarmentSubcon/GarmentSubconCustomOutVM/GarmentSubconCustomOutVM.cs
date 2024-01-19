using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentSubconCustomOutVM
{
    public class GarmentSubconCustomOutVM : BaseViewModel, IValidatableObject
    {
        public string bcNo { get; set; }
        public DateTimeOffset bcDate { get; set; }
        public string bcType { get; set; }
        public SupplierViewModel productOwner { get; set; }
        public string category { get; set; }
        public string remark { get; set; }
        public List<GarmentSubconCustomOutItemVM> items { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(bcNo))
            {
                yield return new ValidationResult("BCNo is required", new List<string> { "BCNo" });
            }
            else
            {
                PurchasingDbContext purchasingDbContext = (PurchasingDbContext)validationContext.GetService(typeof(PurchasingDbContext));
                if (purchasingDbContext.GarmentSubconCustomOuts.Where(bc => bc.BCNo.Equals(bcNo) && bc.Id != Id && bc.BCDate.ToOffset((new TimeSpan(7, 0, 0))).Year == bcDate.Year ).Count() > 0)
                {
                    yield return new ValidationResult("BCNo is already exist", new List<string> { "BCNo" });
                }
            }
            if (bcDate.Equals(DateTimeOffset.MinValue) || bcDate == null)
            {
                yield return new ValidationResult("BCDate is required", new List<string> { "BCDate" });
            }
            if (string.IsNullOrWhiteSpace(bcType))
            {
                yield return new ValidationResult("BCType is required", new List<string> { "BCType" });
            }
            if(productOwner == null || productOwner?.Id == 0 )
            {
                yield return new ValidationResult("Tujuan is required", new List<string> { "ProductOwner" });
            }
            if (string.IsNullOrWhiteSpace(category))
            {
                yield return new ValidationResult("Category is required", new List<string> { "Category" });
            }

            int itemErrorCount = 0;

            if ((this.items == null || items.Count <= 0) )
            {
                yield return new ValidationResult("Item is required", new List<string> { "itemscount" });
            }else if((this.items != null || items.Count != 0))
            {
                string itemError = "[";

                foreach (var item in items)
                {
                    itemError += "{";
                    if(category == "JASA" && string.IsNullOrWhiteSpace(item.localSalesNoteNo))
                    {
                        itemErrorCount++;
                        itemError += "LocalSalesNoteNo: 'Nomor Nota Jual Lokal harus diisi', ";
                    }

                    if (category == "SISA" && string.IsNullOrWhiteSpace(item.uenNo))
                    {
                        itemErrorCount++;
                        itemError += "UENNo: 'Nomor Bon Unit Keluar harus diisi', ";
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
