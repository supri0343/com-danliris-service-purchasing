﻿using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentSubcon.GarmentSubconCustomOut;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentSubcon.GarmentSubconCustomOutVM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentSubcon.GarmentSubconCustomOutFacades.GarmentSubconCustomOutFacade;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces.GarmentSubcon
{
    public interface IGarmentSubconCustomOutFacade
    {
        Tuple<List<GarmentSubconCustomOut>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentSubconCustomOut ReadById(int id);
        Task<int> Create(GarmentSubconCustomOut m, string user, int clientTimeZoneOffset = 7);
        Task<int> Delete(int id, string user);
        Task<int> Update(int id, GarmentSubconCustomOut newModel, string user, int clientTimeZoneOffset = 7);
        Task<List<GetByLocalSalesNoteVM>> GetByLocalSalesNote(List<string> noteNo);
    }
}
