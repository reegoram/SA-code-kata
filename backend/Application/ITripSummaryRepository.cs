using System;
using System.Collections.Generic;
using SA.Domain;

namespace SA.Application
{
    public interface ITripSummaryRepository
    {
        void AddRange(IEnumerable<TripSummary> tripsSummaryPerDriver);
        IEnumerable<TripSummary> GetAllByProcessId(Guid processId);
    }
}