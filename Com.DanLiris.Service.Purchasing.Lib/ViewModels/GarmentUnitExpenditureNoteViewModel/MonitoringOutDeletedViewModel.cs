using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitExpenditureNoteViewModel
{
  public  class MonitoringOutDeletedViewModel
    {
        public long id { get; set; }
        public long ueNid { get; set; }
        public string? deletedByEx { get; set; }
        public string? uenNoEx { get; set; }
        public string? unitEx { get; set; }
        public string? unitDoNoEx { get; set; }
        public string? expenditureType { get; set; }
        public string? expenditureToEx { get; set; }
        public string? roNoEx { get; set; }
        public string? poSerialNumber { get; set; }
        public string? productNameEx { get; set; }
        public double quantityEx { get; set; }
        public string? uomUnitEx { get; set; }
        public string? deletedUtcEx { get; set; }
        public string? expenditureDate { get; set; }
        //public DateTimeOffset deletedUtcEx { get; set; }
        //public DateTimeOffset expenditureDate { get; set; }
    }
}
