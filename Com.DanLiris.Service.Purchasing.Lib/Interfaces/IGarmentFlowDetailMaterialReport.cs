using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentFlowDetailMaterialReport
    {
        MemoryStream GenerateExcel(string category, string productcode, string categoryname, string unit, string unitname, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, DateTimeOffset? dateFromCreate, DateTimeOffset? dateToCreate, int offset);
        MemoryStream GenerateExcelForUnit(string category, string productcode, string categoryname, string unit, string unitname, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, DateTimeOffset? dateFromCreate, DateTimeOffset? dateToCreate, int offset);
        MemoryStream GenerateExcelForMII(string category, string productcode, string categoryname, string unit, string unitname, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, DateTimeOffset? dateFromCreate, DateTimeOffset? dateToCreate, int offset);

        // MemoryStream GenerateExcelEPOOverBudget(string epono, string unit, string supplier, string status, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset);
        Tuple<List<GarmentFlowDetailMaterialViewModel>, int> GetReport(string category, string productcode, string unit, DateTimeOffset? DateFrom, DateTimeOffset? DateTo, DateTimeOffset? DateFromCreate, DateTimeOffset? DateToCreate, int offset, string order, int page, int size);
    }
}
