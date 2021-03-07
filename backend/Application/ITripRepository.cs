using SA.Domain;
using System;
using System.Collections.Generic;

namespace SA.Application
{
    public interface ITripRepository
    {
        void Add(Trip trip, Guid importId);
        IList<Trip> Find(Guid processId);
    }
}