using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentManagementAPI.Data;
using StudentManagementAPI.DTOs;
using StudentManagementAPI.Models;
using StudentManagementAPI.Repositories;

namespace StudentManagementAPI.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository;
        private readonly ApplicationDbContext _context;

        public StudentService(IStudentRepository repository, ApplicationDbContext context)
        {
            _repository = repository;
            _context = context;
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

            if (await _repository.ExistsByRegisterNumberAsync(normalizedRegNo))
            {
                return ApiResponse<StudentResponseDto>.Fail($"A student with register number '{normalizedRegNo}' already exists.");
            }

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

            // Audit Log
            await LogAuditAsync("CREATE", "Student", added.Id, $"Registered student {added.Name} ({added.RegisterNumber})");

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

            // Audit Log
            await LogAuditAsync("UPDATE", "Student", updated.Id, $"Updated student details for {updated.Name}");

            return ApiResponse<StudentResponseDto>.Ok(MapToResponseDto(updated), "Student updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteStudentAsync(int id)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null)
            {
                return ApiResponse<bool>.Fail($"Student with ID {id} was not found");
            }

            var deleted = await _repository.SoftDeleteAsync(id);

            // Audit Log
            await LogAuditAsync("SOFT_DELETE", "Student", id, $"Soft deleted student {student.Name} ({student.RegisterNumber})");

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

            sb.AppendLine("Id,Name,RegisterNumber,Email,Department,Year,Phone,Gender,Status,CGPA,CreatedAt");

            foreach (var s in students)
            {
                sb.AppendLine($"\"{s.Id}\",\"{EscapeCsv(s.Name)}\",\"{EscapeCsv(s.RegisterNumber)}\",\"{EscapeCsv(s.Email)}\",\"{EscapeCsv(s.Department)}\",\"{s.Year}\",\"{EscapeCsv(s.Phone)}\",\"{s.Gender}\",\"{s.Status}\",\"{s.CGPA}\",\"{s.CreatedAt:yyyy-MM-dd HH:mm}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<ApiResponse<BulkImportResultDto>> BulkImportStudentsFromCsvAsync(Stream fileStream)
        {
            var result = new BulkImportResultDto();
            var studentsToAdd = new List<Student>();

            using var reader = new StreamReader(fileStream);
            string? headerLine = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return ApiResponse<BulkImportResultDto>.Fail("CSV file is empty");
            }

            int lineIndex = 1;
            while (!reader.EndOfStream)
            {
                lineIndex++;
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                result.TotalProcessed++;

                // Format: Name,RegisterNumber,Email,Department,Year,Phone,CGPA
                var parts = line.Split(',');
                if (parts.Length < 6)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Line {lineIndex}: Insufficient columns (expected at least 6)");
                    continue;
                }

                var name = parts[0].Trim().Trim('"');
                var regNo = parts[1].Trim().Trim('"').ToUpperInvariant();
                var email = parts[2].Trim().Trim('"').ToLowerInvariant();
                var dept = parts[3].Trim().Trim('"');
                if (!int.TryParse(parts[4].Trim().Trim('"'), out int year) || year < 1 || year > 5)
                {
                    result.FailureCount++;
                    result.Errors.Add($"Line {lineIndex} ({regNo}): Invalid Year '{parts[4]}'");
                    continue;
                }
                var phone = parts[5].Trim().Trim('"');

                decimal? cgpa = null;
                if (parts.Length > 6 && decimal.TryParse(parts[6].Trim().Trim('"'), out decimal parsedCgpa))
                {
                    cgpa = parsedCgpa;
                }

                // Check existing in DB
                if (await _context.Students.AnyAsync(s => s.RegisterNumber == regNo || s.Email == email) ||
                    studentsToAdd.Any(s => s.RegisterNumber == regNo || s.Email == email))
                {
                    result.FailureCount++;
                    result.Errors.Add($"Line {lineIndex} ({regNo}): Duplicate Register Number or Email");
                    continue;
                }

                studentsToAdd.Add(new Student
                {
                    Name = name,
                    RegisterNumber = regNo,
                    Email = email,
                    Department = dept,
                    Year = year,
                    Phone = phone,
                    CGPA = cgpa,
                    Gender = Gender.Male,
                    Status = StudentStatus.Active,
                    CreatedAt = DateTime.UtcNow
                });

                result.SuccessCount++;
                result.SuccessRegisterNumbers.Add(regNo);
            }

            if (studentsToAdd.Any())
            {
                await _context.Students.AddRangeAsync(studentsToAdd);
                await _context.SaveChangesAsync();

                await LogAuditAsync("BULK_IMPORT", "Student", null, $"Imported {studentsToAdd.Count} students via CSV file.");
            }

            return ApiResponse<BulkImportResultDto>.Ok(result, $"Bulk import completed: {result.SuccessCount} added, {result.FailureCount} skipped.");
        }

        public async Task<ApiResponse<List<AuditLogDto>>> GetRecentAuditLogsAsync(int count = 20)
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .Select(l => new AuditLogDto
                {
                    Id = l.Id,
                    Action = l.Action,
                    EntityName = l.EntityName,
                    EntityId = l.EntityId,
                    Details = l.Details,
                    Timestamp = l.Timestamp
                })
                .ToListAsync();

            return ApiResponse<List<AuditLogDto>>.Ok(logs);
        }

        private async Task LogAuditAsync(string action, string entityName, int? entityId, string details)
        {
            try
            {
                await _context.AuditLogs.AddAsync(new AuditLog
                {
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Non-blocking audit failure
            }
        }

        private static string EscapeCsv(string value) => value.Replace("\"", "\"\"");

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
