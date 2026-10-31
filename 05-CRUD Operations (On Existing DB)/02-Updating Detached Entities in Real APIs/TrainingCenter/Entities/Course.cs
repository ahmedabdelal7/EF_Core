using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingCenter.Entities
{
    public partial class Course
    {
        public int CourseId { get; set; }
        //public string Title { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Level {  get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int InstructorId { get; set; }



        public ICollection<Enrollment> Enrollments { get; set; } =
            new List<Enrollment>();
        public Instructor Instructor = null!;

    }
}
