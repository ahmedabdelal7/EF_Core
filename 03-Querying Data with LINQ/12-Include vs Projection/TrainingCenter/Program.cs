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
CompareApproaches(context);


/// <summary>
/// Compares N+1, Include(), and Projection approaches.
/// </summary>
static void CompareApproaches(AppDbContext context)
{
    ShowBadNPlusOneApproach(context);

    PrintSeparator();

    ShowBetterIncludeApproach(context);

    PrintSeparator();

    ShowBestProjectionApproach(context);
}


/// <summary>
/// Demonstrates the bad N+1 approach by loading students first,
/// then running one additional count query per student.
/// </summary>
static void ShowBadNPlusOneApproach(AppDbContext context)
{
    Console.WriteLine("BAD APPROACH - N+1 Problem");
    Console.WriteLine("--------------------------");
    Console.WriteLine();

    // Build query first
    var studentsQuery =
        context.Students;

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(studentsQuery.ToQueryString());

    // Execute first query: load all students
    var students = studentsQuery.ToList();

    Console.WriteLine();
    foreach (var student in students)
    {
        // Build query first for each student
        var enrollmentsQuery =
            context.Enrollments
                   .Where(e => e.StudentId == student.StudentId);

        // Preview SQL query shape
        PreviewSQLUsingToQueryString(enrollmentsQuery.ToQueryString());

        // Execute Count() for each student
        // ToQueryString previews query shape,
        // runtime logging shows actual executed SQL for Count().
        int count =
            enrollmentsQuery.Count();

        Console.WriteLine($"{student.FirstName} {student.LastName} - {count}");
    }

    Console.WriteLine();
    Console.WriteLine("Result: Many queries executed. This is the N+1 problem.");
    Console.WriteLine();
}


/// <summary>
/// Demonstrates the better approach using Include() to load related enrollments.
/// </summary>
static void ShowBetterIncludeApproach(AppDbContext context)
{
    Console.WriteLine("BETTER APPROACH - Include()");
    Console.WriteLine("---------------------------");
    Console.WriteLine();

    // Build query first
    var includeQuery =
        context.Students
               .Include(s => s.Enrollments);

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(includeQuery.ToQueryString());

    // Execute query
    var studentsWithEnrollments = includeQuery.ToList();

    Console.WriteLine();
    foreach (var student in studentsWithEnrollments)
    {
        Console.WriteLine(
            $"{student.FirstName} {student.LastName} - {student.Enrollments.Count}");
    }

    Console.WriteLine();
    Console.WriteLine("Result: One query, but it loads full related enrollment data.");
    Console.WriteLine();
}


/// <summary>
/// Demonstrates the best approach using projection to retrieve only required data.
/// </summary>
static void ShowBestProjectionApproach(AppDbContext context)
{
    Console.WriteLine("BEST APPROACH - Projection");
    Console.WriteLine("--------------------------");
    Console.WriteLine();

    // Build query first
    var projectionQuery =
        context.Students
               .Select(s => new
               {
                   s.FirstName,
                   s.LastName,
                   EnrollmentsCount = s.Enrollments.Count()
               });

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(projectionQuery.ToQueryString());

    // Execute query
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for Count().
    var result = projectionQuery.ToList();

    Console.WriteLine();
    foreach (var student in result)
    {
        Console.WriteLine(
            $"{student.FirstName} {student.LastName} - {student.EnrollmentsCount}");
    }

    Console.WriteLine();
    Console.WriteLine("Result: One query, minimal data, best performance.");
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