using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudentManagementAPI.DTOs;

namespace StudentManagementAPI.Services
{
    public interface IAttendanceService
    {
        Task<ApiResponse<AttendanceRecordDto>> MarkAttendanceAsync(MarkAttendanceDto dto);
        Task<ApiResponse<StudentAttendanceSummaryDto>> GetStudentAttendanceSummaryAsync(int studentId);
        Task<ApiResponse<List<AttendanceRecordDto>>> GetAttendanceByDateAsync(DateTime date);
    }
}
