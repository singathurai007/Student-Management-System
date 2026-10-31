using System;
using System.ComponentModel.DataAnnotations;

namespace StudentManagementAPI.Models
{
    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2,
        Late = 3,
        OnLeave = 4
    }

    /// <summary>
    /// Student daily attendance record
    /// </summary>
    public class AttendanceRecord : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public DateTime Date { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [MaxLength(200)]
        public string? Remarks { get; set; }
    }
}
