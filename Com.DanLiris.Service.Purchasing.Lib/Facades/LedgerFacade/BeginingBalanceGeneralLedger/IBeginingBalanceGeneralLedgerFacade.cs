using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.Ledger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.LedgerFacade.BeginingBalanceGeneralLedger
{
    public interface IBeginingBalanceGeneralLedgerFacade
    {
        ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        Task<int> UploadExcelAsync(List<BeginingBalanceGeneralLedgerModel> listData);
    }
}
