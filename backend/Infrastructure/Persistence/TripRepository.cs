using System;
using System.Collections.Generic;
using System.Linq;
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
                              .ToList()
                              .Select(x => {
                                  var trip = x["Trip"];
                                  var driver = trip.AsDocument["Driver"];
                                  var driverName = driver["Name"].AsString;
                                  var startTime = trip["StartTime"];
                                  var endTime = trip["EndTime"];

                                  return new Trip(new Driver(driverName),
                                    new StartTime(startTime["Hour"].AsInt32, startTime["Minutes"].AsInt32),
                                    new EndTime(endTime["Hour"].AsInt32, endTime["Minutes"].AsInt32),
                                    (float) trip["Distance"].AsDouble);
                              })
                              .ToList();
    }
}