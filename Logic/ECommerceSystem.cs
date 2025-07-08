using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ECommerceSystem.Models;

namespace ECommerceSystem.Logic
{
    public class ECommerceSystem
    {
        // store lists of products, customers, and orders
        public List<Product> Products { get; private set; } = new();
        public List<Customer> Customers { get; private set; } = new();
        public List<Order> Orders { get; private set; } = new();

        // add a new product to the system
        public void AddProduct(Product product) => Products.Add(product);

        // register a new customer
        public void RegisterCustomer(Customer customer) => Customers.Add(customer);

        // place an order for a customer with a dictionary of product names and quantities
        public Order PlaceOrder(Guid customerId, Dictionary<string, int> productOrders)
        {
            var customer = Customers.FirstOrDefault(c => c.Id == customerId)
                ?? throw new ArgumentException("[Error] Customer not found!");

            var order = new Order(customer);

            foreach (var entry in productOrders)
            {
                var product = Products.FirstOrDefault(p => p.Name.Equals(entry.Key, StringComparison.OrdinalIgnoreCase));
                if (product == null)
                    throw new ArgumentException($"[Error] Product {entry.Key} not found!");
                if (product.Stock < entry.Value)
                    throw new InvalidOperationException($"[Error] Not enough stock for {product.Name}!");

                product.DecreaseStock(entry.Value);
                order.AddProduct(product, entry.Value);
            }

            Orders.Add(order);
            return order;
        }

        // search for products by keyword in name or category
        public List<Product> SearchProducts(string keyword)
        {
            return Products
                .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            p.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // get all orders placed by a specific customer
        public List<Order> GetOrdersByCustomer(Guid customerId)
        {
            return Orders.Where(o => o.Customer.Id == customerId).ToList();
        }

        // save product, customer, and order data to a file in json format
        public void SaveData(string filename)
        {
            var data = new
            {
                Products,
                Customers,
                Orders
            };
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(filename, json);
        }

        // load data from a json file into the system
        public void LoadData(string filename)
        {
            if (!File.Exists(filename)) return;

            var json = File.ReadAllText(filename);
            var data = JsonSerializer.Deserialize<SerializedData>(json);

            Products = data.Products;
            Customers = data.Customers;
            Orders = data.Orders;
        }

        // class to help deserialize json data structure
        private class SerializedData
        {
            public List<Product> Products { get; set; }
            public List<Customer> Customers { get; set; }
            public List<Order> Orders { get; set; }
        }
    }
}
