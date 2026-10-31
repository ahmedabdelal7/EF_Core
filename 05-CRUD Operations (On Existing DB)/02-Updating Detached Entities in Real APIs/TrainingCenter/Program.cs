
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

// Create options with logging
var options =
    new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .Options;

// Create context
using var context = new AppDbContext(options);

// Test connection
if (!context.Database.CanConnect())
{
    Console.WriteLine("Could not connect to TrainingCenterDB.");
    return;
}

Console.WriteLine("Connected successfully.");
Console.WriteLine();

// Simulate API DTO
var dto = new UpdateStudentDto
{
    StudentId = 1,
    FirstName = "UpdatedName",
    PhoneNumber = "0799999999"
};

// Run examples
UpdateUsingLoadThenUpdate(context, dto);

context.ChangeTracker.Clear();

Console.WriteLine(new string('=', 70));

UpdateUsingControlledAttach(context, dto);

context.ChangeTracker.Clear();

Console.WriteLine(new string('=', 70));

UpdateUsingAttach(context, dto);


/// <summary>
/// Pattern 1: Load Then Update (Recommended)
/// </summary>
static void UpdateUsingLoadThenUpdate(
    AppDbContext context,
    UpdateStudentDto dto)
{
    Console.WriteLine("\n\nPattern 1 - Load Then Update");
    Console.WriteLine("----------------------------");

    var student = context.Students
        .FirstOrDefault(s => s.StudentId == dto.StudentId);

    if (student == null)
    {
        Console.WriteLine("Student not found.");
        return;
    }

    student.FirstName = dto.FirstName;
    student.PhoneNumber = dto.PhoneNumber;

    context.SaveChanges();

    Console.WriteLine("Updated using Load-Then-Update.");
}


/// <summary>
/// Pattern 2: Controlled Attach (Update specific fields only)
/// </summary>
static void UpdateUsingControlledAttach(
    AppDbContext context,
    UpdateStudentDto dto)
{
    Console.WriteLine("\n\nPattern 2 - Controlled Attach");
    Console.WriteLine("-----------------------------");

    var student = new Student
    {
        StudentId = dto.StudentId,
        PhoneNumber = dto.PhoneNumber
    };

    context.Attach(student);

    context.Entry(student)
           .Property(s => s.PhoneNumber)
           .IsModified = true;  

    context.SaveChanges();

    Console.WriteLine("Updated using Attach + IsModified.");
}


/// <summary>
/// Pattern 3: Direct Update using Update()
/// ⚠️ Dangerous with partial DTO
/// </summary>
static void UpdateUsingAttach(
    AppDbContext context,
    UpdateStudentDto dto)
{
    Console.WriteLine("\n\nPattern 3 - Update() (Full Update)");
    Console.WriteLine("----------------------------------");

    var student = new Student
    {
        StudentId = dto.StudentId,
        FirstName = dto.FirstName,
        PhoneNumber = dto.PhoneNumber
    };

    try
    {
        context.Students.Update(student);
        context.SaveChanges();

        Console.WriteLine("Updated using Update().");
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine("\n\n Update() failed due to missing required fields.");
        Console.WriteLine("Reason:");
        Console.WriteLine(ex.InnerException?.Message);
    }
}


/// <summary>
/// Simulates data coming from API request
/// </summary>
public class UpdateStudentDto
{
    public int StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}