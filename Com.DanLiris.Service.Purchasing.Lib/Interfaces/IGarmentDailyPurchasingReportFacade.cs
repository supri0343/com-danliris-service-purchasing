﻿using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDailyPurchasingReportViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentDailyPurchasingReportFacade
    {
        Tuple<List<GarmentDailyPurchasingReportViewModel>, int> GetGDailyPurchasingReport(string unitName, bool supplierType, string supplierName, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, string jnsbc, int offset);
        MemoryStream GenerateExcelGDailyPurchasingReport(string unitName, bool supplierType, string supplierName, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, string jnsbc, int offset);
        MemoryStream GenerateExcelGDailyPurchasingReportMII(string unitName, bool supplierType, string supplierName, DateTime? dateFrom, DateTime? dateTo, DateTime? inputDateFrom, DateTime? inputDateTo, string jnsbc, int offset);

    }
}
