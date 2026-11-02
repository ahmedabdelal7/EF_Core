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

// Create options with Lazy Loading enabled
var options =
    new DbContextOptionsBuilder<AppDbContext>()
        .UseLazyLoadingProxies()
        .UseSqlServer(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .Options;

// Create context
using var context = new AppDbContext(options);

// Test connection
if (!context.Database.CanConnect())
{
    Console.WriteLine("Could not connect to TrainingCenterDB.");
    return;
}

Console.WriteLine("Connected successfully.");
Console.WriteLine();

// Call main method
ShowLazyLoadingExample(context);


/// <summary>
/// Demonstrates Lazy Loading by loading a student first,
/// then accessing the Enrollments navigation property later.
/// </summary>
static void ShowLazyLoadingExample(AppDbContext context)
{
    Console.WriteLine("Lazy Loading Example");
    Console.WriteLine("--------------------");

    // Step 1:
    // Load one student only.
    // Notice: We are NOT using Include().
    var student = context.Students
        .OrderBy(s => s.StudentId)
        .FirstOrDefault();

    if (student == null)
    {
        Console.WriteLine("No students found.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("Student loaded:");
    Console.WriteLine($"{student.StudentId} - {student.FirstName} {student.LastName}");

    Console.WriteLine();
    Console.WriteLine("At this point, only the student was loaded.");
    Console.WriteLine("Enrollments were NOT explicitly loaded.");
    Console.WriteLine();

    Console.WriteLine("Now accessing student.Enrollments...");
    Console.WriteLine("Watch the console logs: EF Core will send another SQL query automatically.");
    Console.WriteLine();

    // Step 2:
    // Accessing the navigation property triggers Lazy Loading.
    var enrollmentCount = student.Enrollments.Count;

    Console.WriteLine($"Total Enrollments: {enrollmentCount}");
    Console.WriteLine();

    // Step 3:
    // Looping through enrollments also uses the loaded related data.
    foreach (var enrollment in student.Enrollments)
    {
        Console.WriteLine(
            $"Enrollment Id: {enrollment.EnrollmentId}, " +
            $"Course Id: {enrollment.CourseId}, " +
            $"Status: {enrollment.Status}");
    }
}
