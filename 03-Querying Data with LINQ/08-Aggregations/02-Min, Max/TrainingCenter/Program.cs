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
ShowMinMax(context);


/// <summary>
/// Demonstrates Min() and Max() using TrainingCenterDB.
/// </summary>
static void ShowMinMax(AppDbContext context)
{
    Console.WriteLine("Min() and Max() Example");
    Console.WriteLine("-----------------------");
    Console.WriteLine();

    // --------------------------------------------------
    // Lowest Course Price
    // --------------------------------------------------

    // Build query first
    var coursePricesQuery =
        context.Courses
               .Select(c => c.Price);

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(coursePricesQuery.ToQueryString());

    // Execute query
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for Min().
    decimal lowestPrice =
        coursePricesQuery.Min();

    // Execute query
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for Max().
    decimal highestPrice =
        coursePricesQuery.Max();

    // --------------------------------------------------
    // Earliest Registration Date
    // --------------------------------------------------

    // Build query first
    var registrationDatesQuery =
        context.Students
               .Select(s => s.RegisteredAt);

    // Preview SQL query shape
    PreviewSQLUsingToQueryString(registrationDatesQuery.ToQueryString());

    // Execute query
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for Min().
    DateTime earliestRegistration =
        registrationDatesQuery.Min();

    // Print readable output
    Console.WriteLine($"Lowest Course Price     : {lowestPrice}");
    Console.WriteLine($"Highest Course Price    : {highestPrice}");
    Console.WriteLine($"Earliest Registration   : {earliestRegistration:d}");
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
