using System.Collections.Generic;
using System.Threading.Tasks;
using StudentManagementAPI.DTOs;

namespace StudentManagementAPI.Services
{
    public interface ICourseService
    {
        Task<ApiResponse<List<CourseDto>>> GetAllCoursesAsync();
        Task<ApiResponse<CourseDto>> GetCourseByIdAsync(int id);
        Task<ApiResponse<CourseDto>> CreateCourseAsync(CreateCourseDto dto);
        Task<ApiResponse<bool>> DeleteCourseAsync(int id);
        Task<ApiResponse<EnrollmentDto>> EnrollStudentAsync(CreateEnrollmentDto dto);
        Task<ApiResponse<List<EnrollmentDto>>> GetStudentEnrollmentsAsync(int studentId);
        Task<ApiResponse<List<EnrollmentDto>>> GetCourseEnrollmentsAsync(int courseId);
    }
}
