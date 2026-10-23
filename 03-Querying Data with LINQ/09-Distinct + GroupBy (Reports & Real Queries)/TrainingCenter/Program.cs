using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrainingCenter.Data;


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
    Console.WriteLine("Could not connect to TrainingCenterDB.");
    return;
}

Console.WriteLine("Connected successfully.");
Console.WriteLine();


// Call main methods
ShowReports(context);


/// <summary>
/// Demonstrates Distinct() and GroupBy() reporting queries.
/// </summary>
static void ShowReports(AppDbContext context)
{
    ShowDistinctStudentStatuses(context);

    Console.WriteLine();

    ShowStudentsPerStatusReport(context);
}


/// <summary>
/// Shows unique student statuses using Distinct().
/// </summary>
static void ShowDistinctStudentStatuses(AppDbContext context)
{
    Console.WriteLine("Unique Student Statuses");
    Console.WriteLine("-----------------------");

    // Build query first
    var query =
        context.Students
               .Select(s => s.Status)
               .Distinct();

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(query.ToQueryString());



    // Execute query
    var statuses = query.ToList();

    Console.WriteLine();
    // Print readable output
    foreach (var status in statuses)
    {
        Console.WriteLine(status);
    }
}


/// <summary>
/// Shows number of students grouped by status.
/// </summary>
static void ShowStudentsPerStatusReport(AppDbContext context)
{
    Console.WriteLine("Students Per Status");
    Console.WriteLine("-------------------");

    // Build query first
    var query =
        context.Students
               .GroupBy(s => s.Status)
               .Select(g => new
               {
                   Status = g.Key,
                   TotalStudents = g.Count()
               })
               .OrderBy(s => s.Status);
    // Preview SQL before execution
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for Count().
    var report = query.ToList();

    Console.WriteLine();
    // Print readable output
    foreach (var row in report)
    {
        Console.WriteLine($"{row.Status} : {row.TotalStudents}");
    }
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
