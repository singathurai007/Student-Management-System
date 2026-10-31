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
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>
        /// Retrieves all available academic courses
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CourseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _courseService.GetAllCoursesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves details of a specific course by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _courseService.GetCourseByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new course in the curriculum
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<CourseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _courseService.CreateCourseAsync(dto);
            if (!result.Success) return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        /// <summary>
        /// Deletes a course (soft delete)
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _courseService.DeleteCourseAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Enrolls a student in a course
        /// </summary>
        [HttpPost("enroll")]
        [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Enroll([FromBody] CreateEnrollmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _courseService.EnrollStudentAsync(dto);
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all enrolled students in a specific course
        /// </summary>
        [HttpGet("{id:int}/enrollments")]
        [ProducesResponseType(typeof(ApiResponse<List<EnrollmentDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCourseEnrollments(int id)
        {
            var result = await _courseService.GetCourseEnrollmentsAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all courses a specific student is enrolled in
        /// </summary>
        [HttpGet("student/{studentId:int}")]
        [ProducesResponseType(typeof(ApiResponse<List<EnrollmentDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStudentEnrollments(int studentId)
        {
            var result = await _courseService.GetStudentEnrollmentsAsync(studentId);
            return Ok(result);
        }
    }
}
