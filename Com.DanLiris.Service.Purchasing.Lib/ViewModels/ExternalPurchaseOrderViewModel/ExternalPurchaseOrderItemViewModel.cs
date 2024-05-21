using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.ExternalPurchaseOrderViewModel
{
    public class ExternalPurchaseOrderItemViewModel : BaseViewModel
    {
        public string? poNo { get; set; }
        public string? prNo { get; set; }
        public long poId { get; set; }
        public long prId { get; set; }

        public UnitViewModel unit { get; set; }
        public List<ExternalPurchaseOrderDetailViewModel> details { get; set; }

    }
}
