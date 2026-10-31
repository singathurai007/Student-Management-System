using System.Collections.Generic;
using System.Threading.Tasks;
using StudentManagementAPI.DTOs;

namespace StudentManagementAPI.Services
{
    public interface IStudentService
    {
        Task<ApiResponse<PagedResult<StudentResponseDto>>> GetAllStudentsAsync(PaginationParams @params);
        Task<ApiResponse<StudentResponseDto>> GetStudentByIdAsync(int id);
        Task<ApiResponse<StudentResponseDto>> GetStudentByRegisterNumberAsync(string registerNumber);
        Task<ApiResponse<StudentResponseDto>> CreateStudentAsync(CreateStudentDto dto);
        Task<ApiResponse<StudentResponseDto>> UpdateStudentAsync(int id, UpdateStudentDto dto);
        Task<ApiResponse<bool>> DeleteStudentAsync(int id);
        Task<ApiResponse<DashboardStatsDto>> GetDashboardAnalyticsAsync();
        Task<byte[]> ExportStudentsToCsvAsync();
    }
}

