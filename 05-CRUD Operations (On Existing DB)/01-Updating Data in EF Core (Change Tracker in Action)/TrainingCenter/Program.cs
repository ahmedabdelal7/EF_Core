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
UpdateStudent(context);


/// <summary>
/// Demonstrates bad and good update approaches in EF Core.
/// </summary>
static void UpdateStudent(AppDbContext context)
{
    ShowBadUpdateUsingAsNoTracking(context);

    PrintSeparator();

    ShowGoodUpdateUsingTrackedEntity(context);
}


/// <summary>
/// Demonstrates why AsNoTracking() should not be used when you want EF Core to track updates.
/// </summary>
static void ShowBadUpdateUsingAsNoTracking(AppDbContext context)
{
    Console.WriteLine("BAD APPROACH - Update Using AsNoTracking()");
    Console.WriteLine("------------------------------------------");
    Console.WriteLine();

    // Build query first
    var badQuery =
        context.Students
               .AsNoTracking()
               .Where(s => s.StudentId == 1);

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(badQuery.ToQueryString());

    // Execute query
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for FirstOrDefault().
    var badStudent = badQuery.FirstOrDefault();

    if (badStudent == null)
    {
        Console.WriteLine("Student not found.");
        return;
    }

    Console.WriteLine($"Original Name: {badStudent.FirstName}");

    // Modify property in memory only
    badStudent.FirstName = "UpdatedName";

    Console.WriteLine($"Changed Name In Memory: {badStudent.FirstName}");

    // SaveChanges() will not update this entity because it is not tracked
    int affectedRows = context.SaveChanges();

    Console.WriteLine();
    Console.WriteLine($"Affected Rows: {affectedRows}");
    Console.WriteLine("Nothing was updated because the entity was loaded using AsNoTracking().");
    Console.WriteLine();
}


/// <summary>
/// Demonstrates the correct update approach using a tracked entity.
/// </summary>
static void ShowGoodUpdateUsingTrackedEntity(AppDbContext context)
{
    Console.WriteLine("GOOD APPROACH - Update Using Tracked Entity");
    Console.WriteLine("-------------------------------------------");
    Console.WriteLine();

    // Build query first
    var goodQuery =
        context.Students
               .Where(s => s.StudentId == 1);

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(goodQuery.ToQueryString());

    // Execute query
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for FirstOrDefault().
    var student = goodQuery.FirstOrDefault();

    if (student == null)
    {
        Console.WriteLine("Student not found.");
        return;
    }

    Console.WriteLine($"Before Update: {student.FirstName}");

    // Modify property while entity is tracked by EF Core
    student.FirstName = "Ali Updated";

    Console.WriteLine($"After Change In Memory: {student.FirstName}");

    // Save changes
    // Runtime logging shows the actual executed UPDATE SQL.
    int affectedRows = context.SaveChanges();

    Console.WriteLine();
    Console.WriteLine($"Affected Rows: {affectedRows}");
    Console.WriteLine("Changes saved to database.");
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