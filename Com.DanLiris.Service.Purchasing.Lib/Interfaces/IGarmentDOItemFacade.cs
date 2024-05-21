using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitReceiptNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels.DOItems;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentDOItemFacade
    {
        List<object> ReadForUnitDO(string Keyword = null, string Filter = "{}");
        List<object> ReadForUnitDOMore(string Keyword = null, string Filter = "{}", int size = 100);
        List<DOItemsViewModels> GetByPO(string productcode, string po, string unitcode);
        GarmentDOItems ReadById(int id);
        Task<int> Update(int id, DOItemsRackingViewModels viewModels);
        Task<List<StellingEndViewModels>> GetStellingQuery(int id, int offset);
        MemoryStream GenerateExcel(string productcode, string po, string unitcode);
        MemoryStream GeneratePdf(List<StellingEndViewModels> stellingEndViewModels);
    }
}
