using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentManagementAPI.Data;
using StudentManagementAPI.DTOs;
using StudentManagementAPI.Models;

namespace StudentManagementAPI.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Student>> GetAllAsync(PaginationParams @params)
        {
            var query = _context.Students.AsNoTracking().AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(@params.SearchTerm))
            {
                var search = @params.SearchTerm.Trim().ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(search) ||
                    s.RegisterNumber.ToLower().Contains(search) ||
                    s.Email.ToLower().Contains(search) ||
                    s.Department.ToLower().Contains(search));
            }

            // Department filter
            if (!string.IsNullOrWhiteSpace(@params.Department))
            {
                query = query.Where(s => s.Department.ToLower() == @params.Department.Trim().ToLower());
            }

            // Year filter
            if (@params.Year.HasValue && @params.Year.Value > 0)
            {
                query = query.Where(s => s.Year == @params.Year.Value);
            }

            // Sorting
            query = @params.SortBy?.ToLower() switch
            {
                "regno" or "registernumber" => @params.IsDescending ? query.OrderByDescending(s => s.RegisterNumber) : query.OrderBy(s => s.RegisterNumber),
                "year" => @params.IsDescending ? query.OrderByDescending(s => s.Year) : query.OrderBy(s => s.Year),
                "cgpa" => @params.IsDescending ? query.OrderByDescending(s => s.CGPA) : query.OrderBy(s => s.CGPA),
                "createdat" => @params.IsDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
                _ => @params.IsDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((@params.PageNumber - 1) * @params.PageSize)
                .Take(@params.PageSize)
                .ToListAsync();

            return new PagedResult<Student>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = @params.PageNumber,
                PageSize = @params.PageSize
            };
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Student?> GetByRegisterNumberAsync(string registerNumber)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.RegisterNumber.ToLower() == registerNumber.Trim().ToLower());
        }

        public async Task<Student?> GetByEmailAsync(string email)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.Email.ToLower() == email.Trim().ToLower());
        }

        public async Task<Student> AddAsync(Student student)
        {
            student.CreatedAt = DateTime.UtcNow;
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
            return student;
        }

        public async Task<Student> UpdateAsync(Student student)
        {
            student.UpdatedAt = DateTime.UtcNow;
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
            return student;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
            {
                return false;
            }

            student.IsDeleted = true;
            student.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Students.AnyAsync(s => s.Id == id);
        }

        public async Task<bool> ExistsByRegisterNumberAsync(string registerNumber, int? excludeId = null)
        {
            var cleanReg = registerNumber.Trim().ToLower();
            return await _context.Students.AnyAsync(s => s.RegisterNumber.ToLower() == cleanReg && (!excludeId.HasValue || s.Id != excludeId.Value));
        }

        public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
        {
            var cleanEmail = email.Trim().ToLower();
            return await _context.Students.AnyAsync(s => s.Email.ToLower() == cleanEmail && (!excludeId.HasValue || s.Id != excludeId.Value));
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var allStudents = await _context.Students.AsNoTracking().ToListAsync();

            var totalStudents = allStudents.Count;
            var activeStudents = allStudents.Count(s => s.Status == StudentStatus.Active);
            var departments = allStudents.Select(s => s.Department).Distinct().Count();
            var avgCgpa = allStudents.Any(s => s.CGPA.HasValue)
                ? (double)allStudents.Where(s => s.CGPA.HasValue).Average(s => s.CGPA!.Value)
                : 0.0;

            var deptDist = allStudents
                .GroupBy(s => s.Department)
                .ToDictionary(g => g.Key, g => g.Count());

            var yearDist = allStudents
                .GroupBy(s => $"Year {s.Year}")
                .ToDictionary(g => g.Key, g => g.Count());

            var recent = allStudents
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(s => new StudentResponseDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    RegisterNumber = s.RegisterNumber,
                    Email = s.Email,
                    Department = s.Department,
                    Year = s.Year,
                    Phone = s.Phone,
                    Gender = s.Gender.ToString(),
                    Status = s.Status.ToString(),
                    CGPA = s.CGPA,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToList();

            return new DashboardStatsDto
            {
                TotalStudents = totalStudents,
                ActiveStudents = activeStudents,
                TotalDepartments = departments,
                AverageCGPA = Math.Round(avgCgpa, 2),
                DepartmentDistribution = deptDist,
                YearDistribution = yearDist,
                RecentStudents = recent
            };
        }

        public async Task<List<Student>> GetAllForExportAsync()
        {
            return await _context.Students.AsNoTracking().OrderBy(s => s.RegisterNumber).ToListAsync();
        }
    }
}

