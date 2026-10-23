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
GetFilteredStudents(context);


/// <summary>
/// Demonstrates combining Where(), Select(), and OrderByDescending().
/// </summary>
static void GetFilteredStudents(AppDbContext context)
{
    Console.WriteLine("Filtered Projection With Sorting");
    Console.WriteLine("--------------------------------");
    Console.WriteLine();

    // Build query first
    var query =
        context.Students
               .Where(s => s.Status == "Active") // Filter
               .Select(s => new                  // Projection
               {
                   s.StudentId,
                   FullName = s.FirstName + " " + s.LastName
               })
               .OrderByDescending(s => s.StudentId); // Sorting

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    var students = query.ToList();

    // Print results
    Console.WriteLine("\n\nFiltered Students:");
    Console.WriteLine("------------------");

    foreach (var student in students)
    {
        Console.WriteLine($"{student.StudentId} - {student.FullName}");
    }

    Console.WriteLine();
    Console.WriteLine($"Total Students: {students.Count}");
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