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
CheckData(context);


/// <summary>
/// Demonstrates Any() and All().
/// </summary>
static void CheckData(AppDbContext context)
{
    Console.WriteLine("Any() and All() Example");
    Console.WriteLine("-----------------------");
    Console.WriteLine();

    // --------------------------------------------------
    // Any() Example
    // --------------------------------------------------

    // Build query first
    var activeStudentsQuery =
        context.Students
               .Where(s => s.Status == "Active");

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(activeStudentsQuery.ToQueryString());

    // Execute query
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for Any().
    bool hasActiveStudents =
        activeStudentsQuery.Any();

    Console.WriteLine($"Has Active Students: {hasActiveStudents}");
    Console.WriteLine();

    // --------------------------------------------------
    // All() Example
    // --------------------------------------------------

    // Build query first
    var coursesQuery =
        context.Courses;

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(coursesQuery.ToQueryString());

    // Execute query
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for All().
    bool allCoursesValid =
        coursesQuery.All(c => c.Price > 0);

    Console.WriteLine($"All Courses Price > 0: {allCoursesValid}");
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