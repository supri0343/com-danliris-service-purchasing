﻿using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentReports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IROFeatureFacade
    {
        Tuple<List<ROFeatureViewModel>, int> GetROReport(int offset, string RO, int page, int size, string Order);
    }
}
