using System;
using System.ComponentModel.DataAnnotations;

namespace StudentManagementAPI.DTOs
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RegisterNumber { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string? Grade { get; set; }
        public DateTime EnrolledAt { get; set; }
    }

    public class CreateEnrollmentDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(20)]
        public string Semester { get; set; } = "Fall 2024";

        [StringLength(5)]
        public string? Grade { get; set; } = "In Progress";
    }
}
