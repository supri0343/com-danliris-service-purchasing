using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentUnitReceiptNoteViewModels
{
    public class GarmentSubconUnitReceiptNoteItemViewModel : BaseViewModel
    {
        public long URNId { get; set; }

        public long DOItemId { get; set; }

        public long EPOItemId { get; set; }
        public string DRItemId { get; set; }

        public long PRId { get; set; }
        public string PRNo { get; set; }
        public long PRItemId { get; set; }

        public long POId { get; set; }
        public long POItemId { get; set; }
        public string POSerialNumber { get; set; }

        public GarmentProductViewModel Product { get; set; }


        public decimal ReceiptQuantity { get; set; }

        public UomViewModel Uom { get; set; }

        public decimal PricePerDealUnit { get; set; }

        public string DesignColor { get; set; }

        public bool IsCorrection { get; set; }

        public decimal Conversion { get; set; }

        public decimal SmallQuantity { get; set; }

        public decimal ReceiptCorrection { get; set; }

        public decimal OrderQuantity { get; set; }

        public UomViewModel SmallUom { get; set; }

        //public BuyerViewModel Buyer { get; set; }

        public decimal CorrectionConversion { get; set; }
        public double DOCurrencyRate { get; set; }
        public long ExpenditureItemId { get; set; }

        public long UENItemId { get; set; }
        public string PaymentType { get; set; }
        public string PaymentMethod { get; set; }

        public string Colour { get; set; }
        public string Rack { get; set; }
        public string Level { get; set; }
        public string Box { get; set; }
        public string Area { get; set; }

        public long DOItemsId { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal DOQuantity { get; set; }

        public IntegrationViewModel.StorageViewModel Storage { get; set; }
    }
}
