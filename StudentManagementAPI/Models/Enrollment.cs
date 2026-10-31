using System;
using System.ComponentModel.DataAnnotations;

namespace StudentManagementAPI.Models
{
    /// <summary>
    /// Student course enrollment join entity (Many-to-Many with payload)
    /// </summary>
    public class Enrollment : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        [MaxLength(20)]
        public string Semester { get; set; } = "Fall 2024";

        [MaxLength(5)]
        public string? Grade { get; set; } // e.g. "A+", "A", "B", "In Progress"

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}
