using System;
using System.Collections.Generic;

namespace SA.Application
{
    public interface IInputFileImporterRepository
    {
        IDictionary<string, DateTime> GetAll();
        IDictionary<ImporterStatus, DateTime> Find(Guid processId);
        void SaveStatus(Guid processId, ImporterStatus status, string message = null);
    }
}