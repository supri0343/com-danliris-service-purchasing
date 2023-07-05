using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentUnitDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentUnitDeliveryOrderModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon
{
    public interface IGarmentSubconUnitDeliveryOrderFacade
    {
        ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentSubconUnitDeliveryOrder ReadById(int id);
        Task<int> Create(GarmentSubconUnitDeliveryOrder garmentUnitDeliveryOrder);
        Task<int> Update(int id, GarmentSubconUnitDeliveryOrder garmentUnitDeliveryOrder);
        Task<int> Delete(int id);
        ReadResponse<object> ReadForUnitExpenditureNote(int Page = 1, int Size = 10, string Order = "{}", string Keyword = null, string Filter = "{}");

        //GarmentUnitDeliveryOrderItem ReadItemById(int id);
    }
}
