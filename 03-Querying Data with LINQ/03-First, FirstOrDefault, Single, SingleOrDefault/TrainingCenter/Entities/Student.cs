using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingCenter.Entities
{
    public class Student
    {
        public int StudentId { get; set; }
        public string FirstName {  get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime RegisteredAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }


        public virtual StudentProfile? StudentProfile { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; } = 
            new List<Enrollment>();
             
    }
}
