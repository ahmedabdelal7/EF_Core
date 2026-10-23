using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrainingCenter.Data;

// Configuration setup
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

string? connectionString =
    configuration.GetConnectionString("DefaultConnection");

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

// Call examples
Example_First(context);
Example_FirstOrDefault(context);
Example_Single(context);
Example_SingleOrDefault(context);


/// <summary>
/// First() returns the first matching row.
/// Use it when at least one row is expected.
/// </summary>
static void Example_First(AppDbContext context)
{
    Console.WriteLine("\nExample 1 - First()");
    Console.WriteLine("-------------------");

    // Build query first (no execution yet)
    var query = context.Students
        .Where(s => s.Status == "Active")
        .OrderBy(s => s.StudentId);

    // Preview query shape
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    // Runtime logging will show the actual executed SQL.
    var student = query.First();

    Console.WriteLine("\nFirst Active Student:");
    Console.WriteLine($"{student.StudentId} - {student.FirstName} {student.LastName}");
}


/// <summary>
/// FirstOrDefault() returns the first matching row,
/// or null if no row exists.
/// </summary>
static void Example_FirstOrDefault(AppDbContext context)
{
    Console.WriteLine("\nExample 2 - FirstOrDefault()");
    Console.WriteLine("----------------------------");

    // Build query first (no execution yet)
    var query = context.Students
        .Where(s => s.Email == "notfound@student.com");

    // Preview query shape
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    // Runtime logging will show the actual executed SQL.
    var student = query.FirstOrDefault();

    if (student == null)
    {
        Console.WriteLine("\nNo student found.");
    }
    else
    {
        Console.WriteLine("\nStudent Found:");
        Console.WriteLine($"{student.StudentId} - {student.FirstName} {student.LastName}");
    }
}


/// <summary>
/// Single() expects exactly one matching row.
/// Use it when the data must be unique.
/// </summary>
static void Example_Single(AppDbContext context)
{
    Console.WriteLine("\nExample 3 - Single()");
    Console.WriteLine("--------------------");

    // Build query first (no execution yet)
    var query = context.Courses
        .Where(c => c.Code == "EF-101");

    // Preview query shape
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    // Runtime logging will show the actual executed SQL.
    var course = query.Single();

    Console.WriteLine("\nCourse Found:");
    Console.WriteLine($"{course.CourseId} - {course.Code} - {course.Title}");
}


/// <summary>
/// SingleOrDefault() expects zero or one matching row.
/// Returns null if none exists, but throws if duplicates exist.
/// </summary>
static void Example_SingleOrDefault(AppDbContext context)
{
    Console.WriteLine("\nExample 4 - SingleOrDefault()");
    Console.WriteLine("-----------------------------");

    // Build query first (no execution yet)
    var query = context.Courses
        .Where(c => c.Code == "UNKNOWN-999");

    // Preview query shape
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    // Runtime logging will show the actual executed SQL.
    var course = query.SingleOrDefault();

    if (course == null)
    {
        Console.WriteLine("\nNo course found.");
    }
    else
    {
        Console.WriteLine("\nCourse Found:");
        Console.WriteLine($"{course.CourseId} - {course.Code} - {course.Title}");
    }
}

static void PreviewSQLUsingToQueryString(string SQLString)
{
    Console.WriteLine("\nPreview SQL using ToQueryString():");
    Console.WriteLine("----------------------------------");
    Console.WriteLine(SQLString);
    Console.WriteLine();
}