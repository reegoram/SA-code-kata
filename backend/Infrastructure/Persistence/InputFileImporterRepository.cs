using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using SA.Application;

namespace SA.Infrastructure.Persistence
{
    public class InputFileImporterRepository : IInputFileImporterRepository
    {
        private ILiteCollection<BsonDocument> _importsCollection;

        public InputFileImporterRepository(LiteDatabase db)
        {
            _importsCollection = db.GetCollection("imports");
        }

        public IDictionary<ImporterStatus, DateTime> Find(Guid processId)
            => _importsCollection.Query()
                                 .Where(x => (Guid)x["ImportId"] == processId)
                                 .Select(x => new 
                                 {
                                     Status = (ImporterStatus) ((int) x["Status"]),
                                     DateTime = (DateTime) x["CreateDate"]
                                 })
                                 .ToList()
                                 .ToDictionary(x => x.Status, x => x.DateTime);

        public IDictionary<string, DateTime> GetAll()
            => _importsCollection.Query()
                                 .Select(x => new
                                 {
                                     ImportId = x["ImportId"].AsGuid,
                                     Status = x["Status"].AsInt32,
                                     CreateDate = x["CreateDate"].AsDateTime
                                 })
                                 .ToList()
                                 .Where(x => x.Status == (int)ImporterStatus.Started)
                                 .ToDictionary(x => x.ImportId.ToString(), x => x.CreateDate);

        public void SaveStatus(Guid processId, ImporterStatus status, string message = null)
        {
            var importStatus = new BsonDocument
            {
                { "ImportId", processId },
                { "Status", (int) status },
                { "CreateDate", DateTime.Now }
            };

            if (!string.IsNullOrEmpty(message))
                importStatus["message"] = message;

            _importsCollection.Insert(importStatus);
        }
    }
}