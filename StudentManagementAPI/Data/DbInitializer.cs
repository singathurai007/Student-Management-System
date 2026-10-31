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
            // Ensures database exists with schema
            await context.Database.EnsureCreatedAsync();

            // Look for any existing students
            if (await context.Students.IgnoreQueryFilters().AnyAsync())
            {
                return; // DB has already been seeded
            }

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
        }
    }
}

