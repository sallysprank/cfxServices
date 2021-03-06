﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QBODataCollect.Models;

namespace QBODataCollect.Repositories.Interfaces
{
    public interface IQBOAccessRepository
    {
        QBOAccess GetById(int sId);
        bool UpdateQBOAccess(int id, string accessToken, string refreshToken, QBOAccess ourQBOAccess);
    }
}
