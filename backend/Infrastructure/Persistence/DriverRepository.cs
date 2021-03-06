using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using SA.Application;
using SA.Domain;

namespace SA.Infrastructure.Persistence
{
    public class DriverRepository : IDriverRepository
    {
        private ILiteCollection<BsonDocument> _driverCollection;

        public DriverRepository(LiteDatabase db)
        {
            _driverCollection = db.GetCollection("drivers");
        }

        public void Add(string driverName, Guid importId)
        {
            var driver = new BsonDocument();
            driver["ImportId"] = importId;
            driver["DriverName"] = driverName;
            
            _driverCollection.Insert(driver);
        }

        public bool Exists(string driverName, Guid processId) 
            => _driverCollection.Exists(x => x["DriverName"] == driverName && x["ImportId"].AsGuid == processId);

        public Driver Find(string driverName, Guid processId)
            => _driverCollection.Query()
                                .Where(x => x["DriverName"] == driverName && x["ImportId"].AsGuid == processId)
                                .Select(x => x["DriverName"].AsString)
                                .ToList()
                                .Select(x => new Driver(x))
                                .FirstOrDefault();

        public IList<Driver> GetByProcessId(Guid processId)
            => _driverCollection.Query()
                                .Where(x => (Guid)x["ImportId"] == processId)
                                .Select(x => x["DriverName"].AsString)
                                .ToList()
                                .Select(x => new Driver(x))
                                .ToList();
    }
}