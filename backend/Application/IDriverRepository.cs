using System;
using System.Collections.Generic;
using SA.Domain;

namespace SA.Application
{
    public interface IDriverRepository
    {
        void Add(string driverName, Guid importId);
        bool Exists(string driverName, Guid importId);
        Driver Find(string driverName, Guid importId);
        IList<Driver> GetByProcessId(Guid processId);
    }
}