using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentManagementAPI.DTOs;
using StudentManagementAPI.Services;

namespace StudentManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// Marks or updates attendance for a student on a specific date
        /// </summary>
        [HttpPost("mark")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceRecordDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AttendanceRecordDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _attendanceService.MarkAttendanceAsync(dto);
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves complete attendance percentage and recent records for a specific student
        /// </summary>
        [HttpGet("student/{studentId:int}")]
        [ProducesResponseType(typeof(ApiResponse<StudentAttendanceSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<StudentAttendanceSummaryDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStudentAttendance(int studentId)
        {
            var result = await _attendanceService.GetStudentAttendanceSummaryAsync(studentId);
            if (!result.Success) return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all attendance records for a specific date (defaults to today)
        /// </summary>
        [HttpGet("date")]
        [ProducesResponseType(typeof(ApiResponse<List<AttendanceRecordDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAttendanceByDate([FromQuery] DateTime? date)
        {
            var targetDate = date ?? DateTime.UtcNow.Date;
            var result = await _attendanceService.GetAttendanceByDateAsync(targetDate);
            return Ok(result);
        }
    }
}
