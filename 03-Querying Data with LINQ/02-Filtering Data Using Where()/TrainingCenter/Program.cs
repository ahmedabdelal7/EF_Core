using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrainingCenter.Data;
using Microsoft.Extensions.Logging;


// Configuration setup
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

string? connectionString = configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Connection string not found.");
    return;
}

// Create options with logging
var options =
    new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .Options;


using var context = new AppDbContext(options);

Console.WriteLine("Connected successfully.");
Console.WriteLine();

// Call filtering function
GetActiveStudents(context);


/// <summary>
/// Retrieves only active students using Where()
/// </summary>
static void GetActiveStudents(AppDbContext context)
{
    // Build query (no execution yet)
    var query = context.Students
        .Where(s => s.Status == "Active")
        .OrderBy(s => s.StudentId);

    // Show generated SQL
    PreviewSQLUsingToQueryString(query.ToQueryString());


    // Execute query
    var students = query.ToList();

    // Print results
    Console.WriteLine("\nActive Students:");
    Console.WriteLine("----------------");

    foreach (var student in students)
    {
        Console.WriteLine($"{student.StudentId} - {student.FirstName} {student.LastName}");
    }

    Console.WriteLine();
    Console.WriteLine($"Total Active Students: {students.Count}");
}

static void PreviewSQLUsingToQueryString(string SQLString)
{
    Console.WriteLine("\nPreview SQL using ToQueryString():");
    Console.WriteLine("----------------------------------");
    Console.WriteLine(SQLString);
    Console.WriteLine();

}
