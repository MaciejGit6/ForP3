using System.Linq;

namespace Task3;

public class Program
{
    public static void Main(string[] args)
    {
        Database database = Database.GetInstance();

        // Question 1
        // List all customers who do not have a loyalty card.
        // Sort ascending by last name, then by first name.
        Console.WriteLine("****** Question 1 ******");
        Console.WriteLine("Provided:");

        // customers that have a card
        var cardOwners = database.LoyaltyCards.Select(lc => lc.CustomerId);

        var noCard =
            from c in database.Customers
            where !cardOwners.Contains(c.CustomerId)
            orderby c.LastName, c.FirstName
            select c;

        foreach (var c in noCard)
            Console.WriteLine(c.LastName + " " + c.FirstName);

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Bąk Magdalena");
        Console.WriteLine("Bąk Marek");
        Console.WriteLine("Wiśniewski Artur");

        // Question 2
        // Find customers older than 29 years (as of the reference year 2020).
        // Determine the birth year from the PESEL number: the first two digits are the year,
        // the next two the month (a month value above 20 means a birth year in the 2000s).
        // Sort ascending by birth year (oldest first) and print each customer's age.
        Console.WriteLine();
        Console.WriteLine("****** Question 2 ******");
        Console.WriteLine("Provided:");

        const int referenceYear = 2020;

        // e.g. 85031512345 -> 1985, 02231512345 -> 2002
        static int BirthYear(string pesel)
        {
            int yy = int.Parse(pesel.Substring(0, 2));
            int mm = int.Parse(pesel.Substring(2, 2));
            // month > 20 means born after 2000
            return (mm > 20 ? 2000 : 1900) + yy;
        }

        var olderThan29 =
            from c in database.Customers
            let birthYear = BirthYear(c.PESEL)
            let age = referenceYear - birthYear
            where age > 29
            orderby birthYear
            select new { Customer = c, Age = age };

        foreach (var x in olderThan29)
            Console.WriteLine(x.Customer.LastName + " " + x.Customer.FirstName + " " + x.Age);

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Kowalski Jan 117");
        Console.WriteLine("Marciniak Jerzy 35");
        Console.WriteLine("Antoniewski Michał 32");

        // Question 3
        // Find the 3 longest-standing customers, i.e. those whose first order was placed earliest.
        // Consider only customers who have placed at least one order.
        // Sort ascending by the date of the first order and print that date (dd.MM.yyyy).
        Console.WriteLine();
        Console.WriteLine("****** Question 3 ******");
        Console.WriteLine("Provided:");

        // first order date per customer
        // (people without orders just don't show up here)
        var firstOrders =
            from o in database.Orders
            group o.Date by o.CustomerId into g
            select new { CustomerId = g.Key, FirstOrder = g.Min() };

        var longest =
            (from f in firstOrders
             join c in database.Customers on f.CustomerId equals c.CustomerId
             orderby f.FirstOrder
             select new { Customer = c, f.FirstOrder })
            .Take(3);

        foreach (var x in longest)
            Console.WriteLine(x.Customer.LastName + " " + x.Customer.FirstName + " " + x.FirstOrder.ToString("dd.MM.yyyy"));

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Nowak Andrzej 02.01.2017");
        Console.WriteLine("Antoniewski Michał 22.03.2017");
        Console.WriteLine("Kowalska Aneta 12.03.2018");

        // Question 4
        // Find the 5 best-selling products: sum the ordered quantity of each product across all orders.
        // Sort descending by total quantity; for equal totals, sort ascending by product name.
        Console.WriteLine();
        Console.WriteLine("****** Question 4 ******");
        Console.WriteLine("Provided:");

        var topProducts =
            (from op in database.OrderProducts
             join p in database.Products on op.ProductId equals p.ProductId
             group op.Quantity by p.Name into g
             let total = g.Sum(q => (long)q)
             orderby total descending, g.Key   // name as tiebreaker
             select new { Name = g.Key, Total = total })
            .Take(5);

        foreach (var x in topProducts)
            Console.WriteLine(x.Name + " " + x.Total);

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Gaming chair 21");
        Console.WriteLine("Marker 11");
        Console.WriteLine("CD 10");
        Console.WriteLine("Pen 10");
        Console.WriteLine("Monitor 5");

        // Question 5
        // For every customer, compute the total amount spent on all their orders
        // (price × quantity of each ordered product), reduced by the discount of their loyalty card.
        // Customers without a loyalty card get no discount. Round the result to 2 decimal places.
        // Sort ascending by last name, then by first name.
        Console.WriteLine();
        Console.WriteLine("****** Question 5 ******");
        Console.WriteLine("Provided:");

        var orderLines =
            from o in database.Orders
            join op in database.OrderProducts on o.OrderId equals op.OrderId
            join p in database.Products on op.ProductId equals p.ProductId
            select new { o.CustomerId, Value = p.Price * op.Quantity };

        // sum per customer
        var totals =
            (from line in orderLines
             group line.Value by line.CustomerId into g
             select new { CustomerId = g.Key, Total = g.Sum() })
            .ToDictionary(t => t.CustomerId, t => t.Total);

        var spending =
            from c in database.Customers
                // no orders -> not in dict -> 0
            let total = totals.GetValueOrDefault(c.CustomerId)
            // no card -> FirstOrDefault gives 0, so no discount
            let discount = database.LoyaltyCards
                                   .Where(lc => lc.CustomerId == c.CustomerId)
                                   .Select(lc => lc.Discount)
                                   .FirstOrDefault()
            orderby c.LastName, c.FirstName
            select new { Customer = c, Spent = Math.Round(total * (1 - discount), 2) };

        foreach (var x in spending)
            Console.WriteLine(x.Customer.LastName + " " + x.Customer.FirstName + " " + x.Spent);

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Antoniewski Michał 1053");
        Console.WriteLine("Bąk Magdalena 2912");
        Console.WriteLine("Bąk Marek 3000");
        Console.WriteLine("Czerwińska Justyna 2593,5");
        Console.WriteLine("Kowalska Aneta 212,5");
        Console.WriteLine("Kowalski Jan 3654");
        Console.WriteLine("Marciniak Jerzy 13600");
        Console.WriteLine("Nowak Alina 1400");
        Console.WriteLine("Nowak Andrzej 2550");
        Console.WriteLine("Wiśniewski Artur 2025");

        Console.WriteLine();
    }
}