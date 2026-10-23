using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingCenter.Entities
{
    public class Instructor
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public Decimal Salary { get; set; }
        public int? ManagerId { get; set; }
        
        public int IsActive { get; set; }      
        public string? PhoneNumber { get; set; }

        // One instructor can teach many courses.
        public virtual ICollection<Course> Courses { get; set; } =
            new List<Course>();

        // Points to this instructor's manager.
        public virtual Instructor? Manager { get; set; }

        // Collection of instructors managed by this instructor.
        public virtual ICollection<Instructor> InverseManager { get; set; } =
            new List<Instructor>();

    }
}
