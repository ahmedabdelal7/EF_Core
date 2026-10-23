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
Console.WriteLine("Connected successfully.");
Console.WriteLine();


// Call main methods
GetStudentByIdUsingFind(context);

Console.WriteLine();
GetStudentByIdUsingFirstOrDefault(context);


/// <summary>
/// Retrieves student by Primary Key using Find().
/// Best method for direct PK lookup.
/// May return tracked entity without executing SQL again.
/// </summary>
static void GetStudentByIdUsingFind(AppDbContext context)
{
    Console.WriteLine("Using Find()");
    Console.WriteLine("------------");

    // Find() does not support ToQueryString().
    // Runtime logging will show actual SQL only if query is sent to database.
    var student = context.Students.Find(1);

    PrintStudent(student);
}


/// <summary>
/// Retrieves student by Primary Key using FirstOrDefault().
/// Useful when filtering with conditions.
/// </summary>
static void GetStudentByIdUsingFirstOrDefault(AppDbContext context)
{
    Console.WriteLine("Using FirstOrDefault()");
    Console.WriteLine("----------------------");

    // Build query first
    var query =
        context.Students
               .Where(s => s.StudentId == 1);

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL.
    var student = query.FirstOrDefault();

    PrintStudent(student);
}


/// <summary>
/// Prints student information in readable format.
/// </summary>
static void PrintStudent(dynamic? student)
{
    if (student != null)
    {
        Console.WriteLine("\n\nStudent Found:");
        Console.WriteLine(
            $"{student.StudentId} - {student.FirstName} {student.LastName}");
    }
    else
    {
        Console.WriteLine("Student not found.");
    }

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