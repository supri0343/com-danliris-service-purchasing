﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.BudgetCashflowService.PdfGenerator
{
    public interface IBudgetCashflowDivisionPdf
    {
        MemoryStream Generate(int divisionId, DateTimeOffset dueDate);
    }
}
