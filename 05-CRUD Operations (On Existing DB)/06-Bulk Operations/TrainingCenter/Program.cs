using System.Diagnostics;
using EFCore.BulkExtensions;
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
long badTime = RunBadBulkInsert(context);

PrintSeparator();

long goodTime = RunGoodBulkInsert(context);

PrintSeparator();

long bulkTime = RunBestBulkInsertUsingBulkExtensions(context);

PrintSeparator();

ShowComparison(badTime, goodTime, bulkTime);


/// <summary>
/// Demonstrates the bad bulk insert approach by calling SaveChanges() inside the loop.
/// </summary>
static long RunBadBulkInsert(AppDbContext context)
{
    Console.WriteLine("BAD BULK INSERT - SaveChanges() Inside Loop");
    Console.WriteLine("-------------------------------------------");
    Console.WriteLine();

    var stopwatch = Stopwatch.StartNew();

    for (int i = 1; i <= 15000; i++)
    {
        var student = new Student
        {
            FirstName = "Bad",
            LastName = $"Student{i}",
            Email = $"bad{i}@test.com",
            Status = "Active",
            RegisteredAt = DateTime.Now
        };

        context.Students.Add(student);

        // BAD: SaveChanges() inside the loop creates many database calls
        int affectedRows = context.SaveChanges();

        Console.WriteLine(
            $"Inserted Student {i} | Generated ID: {student.StudentId} | Affected Rows: {affectedRows}");
    }

    stopwatch.Stop();

    Console.WriteLine();
    Console.WriteLine($"Time Taken - BAD: {stopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine("Result: Many database calls were executed.");
    Console.WriteLine();

    return stopwatch.ElapsedMilliseconds;
}


/// <summary>
/// Demonstrates the good bulk insert approach by adding all entities first,
/// then calling SaveChanges() only once.
/// </summary>
static long RunGoodBulkInsert(AppDbContext context)
{
    Console.WriteLine("GOOD BULK INSERT - Single SaveChanges()");
    Console.WriteLine("---------------------------------------");
    Console.WriteLine();

    var stopwatch = Stopwatch.StartNew();

    var students = new List<Student>();

    for (int i = 1; i <= 15000; i++)
    {
        students.Add(new Student
        {
            FirstName = "Good",
            LastName = $"Student{i}",
            Email = $"good{i}@test.com",
            Status = "Active",
            RegisteredAt = DateTime.Now
        });
    }

    context.Students.AddRange(students);

    Console.WriteLine("Students added to Change Tracker as Added.");
    Console.WriteLine("INSERT does not support ToQueryString().");
    Console.WriteLine("Runtime logging will show the actual executed INSERT SQL.");
    Console.WriteLine();

    int affectedRows = context.SaveChanges();

    stopwatch.Stop();

    Console.WriteLine($"Affected Rows: {affectedRows}");
    Console.WriteLine($"Time Taken - GOOD: {stopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine("Result: One SaveChanges() call was executed.");
    Console.WriteLine();

    return stopwatch.ElapsedMilliseconds;
}


/// <summary>
/// Demonstrates the best bulk insert approach using EFCore.BulkExtensions.
/// </summary>
static long RunBestBulkInsertUsingBulkExtensions(AppDbContext context)
{
    Console.WriteLine("BEST BULK INSERT - EFCore.BulkExtensions");
    Console.WriteLine("----------------------------------------");
    Console.WriteLine();

    var stopwatch = Stopwatch.StartNew();

    var students = new List<Student>();

    for (int i = 1; i <= 15000; i++)
    {
        students.Add(new Student
        {
            FirstName = "Bulk",
            LastName = $"Student{i}",
            Email = $"bulk{i}@test.com",
            Status = "Active",
            RegisteredAt = DateTime.Now
        });
    }

    Console.WriteLine("Students prepared in memory.");
    Console.WriteLine("BulkInsert() performs optimized database bulk operation.");
    Console.WriteLine("BulkInsert() does not use SaveChanges().");
    Console.WriteLine();

    context.BulkInsert(students);

    stopwatch.Stop();

    Console.WriteLine($"Inserted Rows: {students.Count}");
    Console.WriteLine($"Time Taken - BULK: {stopwatch.ElapsedMilliseconds} ms");
    Console.WriteLine("Result: Optimized bulk insert operation was executed.");
    Console.WriteLine();

    return stopwatch.ElapsedMilliseconds;
}


/// <summary>
/// Shows a simple performance comparison between the bad, good, and bulk approaches.
/// </summary>
static void ShowComparison(long badTime, long goodTime, long bulkTime)
{
    Console.WriteLine("COMPARISON Insert 15000");
    Console.WriteLine("----------");
    Console.WriteLine($"BAD  Time : {badTime} ms");
    Console.WriteLine($"GOOD Time : {goodTime} ms");
    Console.WriteLine($"BULK Time : {bulkTime} ms");
    Console.WriteLine();
    Console.WriteLine("Fewer SaveChanges() calls usually means better performance.");
    Console.WriteLine("BulkExtensions is usually best for large datasets.");
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
