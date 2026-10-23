using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrainingCenter.Data;


// Configuration setup
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();


// Read connection string
string? connectionString =
    configuration.GetConnectionString("DefaultConnection");


// Validate values
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Connection string 'DefaultConnection' was not found.");
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
    Console.WriteLine("Could not connect to TrainingCenterDB.");
    return;
}

Console.WriteLine("Connected successfully to TrainingCenterDB.");
Console.WriteLine();


// Call main methods
CompareCount(context);
PrintSeparator();

CompareAverage(context);
PrintSeparator();

CompareSum(context);


/// <summary>
/// Compares bad vs good COUNT approach.
/// </summary>
static void CompareCount(AppDbContext context)
{
    Console.WriteLine("COUNT EXAMPLE");
    Console.WriteLine();

    Console.WriteLine("BAD WAY:");
    Console.WriteLine();

    // Build query first
    var badQuery =
        context.Students;

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(badQuery.ToQueryString());

    // Execute query and load all rows into memory
    var students = badQuery.ToList();

    // Count happens in memory after data is already loaded
    int badCount =
        students.Count(s => s.Status == "Active");

    Console.WriteLine($"Bad Count (calculated in memory): {badCount}");
    Console.WriteLine();

    Console.WriteLine("GOOD WAY:");
    Console.WriteLine();

    // Build query first
    var goodQuery =
        context.Students
               .Where(s => s.Status == "Active");

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(goodQuery.ToQueryString());

    // Execute COUNT in the database
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for Count().
    int goodCount =
        goodQuery.Count();

    Console.WriteLine($"Good Count (calculated in database): {goodCount}");
    Console.WriteLine();
}


/// <summary>
/// Compares bad vs good AVERAGE approach.
/// </summary>
static void CompareAverage(AppDbContext context)
{
    Console.WriteLine("AVERAGE EXAMPLE");
    Console.WriteLine();

    Console.WriteLine("BAD WAY:");
    Console.WriteLine();

    // Build query first
    var badQuery =
        context.Enrollments;

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(badQuery.ToQueryString());

    // Execute query and load all rows into memory
    var enrollments = badQuery.ToList();

    // Average happens in memory after data is already loaded
    decimal badAverage =
        enrollments.Average(e => e.ProgressPercent);

    Console.WriteLine($"Bad Average (calculated in memory): {badAverage}");
    Console.WriteLine();

    Console.WriteLine("GOOD WAY:");
    Console.WriteLine();

    // Build query first
    var goodQuery =
        context.Enrollments
               .Select(e => e.ProgressPercent);

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(goodQuery.ToQueryString());

    // Execute AVERAGE in the database
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for Average().
    decimal goodAverage =
        goodQuery.Average();

    Console.WriteLine($"Good Average (calculated in database): {goodAverage}");
    Console.WriteLine();
}


/// <summary>
/// Compares bad vs good SUM approach.
/// </summary>
static void CompareSum(AppDbContext context)
{
    Console.WriteLine("SUM EXAMPLE");
    Console.WriteLine();

    Console.WriteLine("BAD WAY:");
    Console.WriteLine();

    // Build query first
    var badQuery =
        context.Courses;

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(badQuery.ToQueryString());

    // Execute query and load all rows into memory
    var courses = badQuery.ToList();

    // Sum happens in memory after data is already loaded
    int badSum =
        courses.Sum(c => c.DurationHours);

    Console.WriteLine($"Bad Sum (calculated in memory): {badSum}");
    Console.WriteLine();

    Console.WriteLine("GOOD WAY:");
    Console.WriteLine();

    // Build query first
    var goodQuery =
        context.Courses
               .Select(c => c.DurationHours);

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(goodQuery.ToQueryString());

    // Execute SUM in the database
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for Sum().
    int goodSum =
        goodQuery.Sum();

    Console.WriteLine($"Good Sum (calculated in database): {goodSum}");
    Console.WriteLine();
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