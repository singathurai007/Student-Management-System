using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentManagementAPI.Models;

namespace StudentManagementAPI.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // Check if Students exist
            if (await context.Students.IgnoreQueryFilters().AnyAsync())
            {
                return; // DB already seeded
            }

            // 1. Seed Students
            var students = new List<Student>
            {
                new Student
                {
                    Name = "Aravindhan Sundaram",
                    RegisterNumber = "REG2024CS01",
                    Email = "aravindhan.s@college.edu",
                    Department = "Computer Science and Engineering",
                    Year = 3,
                    Phone = "+919876543210",
                    Gender = Gender.Male,
                    Status = StudentStatus.Active,
                    CGPA = 8.75m,
                    CreatedAt = DateTime.UtcNow.AddDays(-120)
                },
                new Student
                {
                    Name = "Priya Dharshini",
                    RegisterNumber = "REG2024IT02",
                    Email = "priya.d@college.edu",
                    Department = "Information Technology",
                    Year = 2,
                    Phone = "+919876543211",
                    Gender = Gender.Female,
                    Status = StudentStatus.Active,
                    CGPA = 9.10m,
                    CreatedAt = DateTime.UtcNow.AddDays(-90)
                },
                new Student
                {
                    Name = "Karthikeyan Murugan",
                    RegisterNumber = "REG2024EC03",
                    Email = "karthik.m@college.edu",
                    Department = "Electronics and Communication",
                    Year = 4,
                    Phone = "+919876543212",
                    Gender = Gender.Male,
                    Status = StudentStatus.Active,
                    CGPA = 8.20m,
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                },
                new Student
                {
                    Name = "Sneha Ramanathan",
                    RegisterNumber = "REG2024ME04",
                    Email = "sneha.r@college.edu",
                    Department = "Mechanical Engineering",
                    Year = 1,
                    Phone = "+919876543213",
                    Gender = Gender.Female,
                    Status = StudentStatus.Active,
                    CGPA = 7.95m,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Student
                {
                    Name = "Vigneshwaran K",
                    RegisterNumber = "REG2024CS05",
                    Email = "vignesh.k@college.edu",
                    Department = "Computer Science and Engineering",
                    Year = 3,
                    Phone = "+919876543214",
                    Gender = Gender.Male,
                    Status = StudentStatus.Active,
                    CGPA = 8.90m,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Student
                {
                    Name = "Divya Bharathi",
                    RegisterNumber = "REG2024AI06",
                    Email = "divya.b@college.edu",
                    Department = "Artificial Intelligence & Data Science",
                    Year = 2,
                    Phone = "+919876543215",
                    Gender = Gender.Female,
                    Status = StudentStatus.Active,
                    CGPA = 9.40m,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            await context.Students.AddRangeAsync(students);
            await context.SaveChangesAsync();

            // 2. Seed Courses
            var courses = new List<Course>
            {
                new Course
                {
                    CourseCode = "CS101",
                    Title = "Data Structures & Algorithms",
                    Department = "Computer Science and Engineering",
                    Credits = 4,
                    Description = "Foundational concepts of arrays, stacks, queues, trees, graphs, and algorithmic complexity."
                },
                new Course
                {
                    CourseCode = "CS201",
                    Title = "Database Management Systems",
                    Department = "Computer Science and Engineering",
                    Credits = 4,
                    Description = "Relational models, SQL, normalization, concurrency control, and transactions."
                },
                new Course
                {
                    CourseCode = "IT102",
                    Title = "Full-Stack Web Development",
                    Department = "Information Technology",
                    Credits = 3,
                    Description = "Modern RESTful APIs, ASP.NET Core, JavaScript frontends, and cloud hosting."
                },
                new Course
                {
                    CourseCode = "AI301",
                    Title = "Artificial Intelligence & Machine Learning",
                    Department = "Artificial Intelligence & Data Science",
                    Credits = 4,
                    Description = "Supervised learning, deep learning, neural networks, and model evaluation."
                },
                new Course
                {
                    CourseCode = "EC204",
                    Title = "Embedded Systems & IoT",
                    Department = "Electronics and Communication",
                    Credits = 3,
                    Description = "Microcontroller programming, sensors, actuators, and wireless protocols."
                }
            };

            await context.Courses.AddRangeAsync(courses);
            await context.SaveChangesAsync();

            // 3. Seed Enrollments
            var enrollments = new List<Enrollment>
            {
                new Enrollment { StudentId = students[0].Id, CourseId = courses[0].Id, Semester = "Semester 5", Grade = "A+" },
                new Enrollment { StudentId = students[0].Id, CourseId = courses[1].Id, Semester = "Semester 5", Grade = "A" },
                new Enrollment { StudentId = students[1].Id, CourseId = courses[2].Id, Semester = "Semester 3", Grade = "A+" },
                new Enrollment { StudentId = students[2].Id, CourseId = courses[4].Id, Semester = "Semester 7", Grade = "B+" },
                new Enrollment { StudentId = students[4].Id, CourseId = courses[0].Id, Semester = "Semester 5", Grade = "A" },
                new Enrollment { StudentId = students[5].Id, CourseId = courses[3].Id, Semester = "Semester 3", Grade = "A+" }
            };

            await context.Enrollments.AddRangeAsync(enrollments);

            // 4. Seed Attendance Records for past 5 days
            var attendanceRecords = new List<AttendanceRecord>();
            for (int i = 4; i >= 0; i--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                // Skip weekends
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;

                foreach (var st in students)
                {
                    attendanceRecords.Add(new AttendanceRecord
                    {
                        StudentId = st.Id,
                        Date = date,
                        Status = (st.Id == students[3].Id && i == 1) ? AttendanceStatus.Absent : AttendanceStatus.Present,
                        Remarks = (st.Id == students[3].Id && i == 1) ? "Sick Leave" : "Regular Attendance"
                    });
                }
            }

            await context.AttendanceRecords.AddRangeAsync(attendanceRecords);

            // 5. Seed Initial Audit Log
            await context.AuditLogs.AddAsync(new AuditLog
            {
                Action = "SYSTEM_INITIALIZE",
                EntityName = "Database",
                Details = "Database initialized with initial students, courses, and attendance records.",
                Timestamp = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }
}
