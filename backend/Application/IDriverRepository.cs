namespace SA.Application
{
    public interface IDriverRepository
    {
        void Add(string driverName);
        bool Exists(string driverName);
    }
}