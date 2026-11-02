using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrainingCenter.Data;
using TrainingCenter.Entities;


// Configuration setup
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();


// Read connection string
string? connectionString =
    configuration.GetConnectionString("DefaultConnection");


// Validate values
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Connection string not found.");
    return;
}


// Create options
var options =
    new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .Options;


// Create context
using var context = new AppDbContext(options);


// Test connection if relevant
if (!context.Database.CanConnect())
{
    Console.WriteLine("Database connection failed.");
    return;
}

Console.WriteLine("Connected successfully.");
Console.WriteLine();


// Call main methods
RunBadExampleWithoutTransaction(context);

PrintSeparator();

RunGoodExampleWithTransaction(context);


/// <summary>
/// Demonstrates the bad approach where multiple operations are executed without a transaction.
/// </summary>
static void RunBadExampleWithoutTransaction(AppDbContext context)
{
    Console.WriteLine("BAD EXAMPLE - No Transaction");
    Console.WriteLine("----------------------------");
    Console.WriteLine();

    try
    {
        var student = new Student
        {
            FirstName = "Bad",
            LastName = "Transaction",
            Email = "bad@test.com",
            Status = "Active",
            RegisteredAt = DateTime.Now
        };

        // Add() marks the entity as Added
        context.Students.Add(student);

        Console.WriteLine("Student added to Change Tracker as Added.");
        Console.WriteLine("INSERT does not support ToQueryString().");
        Console.WriteLine("Runtime logging will show the actual executed INSERT SQL.");
        Console.WriteLine();

        // SaveChanges() executes INSERT immediately
        int affectedRows = context.SaveChanges();

        Console.WriteLine($"Student inserted. Affected Rows: {affectedRows}");
        Console.WriteLine($"Generated Student ID: {student.StudentId}");
        Console.WriteLine();

        // Simulate failure after the first save
        throw new Exception("Something went wrong after inserting student.");

        // This code will not execute because of the simulated error above
        var course = new Course
        {
            Title = "Broken Course",
            DurationHours = 10
        };

        context.Courses.Add(course);
        context.SaveChanges();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine("Problem: Student was saved, but the second operation was not completed.");
        Console.WriteLine();
    }
}


/// <summary>
/// Demonstrates the good approach where multiple operations are protected by a transaction.
/// </summary>
static void RunGoodExampleWithTransaction(AppDbContext context)
{
    Console.WriteLine("GOOD EXAMPLE - With Transaction");
    Console.WriteLine("-------------------------------");
    Console.WriteLine();

    using var transaction = context.Database.BeginTransaction();
    
    try
    {
        var student = new Student
        {
            FirstName = "Good",
            LastName = "Transaction",
            Email = "good@test.com",
            Status = "Active",
            RegisteredAt = DateTime.Now
        };

        // Add() marks the entity as Added
        context.Students.Add(student);

        Console.WriteLine("Student added to Change Tracker as Added.");
        Console.WriteLine("INSERT does not support ToQueryString().");
        Console.WriteLine("Runtime logging will show the actual executed INSERT SQL.");
        Console.WriteLine();

        // SaveChanges() executes INSERT inside the transaction
        int affectedRows = context.SaveChanges();

        Console.WriteLine($"Student inserted inside transaction. Affected Rows: {affectedRows}");
        Console.WriteLine($"Generated Student ID: {student.StudentId}");
        Console.WriteLine();

        // Simulate failure before the transaction is committed
        throw new Exception("Failure inside transaction.");

        // This code will not execute because of the simulated error above
        var course = new Course
        {
            Title = "Safe Course",
            DurationHours = 20
        };

        context.Courses.Add(course);
        context.SaveChanges();

        // Commit only if all operations succeed
        transaction.Commit();

        Console.WriteLine("Transaction committed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");

        // Rollback cancels all operations inside this transaction
        transaction.Rollback();

        Console.WriteLine("Transaction rolled back. Nothing was saved from this transaction.");
        Console.WriteLine();
    }
}


/// <summary>
/// Prints a separator between examples.
/// </summary>
static void PrintSeparator()
{
    Console.WriteLine(new string('-', 60));
    Console.WriteLine();
}


/// <summary>
/// Displays generated SQL before execution.
/// </summary>
static void PreviewSQLUsingToQueryString(string SQLString)
{
    Console.WriteLine("\nPreview SQL using ToQueryString():");
    Console.WriteLine("----------------------------------");
    Console.WriteLine(SQLString);
    Console.WriteLine();
}
