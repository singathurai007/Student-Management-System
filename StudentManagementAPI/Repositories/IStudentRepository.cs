using System.Collections.Generic;
using System.Threading.Tasks;
using StudentManagementAPI.DTOs;
using StudentManagementAPI.Models;

namespace StudentManagementAPI.Repositories
{
    public interface IStudentRepository
    {
        Task<PagedResult<Student>> GetAllAsync(PaginationParams @params);
        Task<Student?> GetByIdAsync(int id);
        Task<Student?> GetByRegisterNumberAsync(string registerNumber);
        Task<Student?> GetByEmailAsync(string email);
        Task<Student> AddAsync(Student student);
        Task<Student> UpdateAsync(Student student);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByRegisterNumberAsync(string registerNumber, int? excludeId = null);
        Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<List<Student>> GetAllForExportAsync();
    }
}

