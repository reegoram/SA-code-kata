using System;

namespace SA.Application
{
    public interface IInputFileProcessor
    {
        void NewBatch(string data, Guid processId);
    }
}