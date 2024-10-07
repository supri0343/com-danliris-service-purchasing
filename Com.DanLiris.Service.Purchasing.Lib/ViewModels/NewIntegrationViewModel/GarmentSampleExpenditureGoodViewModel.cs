using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel.GarmentSampleExpenditureGood
{
    public class GarmentSampleExpenditureGoodViewModel
    {
        public Guid Id { get;  set; }
        public string ExpenditureGoodNo { get;  set; }
        public Unit Unit { get;  set; }
        public string ExpenditureType { get;  set; }
        public string RONo { get;  set; }
        public string Article { get;  set; }
        public GarmentComodity Comodity { get;  set; }
        public Buyer Buyer { get;  set; }
        public DateTimeOffset ExpenditureDate { get;  set; }
        public string Invoice { get;  set; }
        public string ContractNo { get;  set; }
        public double Carton { get;  set; }
        public string Description { get;  set; }
        public bool IsReceived { get;  set; }
        public double TotalQuantity { get; set; }
        public double TotalPrice { get; set; }
    }
    public class Unit
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class Buyer
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }


    public class GarmentComodity
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
