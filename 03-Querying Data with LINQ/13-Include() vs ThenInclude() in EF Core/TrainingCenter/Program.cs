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

// Create options with logging
var options =
    new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .Options;

// Create context
using var context = new AppDbContext(options);

Console.WriteLine("Connected successfully.");
Console.WriteLine();

// Call main method
ShowStudentsWithEnrollmentsAndCourses(context);


/// <summary>
/// Loads students with their enrollments and related courses.
/// </summary>
static void ShowStudentsWithEnrollmentsAndCourses(AppDbContext context)
{
    // Build query first
    var query = context.Students
        .Include(s => s.Enrollments) // Left Join --> Optional RelationShip
            .ThenInclude(e => e.Course) // Inner Join --> Mandatory RelationShip
        .OrderBy(s => s.StudentId);

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    var students = query.ToList();

    Console.WriteLine("\nStudents With Enrollments and Courses:");
    Console.WriteLine("--------------------------------------");

    foreach (var student in students)
    {
        Console.WriteLine($"{student.StudentId} - {student.FirstName} {student.LastName}");

        foreach (var enrollment in student.Enrollments)
        {
            Console.WriteLine(
                $"   Course: {enrollment.Course.Title}, " +
                $"Status: {enrollment.Status}, " +
                $"Progress: {enrollment.ProgressPercent}%");
        }

        Console.WriteLine();
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
