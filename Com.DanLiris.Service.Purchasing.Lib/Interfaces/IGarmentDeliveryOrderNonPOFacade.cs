using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderNonPOModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentDeliveryOrderNonPOFacade
    {
        Tuple<List<GarmentDeliveryOrderNonPO>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentDeliveryOrderNonPO ReadById(int id);
        Task<int> Create(GarmentDeliveryOrderNonPO m, string user, int clientTimeZoneOffset = 7);
        Task<int> Delete(int id, string user);
        Task<int> Update(int id, GarmentDeliveryOrderNonPO m, string user, int clientTimeZoneOffset = 7);
        Task<int> SetIsSubconInvoice(string doNos, bool isSubconInvoice);
        IQueryable<GarmentDeliveryOrderNonPO> DOForCustoms(string Keyword = null, string Filter = "{}", string BillNo = null);
    }
}
