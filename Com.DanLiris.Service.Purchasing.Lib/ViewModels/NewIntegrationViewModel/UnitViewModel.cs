﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel
{
    public class UnitViewModel
    {
        public string? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DivisionViewModel Division { get; set; }
    }
}
