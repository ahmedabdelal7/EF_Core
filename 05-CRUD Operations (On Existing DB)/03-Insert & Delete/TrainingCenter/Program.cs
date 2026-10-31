using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrainingCenter.Data;
using TrainingCenter.Entities;


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
    Console.WriteLine("Database connection failed.");
    return;
}

Console.WriteLine("Connected successfully.");
Console.WriteLine();


// Call main methods
int newStudentId = InsertStudent(context);

PrintSeparator();

DeleteStudent(context, newStudentId);


/// <summary>
/// Inserts a new student and returns the generated StudentId.
/// </summary>
static int InsertStudent(AppDbContext context)
{
    Console.WriteLine("INSERT EXAMPLE");
    Console.WriteLine("--------------");
    Console.WriteLine();

    // Create a new Student entity in memory
    var newStudent = new Student
    {
        FirstName = "Ali",
        LastName = "Hassan",
        Email = "ali.hassan@example.com",
        Status = "Active",
        RegisteredAt = DateTime.Now
    };

    // Add() tells EF Core to track this entity as Added
    context.Students.Add(newStudent);
    Console.WriteLine("Student added to Change Tracker as Added.");
    Console.WriteLine("INSERT does not support ToQueryString().");
    Console.WriteLine("Runtime logging will show the actual executed INSERT SQL.");
    Console.WriteLine();
    Console.WriteLine($"Entity status : {context.Entry(newStudent).State.ToString()}");
    Console.WriteLine();

    // Execute INSERT
    int affectedRows = context.SaveChanges();

    Console.WriteLine($"Affected Rows       : {affectedRows}");
    Console.WriteLine($"Generated Student ID: {newStudent.StudentId}");
    Console.WriteLine("Student inserted successfully.");
    Console.WriteLine();

    return newStudent.StudentId;
}


/// <summary>
/// Deletes a student by StudentId.
/// </summary>
static void DeleteStudent(AppDbContext context, int studentId)
{
    Console.WriteLine("DELETE EXAMPLE");
    Console.WriteLine("--------------");
    Console.WriteLine();

    // Build query first
    var query =
        context.Students
               .Where(s => s.StudentId == studentId);

    // Preview SELECT SQL before execution
    PreviewSQLUsingToQueryString(query.ToQueryString());

    // Execute query
    // ToQueryString previews query shape,
    // runtime logging shows actual executed SQL for FirstOrDefault().
    var student = query.FirstOrDefault();

    if (student == null)
    {
        Console.WriteLine($"No student found with ID = {studentId}");
        return;
    }

    Console.WriteLine(
        $"Deleting Student: {student.FirstName} {student.LastName} (ID = {student.StudentId})");

    // Remove() tells EF Core to track this entity as Deleted
    context.Students.Remove(student);

    Console.WriteLine();
    Console.WriteLine("Student marked as Deleted in Change Tracker.");
    Console.WriteLine("DELETE does not support ToQueryString().");
    Console.WriteLine("Runtime logging will show the actual executed DELETE SQL.");
    Console.WriteLine();
    Console.WriteLine($"Entity Status: {context.Entry(student).State.ToString()}");
    Console.WriteLine();

    // Execute DELETE
    int affectedRows = context.SaveChanges();

    Console.WriteLine($"Affected Rows: {affectedRows}");
    Console.WriteLine("Student deleted successfully.");
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