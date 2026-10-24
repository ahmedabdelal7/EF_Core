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
ShowReports(context);


/// <summary>
/// Demonstrates Distinct() and GroupBy() reporting queries.
/// </summary>
static void ShowReports(AppDbContext context)
{
    ShowStudentsPerStatusHaving(context);

    Console.WriteLine();

    Exersise1(context);
}




/// <summary>
/// Shows number of students grouped by status.
/// </summary>
/// <summary>
/// Shows statuses having more than 2 students.
/// </summary>
static void ShowStudentsPerStatusHaving(AppDbContext context)
{
    Console.WriteLine("Statuses With More Than 2 Students");
    Console.WriteLine("----------------------------------");

    // Build query first
    var query =
        context.Students
               .GroupBy(s => s.Status)
               .Where(g => g.Count() > 2)
               .Select(g => new
               {
                   Status = g.Key,
                   TotalStudents = g.Count()
               })
               .OrderBy(x => x.Status);

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(
        query.ToQueryString());

    // Execute query
    var report = query.ToList();

    Console.WriteLine();

    foreach (var row in report)
    {
        Console.WriteLine(
            $"{row.Status} : {row.TotalStudents}");
    }
}

static void Exersise1(AppDbContext context)
{
    Console.WriteLine("Ex-1");
    Console.WriteLine("-------------------");

    // Build query first
    var query = context.Courses
                       .GroupBy(c => c.Level)
                       .Where(g => g.Average(c => c.Price) > 30)
                       .Select(g => new
                       {
                           LevelName = g.Key,
                           CoursesCount = g.Count(),
                           TotalRevenue = g.Sum(c => c.Price),
                           AvaragePrice = g.Average(c => c.Price),
                           HighestPrice = g.Max(c => c.Price ),
                           LowestPrice = g.Min(c => c.Price ),
                           TotalInstructors = g.Select(c => c.InstructorId)
                                               .Distinct()
                                               .Count()
                       });
     


    // Preview SQL before execution
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for Count().
    var report = query.ToList();

    Console.WriteLine();
    // Print readable output
    foreach (var row in report)
    {
        Console.WriteLine($"{row.LevelName} : " +
            $"{row.CoursesCount} : " +
            $"{row.TotalRevenue} : " +
            $"{row.AvaragePrice} : " +
            $"{row.HighestPrice} : " +
            $"{row.LowestPrice} : " +
            $"{row.TotalInstructors}");
            
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
