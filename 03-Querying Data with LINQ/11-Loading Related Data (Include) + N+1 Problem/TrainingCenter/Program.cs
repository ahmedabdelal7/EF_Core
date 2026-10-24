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
CompareInclude(context);


/// <summary>
/// Demonstrates bad vs good loading of related data using manual queries vs Include().
/// </summary>
static void CompareInclude(AppDbContext context)
{
    ShowBadNPlusOneApproach(context);

    PrintSeparator();

    ShowGoodIncludeApproach(context);
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
        int enrollmentsCount =
            enrollmentsQuery.Count();

        Console.WriteLine(
            $"{student.FirstName} {student.LastName} - Enrollments: {enrollmentsCount}");
    }

    Console.WriteLine();
    Console.WriteLine("Problem: One query for students + one query per student.");
    Console.WriteLine();
}


/// <summary>
/// Demonstrates the good approach by loading students with their enrollments using Include().
/// </summary>
static void ShowGoodIncludeApproach(AppDbContext context)
{
    Console.WriteLine("GOOD APPROACH - Include()");
    Console.WriteLine("-------------------------");
    Console.WriteLine();

    // Build query first
    //Left join
    var query =
        context.Students
               .Include(s => s.Enrollments);


    //Using Inner Join
    //var query2 =
    //    context.Students
    //                   .Where(s => s.Enrollments.Any());

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    var studentsWithEnrollments = query.ToList();

    Console.WriteLine();
    foreach (var student in studentsWithEnrollments)
    {
        Console.WriteLine(
            $"{student.FirstName} {student.LastName} - Enrollments: {student.Enrollments.Count}");
    }

    Console.WriteLine();
    Console.WriteLine("Result: Related enrollments are loaded with the students.");
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