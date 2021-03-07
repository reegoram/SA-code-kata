using System;

namespace SA.Domain
{
    public class Driver : IEquatable<Driver>
    {
        private string _name;
        public string Name => _name;
        
        private Driver() { }

        public Driver(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            _name = name;
        }

        // As the only param for the input file is the name
        // I assumed it has to be an unique name, otherwise
        // this implementation for Equals have to be redefined.
        public bool Equals(Driver other) => this.Name == other.Name;
    }
}