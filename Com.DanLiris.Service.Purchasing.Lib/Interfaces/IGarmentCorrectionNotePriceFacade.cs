﻿using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentCorrectionNoteModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IGarmentCorrectionNotePriceFacade
    {
        Tuple<List<GarmentCorrectionNote>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        GarmentCorrectionNote ReadById(int id);
        Task<int> Create(GarmentCorrectionNote garmentCorrectionNote);
        MemoryStream GenerateDataExcel(DateTime? dateFrom, DateTime? dateTo, int offset);
    }
}
