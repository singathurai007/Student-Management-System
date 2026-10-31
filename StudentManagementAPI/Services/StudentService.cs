using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentManagementAPI.DTOs;
using StudentManagementAPI.Models;
using StudentManagementAPI.Repositories;

namespace StudentManagementAPI.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository;

        public StudentService(IStudentRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResult<StudentResponseDto>>> GetAllStudentsAsync(PaginationParams @params)
        {
            var pagedEntities = await _repository.GetAllAsync(@params);

            var dtos = pagedEntities.Items.Select(MapToResponseDto).ToList();

            var result = new PagedResult<StudentResponseDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                PageNumber = pagedEntities.PageNumber,
                PageSize = pagedEntities.PageSize
            };

            return ApiResponse<PagedResult<StudentResponseDto>>.Ok(result, "Students retrieved successfully");
        }

        public async Task<ApiResponse<StudentResponseDto>> GetStudentByIdAsync(int id)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null)
            {
                return ApiResponse<StudentResponseDto>.Fail($"Student with ID {id} was not found");
            }

            return ApiResponse<StudentResponseDto>.Ok(MapToResponseDto(student));
        }

        public async Task<ApiResponse<StudentResponseDto>> GetStudentByRegisterNumberAsync(string registerNumber)
        {
            var student = await _repository.GetByRegisterNumberAsync(registerNumber);
            if (student == null)
            {
                return ApiResponse<StudentResponseDto>.Fail($"Student with register number '{registerNumber}' was not found");
            }

            return ApiResponse<StudentResponseDto>.Ok(MapToResponseDto(student));
        }

        public async Task<ApiResponse<StudentResponseDto>> CreateStudentAsync(CreateStudentDto dto)
        {
            var normalizedRegNo = dto.RegisterNumber.Trim().ToUpperInvariant();
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            // Check if register number exists
            if (await _repository.ExistsByRegisterNumberAsync(normalizedRegNo))
            {
                return ApiResponse<StudentResponseDto>.Fail($"A student with register number '{normalizedRegNo}' already exists.");
            }

            // Check if email exists
            if (await _repository.ExistsByEmailAsync(normalizedEmail))
            {
                return ApiResponse<StudentResponseDto>.Fail($"A student with email '{normalizedEmail}' already exists.");
            }

            var student = new Student
            {
                Name = dto.Name.Trim(),
                RegisterNumber = normalizedRegNo,
                Email = normalizedEmail,
                Department = dto.Department.Trim(),
                Year = dto.Year,
                Phone = dto.Phone.Trim(),
                Gender = dto.Gender,
                Status = dto.Status,
                CGPA = dto.CGPA
            };

            var added = await _repository.AddAsync(student);
            return ApiResponse<StudentResponseDto>.Ok(MapToResponseDto(added), "Student registered successfully");
        }

        public async Task<ApiResponse<StudentResponseDto>> UpdateStudentAsync(int id, UpdateStudentDto dto)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null)
            {
                return ApiResponse<StudentResponseDto>.Fail($"Student with ID {id} was not found");
            }

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            // Check unique email among other students
            if (await _repository.ExistsByEmailAsync(normalizedEmail, id))
            {
                return ApiResponse<StudentResponseDto>.Fail($"Another student is already registered with email '{normalizedEmail}'.");
            }

            student.Name = dto.Name.Trim();
            student.Email = normalizedEmail;
            student.Department = dto.Department.Trim();
            student.Year = dto.Year;
            student.Phone = dto.Phone.Trim();
            student.Gender = dto.Gender;
            student.Status = dto.Status;
            student.CGPA = dto.CGPA;

            var updated = await _repository.UpdateAsync(student);
            return ApiResponse<StudentResponseDto>.Ok(MapToResponseDto(updated), "Student updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteStudentAsync(int id)
        {
            var exists = await _repository.ExistsAsync(id);
            if (!exists)
            {
                return ApiResponse<bool>.Fail($"Student with ID {id} was not found");
            }

            var deleted = await _repository.SoftDeleteAsync(id);
            return ApiResponse<bool>.Ok(deleted, "Student removed successfully (Soft Deleted)");
        }

        public async Task<ApiResponse<DashboardStatsDto>> GetDashboardAnalyticsAsync()
        {
            var stats = await _repository.GetDashboardStatsAsync();
            return ApiResponse<DashboardStatsDto>.Ok(stats, "Analytics retrieved successfully");
        }

        public async Task<byte[]> ExportStudentsToCsvAsync()
        {
            var students = await _repository.GetAllForExportAsync();
            var sb = new StringBuilder();

            // CSV Header
            sb.AppendLine("Id,Name,RegisterNumber,Email,Department,Year,Phone,Gender,Status,CGPA,CreatedAt");

            foreach (var s in students)
            {
                sb.AppendLine($"\"{s.Id}\",\"{EscapeCsv(s.Name)}\",\"{EscapeCsv(s.RegisterNumber)}\",\"{EscapeCsv(s.Email)}\",\"{EscapeCsv(s.Department)}\",\"{s.Year}\",\"{EscapeCsv(s.Phone)}\",\"{s.Gender}\",\"{s.Status}\",\"{s.CGPA}\",\"{s.CreatedAt:yyyy-MM-dd HH:mm}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string EscapeCsv(string value)
        {
            return value.Replace("\"", "\"\"");
        }

        private static StudentResponseDto MapToResponseDto(Student student)
        {
            return new StudentResponseDto
            {
                Id = student.Id,
                Name = student.Name,
                RegisterNumber = student.RegisterNumber,
                Email = student.Email,
                Department = student.Department,
                Year = student.Year,
                Phone = student.Phone,
                Gender = student.Gender.ToString(),
                Status = student.Status.ToString(),
                CGPA = student.CGPA,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt
            };
        }
    }
}

