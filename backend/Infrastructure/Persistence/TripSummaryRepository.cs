using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using SA.Application;
using SA.Domain;

namespace SA.Infrastructure.Persistence
{
    public class TripSummaryRepository : ITripSummaryRepository
    {
        private ILiteCollection<BsonDocument> _tripSummaryCollection;

        public TripSummaryRepository(LiteDatabase db)
        {
            _tripSummaryCollection = db.GetCollection<BsonDocument>("tripSummaries");
        }

        public void AddRange(IEnumerable<TripSummary> tripsSummaryPerDriver)
        {
            _tripSummaryCollection.InsertBulk(
                tripsSummaryPerDriver.Select(x => 
                    BsonMapper.Global.Serialize(x).AsDocument));
        }

        public IEnumerable<TripSummary> GetAllByProcessId(Guid processId)
            => _tripSummaryCollection.Query()
                                     .Where(x => x["ImportId"].AsGuid == processId)
                                     .ToList()
                                     .Select(x => {
                                         var driver = x["Driver"];

                                         return new TripSummary(
                                            x["ImportId"].AsGuid,
                                            new Driver(driver["Name"]),
                                            x["Miles"].AsInt32,
                                            x["MilesPerHour"]);
                                     });
    }
}