using System;

namespace SA.Application
{
    public interface IInputFileImporterRepository
    {
        void SaveComplete(Guid processId, DateTime time);
        void SaveComputing(Guid processId, DateTime time);
        void SaveError(Guid processId, DateTime time, string error);
        void SaveStart(Guid processId, DateTime time);
    }
}