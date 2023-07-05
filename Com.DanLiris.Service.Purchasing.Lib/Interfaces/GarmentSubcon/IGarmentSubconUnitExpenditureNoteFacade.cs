using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentUnitExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentUnitExpenditureNoteViewModel;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon
{
    public interface IGarmentSubconUnitExpenditureNoteFacade
    {
        ReadResponse<object> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentSubconUnitExpenditureNoteViewModel ReadById(int id);
		Task<int> Create(GarmentSubconUnitExpenditureNote GarmentSubconUnitExpenditureNote);
        Task<int> Update(int id, GarmentSubconUnitExpenditureNote GarmentSubconUnitExpenditureNote);
        Task<int> Delete(int id);
       
    }
}
