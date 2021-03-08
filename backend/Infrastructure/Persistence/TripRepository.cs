using System;
using System.Collections.Generic;
using LiteDB;
using SA.Application;
using SA.Domain;

namespace SA.Infrastructure.Persistence
{
    public class TripRepository : ITripRepository
    {
        private ILiteCollection<BsonDocument> _tripCollection;

        public TripRepository(LiteDatabase db)
        {
            _tripCollection = db.GetCollection("trips");
        }

        public void Add(Trip trip, Guid importId)
        {
            var tripImport = new BsonDocument
            {
                { "ImportId", importId },
                { "Trip", BsonMapper.Global.ToDocument(trip) }
            };

            _tripCollection.Insert(tripImport);
        }

        public IList<Trip> Find(Guid processId)
            => _tripCollection.Query()
                              .Where(x => (Guid)x["ImportId"] == processId)
                              .Select(x => x["Trip"].RawValue as Trip)
                              .ToList();
    }
}