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
        from s in context.Students
        join p in context.StudentProfiles
        on s.StudentId equals p.StudentId
        into StudentsGroup 
        from p in StudentsGroup.DefaultIfEmpty()
        select new
        {
            s.StudentId,
            StudentName = s.FirstName + " " + s.LastName,
            Country = p != null ? p.Country : "No Profile" ,
            City = p != null? p.City : "No Profile"
        };

    // Apply sorting
    var query =
        report.OrderBy(x => x.StudentId);

    // Preview SQL before execution
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    var result = query.ToList();

    // Print readable output
    Console.WriteLine("Student Report:");
    Console.WriteLine("---------------");

    Console.WriteLine();
    foreach (var row in result)
    {
        Console.WriteLine(
            $"{row.StudentId} - {row.StudentName} - {row.City} - {row.Country}");
    }

    Console.WriteLine();
    Console.WriteLine($"Total Students: {result.Count}");
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