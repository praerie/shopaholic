using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceSystem.Models
{
    public class Order
    {
        public Guid OrderId { get; private set; }
        public Customer Customer { get; private set; }
        public Dictionary<Product, int> Items { get; private set; }
        public DateTime Date { get; private set; }

        public Order(Customer customer)
        {
            OrderId = Guid.NewGuid();
            Customer = customer;
            Items = new Dictionary<Product, int>();
            Date = DateTime.Now;
        }

        public void AddProduct(Product product, int quantity)
        {
            // if the product is already in the order, increase the quantity
            if (Items.ContainsKey(product))
                Items[product] += quantity;
            else
                // otherwise, add the product with the given quantity
                Items[product] = quantity;
        }

        public string GenerateInvoice()
        {
            var sb = new StringBuilder();
            // start invoice with basic order info
            sb.AppendLine($"Invoice for Order {OrderId}");
            sb.AppendLine($"Customer: {Customer.Name}");
            sb.AppendLine($"Date: {Date}");

            decimal total = 0;

            // list each product with quantity and total price
            foreach (var item in Items)
            {
                sb.AppendLine($"{item.Key.Name} x{item.Value} - ${item.Key.Price * item.Value}");
                total += item.Key.Price * item.Value;
            }

            // add the total at the end of the invoice
            sb.AppendLine($"Total: ${total}");
            return sb.ToString();
        }
    }
}
