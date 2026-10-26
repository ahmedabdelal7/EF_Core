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
    Console.WriteLine("Could not connect to TrainingCenterDB.");
    return;
}

Console.WriteLine("Connected successfully.");
Console.WriteLine();


// Call main methods
ShowStudentsWithProfiles(context);


/// <summary>
/// Shows students with or without profiles using Left Join.
/// </summary>
static void ShowStudentsWithProfiles(AppDbContext context)
{
    Console.WriteLine("Students With Profiles - Left Join");
    Console.WriteLine("----------------------------------");
    Console.WriteLine();

    // Build query first
    var report =
        context.Students.SelectMany(
                student => student.Enrollments,
                (student, enrollment) => new
                {
                    student.StudentId,
                    StudentName = student.FirstName + " " + student.LastName,
                    enrollment.CourseId,
                    enrollment.Status
                }
            ).OrderBy(x => x.StudentId);

    // Using Join
    var report2 =
        context.Students.Join(
                context.Enrollments,
                s => s.StudentId,
                e => e.StudentId,
                (student, enrollment) => new
                {
                    student.StudentId,
                    StudentName = student.FirstName + " " + student.LastName,
                    enrollment.CourseId,
                    enrollment.Status
                }
            ).OrderBy(x =>x.StudentId);

    //Using Query Syntax
    var report3 =
        from s in context.Students
        join e in context.Enrollments
        on s.StudentId equals e.StudentId
        orderby s.StudentId
        select new
        {
            s.StudentId,
            StudentName = s.FirstName + " " + s.LastName,
            e.CourseId,
            e.Status
        };

  // Apply sorting
  //var query =
  //      report2.OrderBy(x => x.StudentId);

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(report.ToQueryString());

    // Execute query
    var result = report.ToList();

    // Print readable output
    Console.WriteLine("Student Report:");
    Console.WriteLine("---------------");

    Console.WriteLine();
    foreach (var row in result)
    {
        Console.WriteLine(
            $"{row.StudentId} - {row.StudentName} - Course: {row.CourseId} - {row.Status}");
    }

    Console.WriteLine();
    Console.WriteLine($"Total Students Enrollments: {result.Count}");
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