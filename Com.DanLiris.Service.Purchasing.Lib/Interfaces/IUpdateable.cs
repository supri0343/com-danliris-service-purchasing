﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Interfaces
{
    public interface IUpdateable<TModel>
    {
        Task<int> Update(int id, TModel model);
    }
}
