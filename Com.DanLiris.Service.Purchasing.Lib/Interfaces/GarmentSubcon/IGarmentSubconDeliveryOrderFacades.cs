using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubconDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderNonPOViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon
{
    public interface IGarmentSubconDeliveryOrderFacades
    {
        Tuple<List<GarmentSubconDeliveryOrder>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentSubconDeliveryOrder ReadById(int id);
        Task<int> Create(GarmentSubconDeliveryOrder m, string user, int clientTimeZoneOffset = 7);
        Task<int> Delete(int id, string user);
        Task<int> Update(int id, GarmentSubconDeliveryOrder newModel, string user, int clientTimeZoneOffset = 7);
        IQueryable<GarmentSubconDeliveryOrder> DOForCustoms(string Keyword, string Filter, string currencycode = null);
        ReadResponse<object> ReadForUnitReceiptNote(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}");
        Tuple<List<GarmentDeliveryOrderReportViewModel>, int> GetReportDO(string no, DateTime? dateFrom, DateTime? dateTo, int page, int size, string Order, int offset);
        MemoryStream GenerateExcelDO(string no, DateTime? dateFrom, DateTime? dateTo, int offset);
    }
}
