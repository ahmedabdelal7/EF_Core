
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrainingCenter.Data;


// Configuration
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();


string? connectionString =
    configuration.GetConnectionString("DefaultConnection");


if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Connection string not found.");
    return;
}


// DbContext
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(connectionString)
    .Options;


using var context = new AppDbContext(options);


Console.WriteLine("Connected successfully.");
Console.WriteLine();


ShowStudents(context);
Console.WriteLine("\n----------------------------------------\n");
ShowStudentsWithAutoTracking(context);

static void ShowStudents(AppDbContext context)
{
    var students = context.Students
        .AsNoTracking()
        .Where(s => s.Status == "Active")
        .OrderBy(s => s.StudentId)
        .ToList();


    foreach (var student in students)
    {
        Console.WriteLine(
            $"{student.StudentId} - {student.FirstName} {student.LastName}");


    }
            
    Console.WriteLine($"\nStudents Count: {students.Count}");
    Console.WriteLine($"Tracked Students Count: {context.ChangeTracker.Entries().Count()}");
}

static void ShowStudentsWithAutoTracking(AppDbContext context)
{
    var students = context.Students
        .Where(s => s.Status == "Active")
        .OrderBy(s => s.StudentId)
        .ToList();


    foreach (var student in students)
    {
        Console.WriteLine(
            $"{student.StudentId} - {student.FirstName} {student.LastName}");


    }
    Console.WriteLine($"\nStudents Count: {students.Count}");
    Console.WriteLine($"Tracked Students Count: {context.ChangeTracker.Entries().Count()}");
}
