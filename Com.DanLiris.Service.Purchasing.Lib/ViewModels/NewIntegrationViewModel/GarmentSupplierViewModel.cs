﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel
{
    public class GarmentSupplierViewModel
    {
        public long id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? address { get; set; }
         public string country { get; set; }
        public bool import { get; set; }
        public string? npwp { get; set; }
    }
}
