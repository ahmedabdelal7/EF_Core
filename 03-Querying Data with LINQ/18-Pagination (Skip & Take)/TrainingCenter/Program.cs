using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrainingCenter.Data;


// Configuration setup
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();


// Read connection string
string? connectionString =
    configuration.GetConnectionString("DefaultConnection");


// Validate values
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Connection string 'DefaultConnection' was not found.");
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

Console.WriteLine("Connected successfully to TrainingCenterDB.");
Console.WriteLine();


// Call main methods
ComparePagination(context);


/// <summary>
/// Demonstrates bad vs good pagination using Skip() and Take().
/// </summary>
static void ComparePagination(AppDbContext context)
{
    int pageNumber = 2;
    int pageSize = 5;

    ShowBadPaginationApproach(context, pageNumber, pageSize);

    PrintSeparator();

    ShowGoodPaginationApproach(context, pageNumber, pageSize);
}


/// <summary>
/// Demonstrates bad pagination by loading all rows first,
/// then applying pagination in memory.
/// </summary>
static void ShowBadPaginationApproach(
    AppDbContext context,
    int pageNumber,
    int pageSize)
{
    Console.WriteLine("BAD APPROACH - Load Everything, Then Paginate");
    Console.WriteLine("--------------------------------------------");
    Console.WriteLine();

    // Build query first
    var badQuery =
        context.Students;

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(badQuery.ToQueryString());

    // Execute query and load ALL students into memory
    var allStudents = badQuery.ToList();

    Console.WriteLine($"Total Loaded Rows: {allStudents.Count}");
    Console.WriteLine("All rows were loaded into memory.");
    Console.WriteLine();

    // Pagination happens in memory here, not in SQL Server
    var badPage =
        allStudents
            .OrderBy(s => s.StudentId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

    Console.WriteLine($"Page {pageNumber} Results - BAD:");
    Console.WriteLine("--------------------------");

    Console.WriteLine();
    foreach (var student in badPage)
    {
        Console.WriteLine(
            $"{student.StudentId} - {student.FirstName} {student.LastName}");
    }

    Console.WriteLine();
    Console.WriteLine(
        "Only a few records are displayed, but the entire table was loaded first.");
    Console.WriteLine();
}


/// <summary>
/// Demonstrates good pagination by applying OrderBy(), Skip(),
/// and Take() directly in the database query.
/// </summary>
static void ShowGoodPaginationApproach(
    AppDbContext context,
    int pageNumber,
    int pageSize)
{
    Console.WriteLine("GOOD APPROACH - Paginate In The Database");
    Console.WriteLine("----------------------------------------");
    Console.WriteLine();

    // Build query first
    var goodQuery =
        context.Students
               .OrderBy(s => s.StudentId)
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize);

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(goodQuery.ToQueryString());

    // Execute query and load ONLY the required page
    var goodPage = goodQuery.ToList();

    Console.WriteLine($"Page {pageNumber} Results - GOOD:");
    Console.WriteLine("---------------------------");

    Console.WriteLine();
    foreach (var student in goodPage)
    {
        Console.WriteLine(
            $"{student.StudentId} - {student.FirstName} {student.LastName}");
    }

    Console.WriteLine();
    Console.WriteLine($"Loaded Rows: {goodPage.Count}");
    Console.WriteLine("Only the required page rows were loaded from the database.");
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
