using System;
using System.Collections.Generic;
using System.Text;
using static Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentReports.BCForAvalFacade;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IBCForAval 
    {
        List<ViewModel> GetQuery(string buk);
    }
}
