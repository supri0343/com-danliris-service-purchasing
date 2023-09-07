using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentImportPurchasingJournalReportFacade
    {
         List<GarmentImportPurchasingJournalReportViewModel> GetReportData(DateTime? dateFrom, DateTime? dateTo, int offset);
         MemoryStream GenerateExcel(DateTime? dateFrom, DateTime? dateTo, int offset);
    }
}
