using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StudentManagementAPI.Models;

namespace StudentManagementAPI.DTOs
{
    public class AttendanceRecordDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RegisterNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class MarkAttendanceDto
    {
        [Required]
        public int StudentId { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        [Required]
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [StringLength(200)]
        public string? Remarks { get; set; }
    }

    public class StudentAttendanceSummaryDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RegisterNumber { get; set; } = string.Empty;
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public double AttendancePercentage { get; set; }
        public List<AttendanceRecordDto> RecentRecords { get; set; } = new();
    }
}
