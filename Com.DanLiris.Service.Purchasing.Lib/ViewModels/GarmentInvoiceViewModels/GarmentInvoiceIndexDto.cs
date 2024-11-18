using System;
using System.Collections.Generic;
using System.Text;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentInvoiceModel;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentInvoiceViewModels
{
    public class   GarmentInvoiceIndexDto : BaseViewModel
    {
        public string invoiceNo { get; set; }
        public string supplierName { get; set; }
        public DateTimeOffset? invoiceDate { get; set; }
        public string internNoteNo { get; set; }
        public long internNoteId { get; set; }
        public SupplierViewModel supplier { get; set; }
        public string createdBy { get; set; }
        public string npn { get; set; }
        public string nph { get; set; }
        public bool isPayTax { get; set; }
        public bool useIncomeTax { get; set; }
        public bool useVat { get; set; }
        public virtual ICollection< GarmentInvoiceItemIndexDto> items { get; set; }
    }


    public class GarmentInvoiceItemIndexDto : BaseViewModel
    {
        public string deliveryOrderNo { get; set; }
        public long deliveryOrderId { get; set; }
        public Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel.GarmentDeliveryOrderViewModel deliveryOrder { get; set; }

    }

  
}

