using System.Collections.Generic;

namespace StudentManagementAPI.DTOs
{
    public class BulkImportResultDto
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> SuccessRegisterNumbers { get; set; } = new();
    }
}
