using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentManagementAPI.Models
{
    /// <summary>
    /// Academic Course entity
    /// </summary>
    public class Course : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string CourseCode { get; set; } = string.Empty; // e.g. CS101, IT202

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Range(1, 6)]
        public int Credits { get; set; } = 3;

        [MaxLength(500)]
        public string? Description { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
