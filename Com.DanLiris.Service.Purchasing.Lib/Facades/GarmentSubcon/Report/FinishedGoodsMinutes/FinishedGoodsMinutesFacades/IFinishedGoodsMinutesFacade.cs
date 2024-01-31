using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSubcon.Report.FinishedGoodsMinutes.FinishedGoodsMinutesFacades
{
    public interface IFinishedGoodsMinutesFacade
    {
        Task<List<FinishedGoodsMinutesVM>> GetByRONo(List<string> Rono);
    }
}
