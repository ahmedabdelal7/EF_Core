using System;
using System.Collections.Generic;
using System.Text;

namespace TrainingCenter.Entities
{
    public class Enrollment
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal ProgressPercent { get; set; }
        public decimal? FinalGrade { get; set; }
        public string Status { get; set; } = null!;
        //public string Status { get; set; } = string.Empty;


        public virtual Course Course { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;


    }
}
