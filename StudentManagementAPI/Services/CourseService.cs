using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentManagementAPI.Data;
using StudentManagementAPI.DTOs;
using StudentManagementAPI.Models;

namespace StudentManagementAPI.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<CourseDto>>> GetAllCoursesAsync()
        {
            var courses = await _context.Courses
                .AsNoTracking()
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    CourseCode = c.CourseCode,
                    Title = c.Title,
                    Department = c.Department,
                    Credits = c.Credits,
                    Description = c.Description,
                    EnrolledStudentCount = c.Enrollments.Count()
                })
                .OrderBy(c => c.CourseCode)
                .ToListAsync();

            return ApiResponse<List<CourseDto>>.Ok(courses);
        }

        public async Task<ApiResponse<CourseDto>> GetCourseByIdAsync(int id)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    CourseCode = c.CourseCode,
                    Title = c.Title,
                    Department = c.Department,
                    Credits = c.Credits,
                    Description = c.Description,
                    EnrolledStudentCount = c.Enrollments.Count()
                })
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return ApiResponse<CourseDto>.Fail($"Course with ID {id} was not found");
            }

            return ApiResponse<CourseDto>.Ok(course);
        }

        public async Task<ApiResponse<CourseDto>> CreateCourseAsync(CreateCourseDto dto)
        {
            var code = dto.CourseCode.Trim().ToUpperInvariant();
            if (await _context.Courses.AnyAsync(c => c.CourseCode == code))
            {
                return ApiResponse<CourseDto>.Fail($"A course with code '{code}' already exists.");
            }

            var course = new Course
            {
                CourseCode = code,
                Title = dto.Title.Trim(),
                Department = dto.Department.Trim(),
                Credits = dto.Credits,
                Description = dto.Description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            return ApiResponse<CourseDto>.Ok(new CourseDto
            {
                Id = course.Id,
                CourseCode = course.CourseCode,
                Title = course.Title,
                Department = course.Department,
                Credits = course.Credits,
                Description = course.Description,
                EnrolledStudentCount = 0
            }, "Course created successfully");
        }

        public async Task<ApiResponse<bool>> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
            {
                return ApiResponse<bool>.Fail($"Course with ID {id} was not found");
            }

            course.IsDeleted = true;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Course soft-deleted successfully");
        }

        public async Task<ApiResponse<EnrollmentDto>> EnrollStudentAsync(CreateEnrollmentDto dto)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == dto.StudentId);
            if (student == null)
            {
                return ApiResponse<EnrollmentDto>.Fail($"Student with ID {dto.StudentId} was not found");
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == dto.CourseId);
            if (course == null)
            {
                return ApiResponse<EnrollmentDto>.Fail($"Course with ID {dto.CourseId} was not found");
            }

            var semester = dto.Semester.Trim();
            var exists = await _context.Enrollments.AnyAsync(e =>
                e.StudentId == dto.StudentId && e.CourseId == dto.CourseId && e.Semester == semester);

            if (exists)
            {
                return ApiResponse<EnrollmentDto>.Fail($"Student '{student.Name}' is already enrolled in '{course.CourseCode}' for {semester}.");
            }

            var enrollment = new Enrollment
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId,
                Semester = semester,
                Grade = dto.Grade?.Trim() ?? "In Progress",
                EnrolledAt = DateTime.UtcNow
            };

            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            return ApiResponse<EnrollmentDto>.Ok(new EnrollmentDto
            {
                Id = enrollment.Id,
                StudentId = student.Id,
                StudentName = student.Name,
                RegisterNumber = student.RegisterNumber,
                CourseId = course.Id,
                CourseCode = course.CourseCode,
                CourseTitle = course.Title,
                Semester = enrollment.Semester,
                Grade = enrollment.Grade,
                EnrolledAt = enrollment.EnrolledAt
            }, "Student enrolled successfully");
        }

        public async Task<ApiResponse<List<EnrollmentDto>>> GetStudentEnrollmentsAsync(int studentId)
        {
            var enrollments = await _context.Enrollments
                .AsNoTracking()
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId)
                .Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student.Name,
                    RegisterNumber = e.Student.RegisterNumber,
                    CourseId = e.CourseId,
                    CourseCode = e.Course.CourseCode,
                    CourseTitle = e.Course.Title,
                    Semester = e.Semester,
                    Grade = e.Grade,
                    EnrolledAt = e.EnrolledAt
                })
                .ToListAsync();

            return ApiResponse<List<EnrollmentDto>>.Ok(enrollments);
        }

        public async Task<ApiResponse<List<EnrollmentDto>>> GetCourseEnrollmentsAsync(int courseId)
        {
            var enrollments = await _context.Enrollments
                .AsNoTracking()
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.CourseId == courseId)
                .Select(e => new EnrollmentDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = e.Student.Name,
                    RegisterNumber = e.Student.RegisterNumber,
                    CourseId = e.CourseId,
                    CourseCode = e.Course.CourseCode,
                    CourseTitle = e.Course.Title,
                    Semester = e.Semester,
                    Grade = e.Grade,
                    EnrolledAt = e.EnrolledAt
                })
                .ToListAsync();

            return ApiResponse<List<EnrollmentDto>>.Ok(enrollments);
        }
    }
}
