using System;
using System.Collections.Generic;
using SA.Domain;

namespace SA.Application
{
    public interface IDriverRepository
    {
        void Add(string driverName, Guid importId);
        bool Exists(string driverName);
        Driver Find(string driverName);
        IList<Driver> GetByProcessId(Guid processId);
    }
}