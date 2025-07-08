using System;

namespace ECommerceSystem.Models
{
    public class Product
    {
        public string Name { get; set; }         // name of the product
        public string Category { get; set; }     // category the product belongs to
        public int Stock { get; private set; }   // current stock quantity (read-only outside)
        public decimal Price { get; set; }       // price per unit

        public Product(string name, string category, int stock, decimal price)
        {
            // set initial values from constructor parameters
            Name = name;
            Category = category;
            Stock = stock;
            Price = price;
        }

        public void DecreaseStock(int quantity)
        {
            // check if there's enough stock to fulfill the request
            if (quantity > Stock)
                throw new InvalidOperationException("[Error] Not enough stock!");

            // subtract the quantity from current stock
            Stock -= quantity;
        }

        public void IncreaseStock(int quantity)
        {
            // add the quantity to the current stock
            Stock += quantity;
        }
    }
}
