using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task
{
    interface ISearchable
    {
        List<Product> SearchProducts(Func<Product, bool> criteria);
    }

    class Product
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public double Rating { get; set; }

        public Product(string name, double price, string description, string category, double rating)
        {
            Name = name;
            Price = price;
            Description = description;
            Category = category;
            Rating = rating;
        }

        public override string ToString()
        {
            return $"{Name} - {Category}: {Price} грн (Рейтинг: {Rating:F1})\nОпис: {Description}";
        }
    }

    class User
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public List<Order> PurchaseHistory { get; set; }

        public User(string login, string password)
        {
            Login = login;
            Password = password;
            PurchaseHistory = new List<Order>();
        }
    }

    class Order
    {
        public List<(Product Product, int Quantity)> Items { get; set; }
        public double TotalAmount { get; set; }
        public string Status { get; set; }

        public Order()
        {
            Items = new List<(Product, int)>();
            Status = "Новий";
        }

        public void CalculateTotalAmount()
        {
            TotalAmount = Items.Sum(item => item.Product.Price * item.Quantity);
        }

        public override string ToString()
        {
            string itemDetails = string.Join("\n", Items.Select(item =>
                $"{item.Product.Name} x{item.Quantity} = {item.Product.Price * item.Quantity} грн"));
            return $"Замовлення:\n{itemDetails}\nЗагальна сума: {TotalAmount} грн (Статус: {Status})";
        }
    }

    class Store : ISearchable
    {
        private List<Product> products = new();
        private List<User> users = new();
        private List<Order> orders = new();

        public void AddProduct(Product product) => products.Add(product);

        public void AddUser(User user) => users.Add(user);

        public User AuthenticateUser(string login, string password)
        {
            var user = users.FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user == null)
                throw new Exception("Невірний логін або пароль.");
            return user;
        }

        public List<Product> SearchProducts(Func<Product, bool> criteria)
        {
            return products.Where(criteria).ToList();
        }

        public void PlaceOrder(User user, Order order)
        {
            order.CalculateTotalAmount();
            orders.Add(order);
            user.PurchaseHistory.Add(order);
            Console.WriteLine("Замовлення успішно оформлено.");
        }
    }

    class Program
    {
        static void Main()
        {
            // Створення магазину, користувачів і товарів
            var store = new Store();

            store.AddProduct(new Product("Ноутбук", 20000, "Потужний ноутбук для роботи", "Електроніка", 4.8));
            store.AddProduct(new Product("Смартфон", 15000, "Сучасний смартфон", "Електроніка", 4.6));
            store.AddProduct(new Product("Навушники", 2000, "Безпровідні навушники", "Аксесуари", 4.2));
            store.AddProduct(new Product("Чайник", 800, "Електрочайник", "Побутова техніка", 4.0));
            store.AddProduct(new Product("Телевізор", 12000, "Сучасний 4K телевізор", "Електроніка", 4.9));

            store.AddUser(new User("admin", "1234"));

            Console.WriteLine("Ласкаво просимо до магазину!");
            Console.Write("Введіть логін: ");
            string login = Console.ReadLine();
            Console.Write("Введіть пароль: ");
            string password = Console.ReadLine();

            try
            {
                var user = store.AuthenticateUser(login, password);

                while (true)
                {
                    Console.WriteLine("\nМеню:");
                    Console.WriteLine("1. Переглянути товари");
                    Console.WriteLine("2. Пошук товарів");
                    Console.WriteLine("3. Оформити замовлення");
                    Console.WriteLine("4. Переглянути історію покупок");
                    Console.WriteLine("5. Вихід");
                    Console.Write("Ваш вибір: ");
                    int choice = int.Parse(Console.ReadLine());

                    switch (choice)
                    {
                        case 1:
                            Console.WriteLine("\nДоступні товари:");
                            foreach (var product in store.SearchProducts(p => true))
                                Console.WriteLine(product);
                            break;

                        case 2:
                            Console.WriteLine("\n1. Пошук за категорією");
                            Console.WriteLine("2. Пошук за ціною (до)");
                            Console.WriteLine("3. Пошук за рейтингом (від)");
                            Console.Write("Ваш вибір: ");
                            int searchChoice = int.Parse(Console.ReadLine());

                            if (searchChoice == 1)
                            {
                                Console.Write("Введіть категорію: ");
                                string category = Console.ReadLine();
                                var result = store.SearchProducts(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
                                Console.WriteLine("\nРезультати пошуку:");
                                foreach (var product in result)
                                    Console.WriteLine(product);
                            }
                            else if (searchChoice == 2)
                            {
                                Console.Write("Введіть максимальну ціну: ");
                                double maxPrice = double.Parse(Console.ReadLine());
                                var result = store.SearchProducts(p => p.Price <= maxPrice);
                                Console.WriteLine("\nРезультати пошуку:");
                                foreach (var product in result)
                                    Console.WriteLine(product);
                            }
                            else if (searchChoice == 3)
                            {
                                Console.Write("Введіть мінімальний рейтинг: ");
                                double minRating = double.Parse(Console.ReadLine());
                                var result = store.SearchProducts(p => p.Rating >= minRating);
                                Console.WriteLine("\nРезультати пошуку:");
                                foreach (var product in result)
                                    Console.WriteLine(product);
                            }
                            break;

                        case 3:
                            var order = new Order();
                            Console.WriteLine("\nВведіть назви товарів для додавання в замовлення (введіть 'готово' для завершення):");

                            while (true)
                            {
                                Console.Write("Товар: ");
                                string productName = Console.ReadLine();
                                if (productName.ToLower() == "готово") break;

                                var product = store.SearchProducts(p => p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                                if (product != null)
                                {
                                    Console.Write("Кількість: ");
                                    int quantity = int.Parse(Console.ReadLine());
                                    order.Items.Add((product, quantity));
                                }
                                else
                                {
                                    Console.WriteLine("Товар не знайдено.");
                                }
                            }

                            store.PlaceOrder(user, order);
                            break;

                        case 4:
                            Console.WriteLine("\nІсторія покупок:");
                            foreach (var o in user.PurchaseHistory)
                                Console.WriteLine(o);
                            break;

                        case 5:
                            Console.WriteLine("Дякуємо за використання нашого магазину!");
                            return;

                        default:
                            Console.WriteLine("Неправильний вибір. Спробуйте ще раз.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }
    }
}