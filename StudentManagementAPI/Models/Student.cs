using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementAPI.Models
{
    public enum Gender
    {
        Male = 1,
        Female = 2,
        Other = 3
    }

    public enum StudentStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        Graduated = 4
    }

    /// <summary>
    /// Student domain entity
    /// </summary>
    public class Student : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string RegisterNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Year { get; set; }

        [Required]
        [Phone]
        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        public Gender Gender { get; set; } = Gender.Male;

        public StudentStatus Status { get; set; } = StudentStatus.Active;

        [Column(TypeName = "decimal(4,2)")]
        [Range(0.0, 10.0)]
        public decimal? CGPA { get; set; }

        // Navigation Properties (Relationships)
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
