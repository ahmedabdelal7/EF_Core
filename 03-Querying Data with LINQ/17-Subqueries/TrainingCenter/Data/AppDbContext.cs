using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using TrainingCenter.Entities;

namespace TrainingCenter.Data
{
    public partial class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options)
        {
        }

        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Enrollment> Enrollments { get; set; }
        public virtual DbSet<Instructor> Instructors { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<StudentProfile> StudentProfiles { get; set; }
        public object StudentProfile { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ==================================================
            // Configure Course Entity
            // ==================================================
            modelBuilder.Entity<Course>(entity =>
            {

                //entity.HasKey(e => e.Id);

                // Create index on InstructorId
                // Speeds searching courses by instructor
                entity.HasIndex(e => e.InstructorId, "IX_Courses_InstructorId");

                // Create index on Status
                entity.HasIndex(e => e.Status, "IX_Courses_Status");

                // Course Code must be unique
                // Example: C#101 cannot repeat
                entity.HasIndex(e => e.Code, "UQ_Courses_Code")
                      .IsUnique();

                // Maximum 30 characters
                entity.Property(e => e.Code)
                      .HasMaxLength(30);

                // Default value = current date/time
                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("(getdate())")
                      .HasColumnType("datetime");

                // Optional description max 500 chars
                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                // Beginner / Intermediate / Advanced
                entity.Property(e => e.Level)
                      .HasMaxLength(30);

                // Decimal with precision (10,2)
                entity.Property(e => e.Price)
                      .HasColumnType("decimal(10, 2)");
                      

                // Publish date
                entity.Property(e => e.PublishedAt)
                      .HasColumnType("datetime");

                // Active / Draft / Closed
                entity.Property(e => e.Status)
                      .HasMaxLength(20);

                // Course title max 150
                entity.Property(e => e.Title)
                      .HasMaxLength(150);

                // Relationship:
                // One Instructor has many Courses
                entity.HasOne(d => d.Instructor)
                      .WithMany(p => p.Courses)
                      .HasForeignKey(d => d.InstructorId)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK_Courses_Instructors");
            });

            // ==================================================
            // Configure Enrollment Entity
            // ==================================================
            modelBuilder.Entity<Enrollment>(entity =>
            {
                // Indexes for faster search
                entity.HasIndex(e => e.CourseId, "IX_Enrollments_CourseId");
                entity.HasIndex(e => e.Status, "IX_Enrollments_Status");
                entity.HasIndex(e => e.StudentId, "IX_Enrollments_StudentId");

                // Prevent duplicate enrollment:
                // Same student cannot enroll twice
                entity.HasIndex(e => new { e.StudentId, e.CourseId },
                                "UQ_Enrollments_StudentId_CourseId")
                      .IsUnique();

                // Completion date
                entity.Property(e => e.CompletionDate)
                      .HasColumnType("datetime");

                // Default enrollment date = now
                entity.Property(e => e.EnrollmentDate)
                      .HasDefaultValueSql("(getdate())")
                      .HasColumnType("datetime");

                // Decimal grades
                entity.Property(e => e.FinalGrade)
                      .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.ProgressPercent)
                      .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.Status)
                      .HasMaxLength(20);

                // Many enrollments belong to one course
                entity.HasOne(d => d.Course)
                      .WithMany(p => p.Enrollments)
                      .HasForeignKey(d => d.CourseId)
                      .HasConstraintName("FK_Enrollments_Courses");

                // Many enrollments belong to one student
                entity.HasOne(d => d.Student)
                      .WithMany(p => p.Enrollments)
                      .HasForeignKey(d => d.StudentId)
                      .HasConstraintName("FK_Enrollments_Students");
            });

            // ==================================================
            // Configure Instructor Entity
            // ==================================================
            modelBuilder.Entity<Instructor>(entity =>
            {
                // Self-reference manager relationship
                entity.HasIndex(e => e.ManagerId, "IX_Instructors_ManagerId");

                // Email must be unique
                entity.HasIndex(e => e.Email, "UQ_Instructors_Email")
                      .IsUnique();

                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.FirstName).HasMaxLength(50);

                // Default = active instructor
                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.Salary)
                      .HasColumnType("decimal(10, 2)");

                // One manager can manage many instructors
                entity.HasOne(d => d.Manager)
                      .WithMany(p => p.InverseManager)
                      .HasForeignKey(d => d.ManagerId)
                      .HasConstraintName("FK_Instructors_Manager");
            });

            // ==================================================
            // Configure Student Entity
            // ==================================================
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(e => e.Status, "IX_Students_Status");

                // Email unique
                entity.HasIndex(e => e.Email, "UQ_Students_Email")
                      .IsUnique();

                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.PhoneNumber).HasMaxLength(30);

                // Registration date defaults to now
                entity.Property(e => e.RegisteredAt)
                      .HasDefaultValueSql("(getdate())")
                      .HasColumnType("datetime");

                entity.Property(e => e.Status).HasMaxLength(20);
            });

            // ==================================================
            // Configure StudentProfile Entity
            // ==================================================
            modelBuilder.Entity<StudentProfile>(entity =>
            {
                // Primary key = StudentId
                entity.HasKey(e => e.StudentId);

                // Not auto increment
                entity.Property(e => e.StudentId)
                      .ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Bio).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.LinkedInUrl).HasMaxLength(200);

                // One Student has one Profile
                entity.HasOne(d => d.Student)
                      .WithOne(p => p.StudentProfile)
                      .HasForeignKey<StudentProfile>(d => d.StudentId)
                      .HasConstraintName("FK_StudentProfiles_Students");
            });

            // Optional extension point
            OnModelCreatingPartial(modelBuilder);
        }

        // Can be implemented in another file
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

}
