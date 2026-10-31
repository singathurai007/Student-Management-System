using System.ComponentModel.DataAnnotations;
using StudentManagementAPI.Models;

namespace StudentManagementAPI.DTOs
{
    /// <summary>
    /// Payload required to create a new student
    /// </summary>
    public class CreateStudentDto
    {
        [Required(ErrorMessage = "Student name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Register number is required")]
        [RegularExpression(@"^[A-Za-z0-9]{4,20}$", ErrorMessage = "Register number must be 4 to 20 alphanumeric characters")]
        public string RegisterNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Year must be between 1 and 5")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Phone number must be a valid 10-15 digit phone number")]
        public string Phone { get; set; } = string.Empty;

        public Gender Gender { get; set; } = Gender.Male;

        public StudentStatus Status { get; set; } = StudentStatus.Active;

        [Range(0.0, 10.0, ErrorMessage = "CGPA must be between 0.00 and 10.00")]
        public decimal? CGPA { get; set; }
    }
}

