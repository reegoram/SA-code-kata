using SA.Domain;

namespace SA.Application
{
    public interface IDriverRepository
    {
        void Add(string driverName);
        bool Exists(string driverName);
        Driver Find(string driverName);
    }
}