using Com.DanLiris.Service.Purchasing.Lib.ViewModels.LogHistoryViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface ILogHistoryFacades
    {
        Task<List<LogHistoryViewModel>> GetReportQuery(DateTime dateFrom, DateTime dateTo);
        void Create(string division, string activity);
    }
}
