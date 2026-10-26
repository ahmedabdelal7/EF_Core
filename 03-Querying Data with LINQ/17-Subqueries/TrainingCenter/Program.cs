using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrainingCenter.Data;


internal class Program
{
    private static void Main(string[] args)
    {
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
        ShowExpensiveCourses(context);
    }

    /// <summary>
    /// Shows courses priced above the average course price using a subquery.
    /// </summary>
    static void ShowExpensiveCourses(AppDbContext context)
    {
        Console.WriteLine("Courses Priced Above Average");
        Console.WriteLine("----------------------------");
        Console.WriteLine();

        // Build query first
        var query =
            context.Courses
                   .Where(c =>
                       c.Price >
                       context.Courses.Average(x => x.Price))
                   .OrderBy(c => c.Price);

        // Preview SQL before execution
        PreviewSQLUsingToQueryString(query.ToQueryString());

        // Execute query
        // ToQueryString previews query shape,
        // runtime logging shows actual executed SQL for Average().
        var courses = query.ToList();

        // Print readable output
        Console.WriteLine("\nExpensive Courses:");
        Console.WriteLine("------------------");


        Console.WriteLine();
        foreach (var course in courses)
        {
            Console.WriteLine(
                $"{course.Code} - {course.Title} - {course.Price}");
        }

        Console.WriteLine();
        Console.WriteLine($"Total Courses: {courses.Count}");
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
}