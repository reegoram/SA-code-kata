using System;

namespace SA.Domain
{
    public class Driver 
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
    }
}