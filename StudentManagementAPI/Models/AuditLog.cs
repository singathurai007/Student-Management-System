using System;
using System.ComponentModel.DataAnnotations;

namespace StudentManagementAPI.Models
{
    /// <summary>
    /// Audit log entity tracking system operations
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty; // "CREATE", "UPDATE", "DELETE", "IMPORT"

        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        [MaxLength(500)]
        public string Details { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
