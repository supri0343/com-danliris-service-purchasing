using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel
{
    public class GarmentExpenditureViewModelForTrace
    {
        public string? BuyerCode { get;  set; }
        public string? BuyerName { get;  set; }
        public string? UnitCode { get; set; }
        //public string? ExpenditureType { get; internal set; }
        public string? RONo { get;  set; }
        //public string? Article { get; internal set; }
        //public GarmentComodity Comodity { get; internal set; }
        public string? ComodityName { get;  set; }
        //public Buyer Buyer { get; internal set; }
        //public DateTimeOffset ExpenditureDate { get; internal set; }
        public string? Invoice { get;  set; }
        //public string? ContractNo { get; internal set; }
        //public double Carton { get; internal set; }
        //public string? Description { get; internal set; }
        //public bool IsReceived { get; private set; }
        public double TotalQuantity { get; set; }
    }
}
