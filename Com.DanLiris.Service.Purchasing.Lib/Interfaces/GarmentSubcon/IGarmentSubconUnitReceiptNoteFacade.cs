using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentUnitReceiptNoteViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon
{
    public interface IGarmentSubconUnitReceiptNoteFacade
    {
        ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentSubconUnitReceiptNote ReadById(int id);
        Task<int> Create(GarmentSubconUnitReceiptNote garmentUnitReceiptNote);
        //Task<int> Update(int id, GarmentSubconUnitReceiptNote garmentUnitReceiptNote);
        Task<int> Delete(int id, string deletedReason);
        List<object> ReadForUnitDO(string Keyword = null, string Filter = "{}");
        List<object> ReadForUnitDOMore(string Keyword = null, string Filter = "{}", int size = 50);
    }
}
