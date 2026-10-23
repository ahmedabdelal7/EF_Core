
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
GetStudentNames(context);


/// <summary>
/// Retrieves only student names using projection.
/// </summary>
static void GetStudentNames(AppDbContext context)
{
    Console.WriteLine("Projection Example Using Select()");
    Console.WriteLine("---------------------------------");
    Console.WriteLine();

    // Build query first (no execution yet)
    var query =
        context.Students
               .Select(s => new
               {
                   s.FirstName,
                   s.LastName
               });

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    var students = query.ToList();

    // Print results
    Console.WriteLine("\n\nStudent Names:");
    Console.WriteLine("--------------");

    foreach (var student in students)
    {
        Console.WriteLine($"{student.FirstName} {student.LastName}");
    }

    Console.WriteLine();
    Console.WriteLine($"\nTotal Students: {students.Count}");
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
