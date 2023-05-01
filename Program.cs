using NLog;
using System.Linq;
using Northwind_Console.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

// See https://aka.ms/new-console-template for more information
string path = Directory.GetCurrentDirectory() + "\\nlog.config";

// create instance of Logger
var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
logger.Info("Program started");

try
{
    var db = new NWConsole_23_JDWContext();
    string choice;
    do
    {
        Console.WriteLine("1) Display Categories");
        Console.WriteLine("2) Add Category");
        Console.WriteLine("3) Display Category and related products");
        Console.WriteLine("4) Display all Categories and their related products");
        Console.WriteLine("5) Part 1 requirements");
        Console.WriteLine("6) Part 2 requirements");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info($"Option {choice} selected");
        if (choice == "1")
        {
            var query = db.Categories.OrderBy(c => c.CategoryName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }
            Console.ForegroundColor = ConsoleColor.White;

        }
        else if (choice == "2")
        {
            Category category = new Category();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");

                    db.Categories.Add(category);
                    db.SaveChanges();
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
        else if (choice == "3")
        {
            var query = db.Categories.OrderBy(c => c.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"\t{p.ProductName}");
            }
        }
        else if (choice == "4")
        {
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
        }
        else if (choice == "5")
        {
            do
            {
                Console.WriteLine("----------------Part 1----------------");
                Console.WriteLine("1) Add new record to Products table");
                Console.WriteLine("2) Edit record from Products table");
                Console.WriteLine("3) Display all product names in Products table");
                Console.WriteLine("4) Display specific product in Products table");
                Console.WriteLine("Press enter to quit");
                choice = Console.ReadLine();
                Console.Clear();
                logger.Info($"Option {choice} selected");

                if (choice == "1")
                {
                    string input;
                    Decimal Decimal;
                    Product product = new Product();
                    Console.WriteLine("Enter Product Name:");
                    product.ProductName = Console.ReadLine();
                    Console.WriteLine("Enter the id of the category this product is from:");
                    var categoriesQuery = db.Categories.OrderBy(p => p.CategoryName);
                    foreach (var item in categoriesQuery)
                    {
                        Console.WriteLine($"{item.CategoryId}: {item.CategoryName} - {item.Description}");
                    }
                    product.CategoryId = int.Parse(Console.ReadLine());
                    Console.WriteLine("Enter SupplierId:");
                    var suppliersQuery = db.Suppliers.OrderBy(s => s.CompanyName);
                    foreach (var item in suppliersQuery)
                    {
                        Console.WriteLine($"{item.SupplierId}: {item.CompanyName}");
                    }
                    product.SupplierId = int.Parse(Console.ReadLine());
                    Console.WriteLine("Enter UnitPrice:");
                    Decimal = decimal.Parse(Console.ReadLine());
                    product.UnitPrice = Decimal;
                    Console.WriteLine("Enter UnitsInStock:");
                    product.UnitsInStock = short.Parse(Console.ReadLine());
                    Console.WriteLine("Enter if discontinued (yes or no):");
                    input = Console.ReadLine();
                    while (input.ToLower() != "yes" && input.ToLower() != "no")
                    {
                        Console.WriteLine("Invalid input. Type yes if true, type no if false");
                        input = Console.ReadLine();
                    }
                    if (input.ToLower() == "yes")
                    {
                        product.Discontinued = true;
                    }
                    else if (input.ToLower() == "no")
                    {
                        product.Discontinued = false;
                    }
                    else
                    {
                        logger.Info("Invalid input, Discontinued set to false");
                        product.Discontinued = false;
                    }
                    ValidationContext context = new ValidationContext(product, null, null);
                    List<ValidationResult> results = new List<ValidationResult>();

                    var isValid = Validator.TryValidateObject(product, context, results, true);
                    if (isValid)
                    {
                        // check for unique name
                        if (db.Products.Any(p => p.ProductName == product.ProductName))
                        {
                            // generate validation error
                            isValid = false;
                            results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                        }
                        else
                        {
                            logger.Info("Validation passed");
                            // Save product to database
                            db.Products.Add(product);
                            db.SaveChanges();
                            logger.Info($"Product '{product.ProductName}' added");
                        }
                    }
                    if (!isValid)
                    {
                        foreach (var result in results)
                        {
                            logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                        }
                    }
                }
                else if (choice == "2")
                {
                    Console.WriteLine("Choose Product to edit:");
                    var product = GetProduct(db, logger);
                    if (product != null)
                    {
                        //input product
                        Product UpdatedProduct = InputProduct(db, logger);
                        if (UpdatedProduct != null)
                        {
                            product.ProductName = UpdatedProduct.ProductName;
                            db.SaveChanges();
                            logger.Info("product edited successfully");
                        }
                    }
                }
                else if (choice == "3")
                {
                    string input;
                    Console.WriteLine("1) See all products");
                    Console.WriteLine("2) See all discontinued products");
                    Console.WriteLine("3) See all active products");
                    input = Console.ReadLine();

                    if (input == "1")
                    {
                        var query = db.Products.OrderBy(p => p.ProductName);

                        Console.WriteLine($"{query.Count()} Products returned");
                        foreach (var item in query)
                        {
                            Console.WriteLine(item.ProductName + " | Discontinued = " + item.Discontinued);
                        }
                    }
                    else if (input == "2")
                    {
                        var query = db.Products.Where(p => p.Discontinued == true).OrderBy(p => p.ProductName);

                        Console.WriteLine($"{query.Count()} Products returned");
                        foreach (var item in query)
                        {
                            Console.WriteLine(item.ProductName);
                        }

                    }
                    else if (input == "3")
                    {
                        var query = db.Products.Where(p => p.Discontinued == false).OrderBy(p => p.ProductName);

                        Console.WriteLine($"{query.Count()} Products returned");
                        foreach (var item in query)
                        {
                            Console.WriteLine(item.ProductName);
                        }

                    }

                }
                else if (choice == "4")
                {
                    Console.WriteLine("Which product would you like to view? (Enter the ID)");
                    var product = GetProduct(db, logger);

                    if (product != null)
                    {
                        Console.WriteLine("Product ID: " + product.ProductId);
                        Console.WriteLine("Product Name: " + product.ProductName);
                        Console.WriteLine("Supplier ID: " + product.SupplierId);
                        Console.WriteLine("Category ID: " + product.CategoryId);
                        Console.WriteLine("Quantity Per Unit: " + product.QuantityPerUnit);
                        Console.WriteLine("Unit Price: " + product.UnitPrice);
                        Console.WriteLine("Units In Stock: " + product.UnitsInStock);
                        Console.WriteLine("Units On Order: " + product.UnitsOnOrder);
                        Console.WriteLine("Reorder Level: " + product.ReorderLevel);
                        Console.WriteLine("Discontinued: " + product.Discontinued);
                    }
                }
            } while (choice == "1" || choice == "2" || choice == "3" || choice == "4");

        }
        else if (choice == "6")
        {
            do
            {
                Console.WriteLine("----------------Part 2----------------");
                Console.WriteLine("1) Add new record to Categories table");
                Console.WriteLine("2) Edit record from Categories table");
                Console.WriteLine("3) Display all category names and descriptions in Categories table");
                Console.WriteLine("4) Display all categories and their related active products");
                Console.WriteLine("5) Display specific category and its related active product data");
                Console.WriteLine("Press enter to quit");
                choice = Console.ReadLine();
                Console.Clear();
                logger.Info($"Option {choice} selected");

                if (choice == "1")
                {
                    Category category = new Category();
                    Console.WriteLine("Enter Category Name:");
                    category.CategoryName = Console.ReadLine();
                    Console.WriteLine("Enter the Category Description:");
                    category.Description = Console.ReadLine();
                    ValidationContext context = new ValidationContext(category, null, null);
                    List<ValidationResult> results = new List<ValidationResult>();

                    var isValid = Validator.TryValidateObject(category, context, results, true);
                    if (isValid)
                    {
                        // check for unique name
                        if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                        {
                            // generate validation error
                            isValid = false;
                            results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                        }
                        else
                        {
                            logger.Info("Validation passed");

                            db.Categories.Add(category);
                            db.SaveChanges();
                            logger.Info($"Category '{category.CategoryName}' added");
                        }
                    }
                    if (!isValid)
                    {
                        foreach (var result in results)
                        {
                            logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                        }
                    }
                }
                else if (choice == "2")
                {
                    Console.WriteLine("Choose Category to edit:");
                    var category = GetCategory(db, logger);
                    if (category != null)
                    {
                        //input category
                        Category UpdatedCategory = InputCategory(db, logger);
                        if (UpdatedCategory != null)
                        {
                            category.CategoryName = UpdatedCategory.CategoryName;
                            db.SaveChanges();
                            logger.Info("category edited successfully");
                        }
                    }
                }
                else if (choice == "3")
                {
                    var query = db.Categories.OrderBy(c => c.CategoryName);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{query.Count()} records returned");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    foreach (var item in query)
                    {
                        Console.WriteLine($"{item.CategoryName} - {item.Description}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (choice == "4")
                {
                    var query = db.Categories.Include(category => category.Products.Where(product => product.Discontinued == false)).OrderBy(c => c.CategoryId);
                    foreach (var item in query)
                    {
                        Console.WriteLine($"{item.CategoryName}");
                        foreach (Product p in item.Products)
                        {
                            Console.WriteLine($"\t{p.ProductName}");
                        }
                    }
                }
                else if (choice == "5") 
                {
                    var query = db.Categories.OrderBy(c => c.CategoryId);

                    Console.WriteLine("Select the category whose active products you want to display:");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var item in query)
                    {
                        Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    int id = int.Parse(Console.ReadLine());
                    Console.Clear();
                    logger.Info($"CategoryId {id} selected");
                    Category category = db.Categories.Include(category => category.Products.Where(product => product.Discontinued == false)).FirstOrDefault(c => c.CategoryId == id);
                    Console.WriteLine($"{category.CategoryName} - {category.Description}");
                    foreach (Product p in category.Products)
                    {
                        Console.WriteLine($"\t{p.ProductName}");
                    }
                }

            } while (choice == "1" || choice == "2" || choice == "3" || choice == "4" || choice == "5");
        }
        Console.WriteLine();

    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");

static Product GetProduct(NWConsole_23_JDWContext db, Logger logger)
{
    // display all products
    var products = db.Products.OrderBy(p => p.ProductId);
    foreach (Product p in products)
    {
        Console.WriteLine($"{p.ProductId}: {p.ProductName}");
    }
    if (int.TryParse(Console.ReadLine(), out int ProductId))
    {
        Product product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
        if (product != null)
        {
            return product;
        }
    }
    logger.Error("Invalid Product Id");
    return null;
}

static Product InputProduct(NWConsole_23_JDWContext db, Logger logger)
{
    Product product = new Product();
    Console.WriteLine("Enter the Product name");
    product.ProductName = Console.ReadLine();

    ValidationContext context = new ValidationContext(product, null, null);
    List<ValidationResult> results = new List<ValidationResult>();

    var isValid = Validator.TryValidateObject(product, context, results, true);
    if (isValid)
    {
        // prevent duplicate product names
        if (db.Products.Any(p => p.ProductName == product.ProductName))
        {
            // generate error
            results.Add(new ValidationResult("Product name exists", new string[] { "Name" }));
        }
        else
        {
            return product;
        }
    }

    foreach (var result in results)
    {
        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
    }

    return null;
}

static Category GetCategory(NWConsole_23_JDWContext db, Logger logger)
{
    // display all categories
    var catigories = db.Categories.OrderBy(c => c.CategoryId);
    foreach (Category c in catigories)
    {
        Console.WriteLine($"{c.CategoryId}: {c.CategoryName}");
    }
    if (int.TryParse(Console.ReadLine(), out int CategoryId))
    {
        Category category = db.Categories.FirstOrDefault(c => c.CategoryId == CategoryId);
        if (category != null)
        {
            return category;
        }
    }
    logger.Error("Invalid Category Id");
    return null;
}

static Category InputCategory(NWConsole_23_JDWContext db, Logger logger)
{
    Category category = new Category();
    Console.WriteLine("Enter the Category name");
    category.CategoryName = Console.ReadLine();

    ValidationContext context = new ValidationContext(category, null, null);
    List<ValidationResult> results = new List<ValidationResult>();

    var isValid = Validator.TryValidateObject(category, context, results, true);
    if (isValid)
    {
        // prevent duplicate category names
        if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
        {
            // generate error
            results.Add(new ValidationResult("Category name exists", new string[] { "Name" }));
        }
        else
        {
            return category;
        }
    }
    foreach (var result in results)
    {
        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
    }

    return null;
}


