using System;
using System.Collections.Generic;

namespace ECommerceSystem.Models
{
    public class Customer
    {
        public Guid Id { get; private set; } // unique customer id
        public string Name { get; set; }     // customer name

        public Customer(string name)
        {
            // check if name is empty or just whitespace
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("[Error] Customer name cannot be empty!");

            // generate a new unique id
            Id = Guid.NewGuid();

            // assign the provided name
            Name = name;
        }
    }
}
