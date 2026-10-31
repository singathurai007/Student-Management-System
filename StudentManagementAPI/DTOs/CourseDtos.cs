using System.ComponentModel.DataAnnotations;

namespace StudentManagementAPI.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? Description { get; set; }
        public int EnrolledStudentCount { get; set; }
    }

    public class CreateCourseDto
    {
        [Required(ErrorMessage = "Course Code is required")]
        [StringLength(20, MinimumLength = 3)]
        [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Course code must be uppercase letters, numbers or hyphens")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        [StringLength(150, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Range(1, 6, ErrorMessage = "Credits must be between 1 and 6")]
        public int Credits { get; set; } = 3;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
