using System;
using System.Collections.Generic;
using ECommerceSystem.Models;
using ECommerceSystem.Logic;

class Program
{
    // create a static instance of the ECommerce system
    static ECommerceSystem.Logic.ECommerceSystem ecommerce = new();

    static void Main()
    {
        bool running = true;

        // application main loop: shows menu and handles user input
        while (running)
        {
            Console.WriteLine("\n--- E-Commerce System ---");
            Console.WriteLine("1. Add Product");
            Console.WriteLine("2. Register Customer");
            Console.WriteLine("3. Place Order");
            Console.WriteLine("4. Generate Invoice");
            Console.WriteLine("5. Search Product");
            Console.WriteLine("6. View All Products");
            Console.WriteLine("7. List Orders by Customer");
            Console.WriteLine("8. Save Data");
            Console.WriteLine("9. Load Data");
            Console.WriteLine("0. Exit");
            Console.Write("Selection: ");

            // handle menu selection
            switch (Console.ReadLine())
            {
                case "1":
                    AddProduct(); break;
                case "2":
                    RegisterCustomer(); break;
                case "3":
                    PlaceOrder(); break;
                case "4":
                    GenerateInvoice(); break;
                case "5":
                    SearchProduct(); break;
                case "6":
                    ViewAllProducts(); break;
                case "7":
                    ListOrders(); break;
                case "8":
                    ecommerce.SaveData("data.json");
                    Console.WriteLine(">> Data saved.");
                    break;
                case "9":
                    ecommerce.LoadData("data.json");
                    Console.WriteLine(">> Data loaded.");
                    break;
                case "0":
                    running = false; break;
                default:
                    Console.WriteLine("[Error] Invalid choice. Please try again.");
                    break;
            }
        }
    }

    // prompt user for product info and add it to inventory
    static void AddProduct()
    {
        Console.WriteLine("\n--- Add New Product ---");

        Console.Write("Product Name: ");
        string name = (Console.ReadLine() ?? string.Empty).Trim();

        Console.Write("Category: ");
        string category = (Console.ReadLine() ?? string.Empty).Trim();

        // validate stock input
        int stock;
        while (true)
        {
            Console.Write("Stock Quantity: ");
            if (int.TryParse(Console.ReadLine(), out stock) && stock >= 0)
                break;
            Console.WriteLine("[Error] Please enter a non-negative number for stock.");
        }

        // validate price input
        decimal price;
        while (true)
        {
            Console.Write("Price: $");
            if (decimal.TryParse(Console.ReadLine(), out price) && price >= 0)
                break;
            Console.WriteLine("[Error] Please enter a valid, non-negative price.");
        }

        // add product to the system
        ecommerce.AddProduct(new Product(name, category, stock, price));
        Console.WriteLine($"\n✅ Product '{name}' added successfully!");
    }

    // register a new customer by name
    static void RegisterCustomer()
    {
        Console.WriteLine("\n--- Register New Customer ---");

        string name;
        while (true)
        {
            Console.Write("Customer Name: ");
            name = (Console.ReadLine() ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(name)) break;
            Console.WriteLine("[Error] Name cannot be empty.");
        }

        var customer = new Customer(name);
        ecommerce.RegisterCustomer(customer);

        Console.WriteLine($"\n✅ Customer registered successfully!");
    }

    // create an order for a given customer
    static void PlaceOrder()
    {
        Console.WriteLine("\n--- Place a New Order ---");

        Console.Write("Enter Customer ID or Name: ");
        string input = (Console.ReadLine() ?? string.Empty).Trim();

        Customer customer = null;

        // try to locate customer by ID or name
        if (Guid.TryParse(input, out Guid id))
        {
            customer = ecommerce.Customers.Find(c => c.Id == id);
        }
        else
        {
            var matches = ecommerce.Customers
                .FindAll(c => c.Name.Equals(input, StringComparison.OrdinalIgnoreCase));

            if (matches.Count == 0)
            {
                Console.WriteLine("[Error] No customer found.");
                return;
            }
            else if (matches.Count == 1)
            {
                customer = matches[0];
            }
            else
            {
                Console.WriteLine("\nMultiple customers found:");
                for (int i = 0; i < matches.Count; i++)
                    Console.WriteLine($"  {i + 1}. {matches[i].Name} (ID: {matches[i].Id})");

                Console.Write("Select customer number: ");
                if (int.TryParse(Console.ReadLine(), out int choice) &&
                    choice >= 1 && choice <= matches.Count)
                {
                    customer = matches[choice - 1];
                }
                else
                {
                    Console.WriteLine("[Error] Invalid selection.");
                    return;
                }
            }
        }

        if (customer == null)
        {
            Console.WriteLine("[Error] Customer not found.");
            return;
        }

        // collect product names and quantities
        var productOrders = new Dictionary<string, int>();
        Console.WriteLine("Enter products for the order. Type 'done' to finish.\n");

        while (true)
        {
            Console.Write("Product name (or 'done'): ");
            string pname = (Console.ReadLine() ?? string.Empty).Trim();

            if (pname.ToLower() == "done") break;

            var matches = ecommerce.Products
                .FindAll(p => p.Name.Contains(pname, StringComparison.OrdinalIgnoreCase));

            if (matches.Count == 0)
            {
                Console.WriteLine($"[Error] Product '{pname}' not found.");
                continue;
            }
            else if (matches.Count > 1)
            {
                Console.WriteLine($"[!] Multiple matches for '{pname}':");
                for (int i = 0; i < matches.Count; i++)
                    Console.WriteLine($"  {i + 1}. {matches[i].Name}");

                Console.Write("Select product number: ");
                if (!int.TryParse(Console.ReadLine(), out int choice) ||
                    choice < 1 || choice > matches.Count)
                {
                    Console.WriteLine("[Error] Invalid selection.");
                    continue;
                }

                pname = matches[choice - 1].Name;
            }
            else
            {
                pname = matches[0].Name;
            }

            Console.Write("Quantity: ");
            if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
            {
                Console.WriteLine("[Error] Invalid quantity.");
                continue;
            }

            if (productOrders.ContainsKey(pname))
                productOrders[pname] += qty;
            else
                productOrders[pname] = qty;
        }

        if (productOrders.Count == 0)
        {
            Console.WriteLine("[Error] No products were added.");
            return;
        }

        // attempt to place the order
        try
        {
            var order = ecommerce.PlaceOrder(customer.Id, productOrders);
            Console.WriteLine($"\n✅ Order placed successfully!");

            // show summary
            Console.WriteLine("\n--- Order Summary ---");
            decimal total = 0;
            foreach (var item in order.Items)
            {
                decimal lineTotal = item.Value * item.Key.Price;
                total += lineTotal;
                Console.WriteLine($"{item.Key.Name,-25} {item.Value,5} ${item.Key.Price,12:F2} ${lineTotal,12:F2}");
            }

            Console.WriteLine($"{"TOTAL:",44} ${total,12:F2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n{ex.Message}");
        }
    }

    // display invoice for an order
    static void GenerateInvoice()
    {
        Console.WriteLine("\n=== 🧾 Generate Invoice ===");

        Console.Write("Enter Order ID: ");
        if (!Guid.TryParse(Console.ReadLine(), out Guid id))
        {
            Console.WriteLine("[Error] Invalid order ID format.");
            return;
        }

        var order = ecommerce.Orders.Find(o => o.OrderId == id);
        if (order == null)
        {
            Console.WriteLine("[Error] Order not found!");
            return;
        }

        // display order invoice
        Console.WriteLine("\n--- Invoice Summary ---");
        Console.WriteLine($"Order ID : {order.OrderId}");
        Console.WriteLine($"Customer : {order.Customer.Name}");
        Console.WriteLine($"Date     : {order.Date}");

        decimal total = 0;
        foreach (var item in order.Items)
        {
            decimal lineTotal = item.Value * item.Key.Price;
            total += lineTotal;
            Console.WriteLine($"{item.Key.Name,-25} {item.Value,5} ${item.Key.Price,12:F2} ${lineTotal,12:F2}");
        }

        Console.WriteLine($"{"TOTAL:",44} ${total,12:F2}");
    }

    // search for products by name or category
    static void SearchProduct()
    {
        Console.WriteLine("\n--- Product Search ---");

        Console.Write("Enter keyword (name or category): ");
        string keyword = (Console.ReadLine() ?? string.Empty).Trim();

        var results = ecommerce.SearchProducts(keyword);
        if (results.Count == 0)
        {
            Console.WriteLine("No matching products found.");
            return;
        }

        Console.WriteLine($"\n🔎 Found {results.Count} product{(results.Count > 1 ? "s" : "")}:");
        foreach (var p in results)
            Console.WriteLine($"{p.Name,-20} {p.Category,-15} ${p.Price,10:F2} {p.Stock,8}");
    }

    // list all products in the system
    static void ViewAllProducts()
    {
        Console.WriteLine("\n--- All Available Products ---");

        var products = ecommerce.Products;
        if (products.Count == 0)
        {
            Console.WriteLine("No products in inventory.");
            return;
        }

        foreach (var p in products)
            Console.WriteLine($"{p.Name,-20} {p.Category,-15} ${p.Price,10:F2} {p.Stock,8}");
    }

    // display all orders for a specific customer
    static void ListOrders()
    {
        Console.WriteLine("\n--- View Customer Orders ---");

        Console.Write("Enter Customer ID: ");
        if (!Guid.TryParse(Console.ReadLine(), out Guid id))
        {
            Console.WriteLine("[Error] Invalid customer ID format.");
            return;
        }

        var orders = ecommerce.GetOrdersByCustomer(id);
        if (orders.Count == 0)
        {
            Console.WriteLine("No orders found.");
            return;
        }

        Console.WriteLine($"\n🧾 {orders.Count} order{(orders.Count > 1 ? "s" : "")} found:");
        foreach (var order in orders)
            Console.WriteLine($"{order.OrderId,-42} {order.Date,30}");
    }
}
