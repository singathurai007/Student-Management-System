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
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;

        public AttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<AttendanceRecordDto>> MarkAttendanceAsync(MarkAttendanceDto dto)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == dto.StudentId);
            if (student == null)
            {
                return ApiResponse<AttendanceRecordDto>.Fail($"Student with ID {dto.StudentId} was not found");
            }

            var recordDate = dto.Date.Date;
            var existingRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.StudentId == dto.StudentId && a.Date.Date == recordDate);

            if (existingRecord != null)
            {
                // Update existing record
                existingRecord.Status = dto.Status;
                existingRecord.Remarks = dto.Remarks;
                existingRecord.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<AttendanceRecordDto>.Ok(new AttendanceRecordDto
                {
                    Id = existingRecord.Id,
                    StudentId = student.Id,
                    StudentName = student.Name,
                    RegisterNumber = student.RegisterNumber,
                    Date = existingRecord.Date,
                    Status = existingRecord.Status.ToString(),
                    Remarks = existingRecord.Remarks
                }, "Attendance updated successfully");
            }

            var newRecord = new AttendanceRecord
            {
                StudentId = dto.StudentId,
                Date = recordDate,
                Status = dto.Status,
                Remarks = dto.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AttendanceRecords.AddAsync(newRecord);
            await _context.SaveChangesAsync();

            return ApiResponse<AttendanceRecordDto>.Ok(new AttendanceRecordDto
            {
                Id = newRecord.Id,
                StudentId = student.Id,
                StudentName = student.Name,
                RegisterNumber = student.RegisterNumber,
                Date = newRecord.Date,
                Status = newRecord.Status.ToString(),
                Remarks = newRecord.Remarks
            }, "Attendance recorded successfully");
        }

        public async Task<ApiResponse<StudentAttendanceSummaryDto>> GetStudentAttendanceSummaryAsync(int studentId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null)
            {
                return ApiResponse<StudentAttendanceSummaryDto>.Fail($"Student with ID {studentId} was not found");
            }

            var records = await _context.AttendanceRecords
                .AsNoTracking()
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            var totalDays = records.Count;
            var presentDays = records.Count(r => r.Status == AttendanceStatus.Present);
            var absentDays = records.Count(r => r.Status == AttendanceStatus.Absent);
            var lateDays = records.Count(r => r.Status == AttendanceStatus.Late);

            double percentage = totalDays > 0
                ? Math.Round(((double)(presentDays + (lateDays * 0.5)) / totalDays) * 100, 1)
                : 100.0;

            var recentRecords = records.Take(10).Select(r => new AttendanceRecordDto
            {
                Id = r.Id,
                StudentId = student.Id,
                StudentName = student.Name,
                RegisterNumber = student.RegisterNumber,
                Date = r.Date,
                Status = r.Status.ToString(),
                Remarks = r.Remarks
            }).ToList();

            var summary = new StudentAttendanceSummaryDto
            {
                StudentId = student.Id,
                StudentName = student.Name,
                RegisterNumber = student.RegisterNumber,
                TotalDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                LateDays = lateDays,
                AttendancePercentage = percentage,
                RecentRecords = recentRecords
            };

            return ApiResponse<StudentAttendanceSummaryDto>.Ok(summary);
        }

        public async Task<ApiResponse<List<AttendanceRecordDto>>> GetAttendanceByDateAsync(DateTime date)
        {
            var targetDate = date.Date;

            var records = await _context.AttendanceRecords
                .AsNoTracking()
                .Include(a => a.Student)
                .Where(a => a.Date.Date == targetDate)
                .Select(a => new AttendanceRecordDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student.Name,
                    RegisterNumber = a.Student.RegisterNumber,
                    Date = a.Date,
                    Status = a.Status.ToString(),
                    Remarks = a.Remarks
                })
                .ToListAsync();

            return ApiResponse<List<AttendanceRecordDto>>.Ok(records);
        }
    }
}
