using System;
using System.Collections.Generic;
using LiteDB;
using SA.Application;
using SA.Domain;

namespace SA.Infrastructure.Persistence
{
    public class TripSummaryRepository : ITripSummaryRepository
    {
        private ILiteCollection<TripSummary> _tripSummaryCollection;

        public TripSummaryRepository(LiteDatabase db)
        {
            _tripSummaryCollection = db.GetCollection<TripSummary>("tripSummaries");
        }

        public void AddRange(IEnumerable<TripSummary> tripsSummaryPerDriver)
        {
            _tripSummaryCollection.InsertBulk(tripsSummaryPerDriver);
        }

        public IEnumerable<TripSummary> GetAllByProcessId(Guid processId)
            => _tripSummaryCollection.Query()
                                     .Where(x => x.ImportId == processId)
                                     .ToList();
    }
}