using System.Collections.Generic;

namespace StudentManagementAPI.DTOs
{
    /// <summary>
    /// Summary analytics and distribution data for dashboard
    /// </summary>
    public class DashboardStatsDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int TotalDepartments { get; set; }
        public double AverageCGPA { get; set; }
        public Dictionary<string, int> DepartmentDistribution { get; set; } = new();
        public Dictionary<string, int> YearDistribution { get; set; } = new();
        public List<StudentResponseDto> RecentStudents { get; set; } = new();
    }
}

