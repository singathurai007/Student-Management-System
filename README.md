# 🎓 Enterprise Student Management System (.NET 8 Web API)

A production-ready, clean-architecture **Student Management System RESTful API** built using **ASP.NET Core 8**, **Entity Framework Core**, **Repository & Service Pattern**, and **C# 12**.

Designed specifically for **.NET Software Developer / Intern Interviews** (e.g., Xiphias Software Technologies) to demonstrate professional software engineering standards far beyond standard tutorial CRUD projects.

---

## 🌟 Key Architectural Highlights & Features

1. **Clean Layered Architecture**:
   - `Controllers` &rarr; `Services` (Business Logic & Validation) &rarr; `Repositories` (Data Access) &rarr; `Entity Framework Core`
2. **Relational Database Design (1-to-Many & Many-to-Many)**:
   - `Student` &harr; `Enrollment` &harr; `Course` (Many-to-Many with payload: `Semester`, `Grade`, `EnrolledAt`).
   - `Student` &harr; `AttendanceRecord` (1-to-Many with `AttendanceStatus`: Present, Absent, Late, OnLeave).
3. **Bulk Data Import via CSV**:
   - `/api/students/import/csv` allows administrators to upload CSV sheets of students with row-by-row validation, duplicate elimination, and summary reports.
4. **Attendance Tracking & Exam Eligibility Calculator**:
   - Daily attendance marking and instant percentage calculation (`AttendancePercentage >= 75%` eligibility indicator).
5. **System Audit Activity Logs**:
   - Every mutation (`CREATE`, `UPDATE`, `SOFT_DELETE`, `BULK_IMPORT`) is recorded in the `AuditLogs` table for regulatory compliance and traceability.
6. **Soft Delete & Audit Tracking**:
   - Uses `CreatedAt`, `UpdatedAt`, and `IsDeleted` on `BaseEntity`.
   - Utilizes EF Core **Global Query Filters** (`HasQueryFilter(e => !e.IsDeleted)`).
7. **Dual Database Provider Support**:
   - **SQLite** enabled out of the box (zero configuration, no local database server setup required).
   - **SQL Server / LocalDB** ready (switchable with a single line in `appsettings.json`).
8. **Data Transfer Objects (DTOs) & Model Validation**:
   - Prevents over-posting vulnerabilities.
   - Comprehensive validation attributes with regex for phone numbers and registration codes.
9. **Advanced Querying**:
   - Pagination (`pageNumber`, `pageSize`, metadata envelope).
   - Dynamic search across multiple fields (Name, Register Number, Email, Department).
   - Filtering by Department and Year.
   - Dynamic Sorting (`SortBy=Name|RegNo|CGPA|CreatedAt`).
10. **Global Exception Handling Middleware**:
    - Catches unhandled server errors and returns RFC-compliant standardized JSON responses without crashing or leaking stack traces.
11. **Interactive Single Page Dashboard UI**:
    - Embedded directly in `wwwroot`, powered by Bootstrap 5, Chart.js, SweetAlert2, and FontAwesome with dedicated tabs for Students, Courses, Attendance, and Audit Logs.
12. **Swagger / OpenAPI Documentation**:
    - Complete API schema definitions with XML documentation comments at `/swagger`.

---

## 📁 Project Structure

```text
StudentManagementSystem/
├── README.md
├── INTERVIEW_GUIDE_TAMIL.md             <-- Complete Tamil Interview Prep Guide
└── StudentManagementAPI/
    ├── Controllers/
    │   ├── StudentsController.cs        <-- CRUD, Search, Filter, Export, CSV Import, Audit
    │   ├── CoursesController.cs         <-- Curriculum, Course CRUD, Student Enrollments
    │   ├── AttendanceController.cs      <-- Daily Attendance, Student % Summary
    │   └── AnalyticsController.cs       <-- Dashboard metrics & department chart data
    ├── Data/
    │   ├── ApplicationDbContext.cs      <-- EF Core DbContext, Relationships & Global Filters
    │   └── DbInitializer.cs             <-- Auto-migration & Rich Seed Data
    ├── DTOs/
    │   ├── ApiResponse.cs               <-- Unified API response wrapper
    │   ├── CreateStudentDto.cs          <-- Validated creation payload
    │   ├── UpdateStudentDto.cs          <-- Validated update payload
    │   ├── StudentResponseDto.cs        <-- Public output view model
    │   ├── PaginationParams.cs          <-- Pagination & search query parameters
    │   ├── CourseDtos.cs                <-- Course creation and response models
    │   ├── EnrollmentDtos.cs            <-- Student course enrollment models
    │   ├── AttendanceDtos.cs            <-- Daily marking and percentage summaries
    │   ├── BulkImportResultDto.cs       <-- CSV import statistics
    │   ├── AuditLogDto.cs               <-- System audit trail view model
    │   └── DashboardStatsDto.cs         <-- Analytics payload
    ├── Middlewares/
    │   └── GlobalExceptionMiddleware.cs <-- Centralized error handler
    ├── Models/
    │   ├── BaseEntity.cs                <-- Audit & soft-delete base
    │   ├── Student.cs                   <-- Domain Entity (with Navigation properties)
    │   ├── Course.cs                    <-- Academic Course entity
    │   ├── Enrollment.cs                <-- Course-Student join entity
    │   ├── AttendanceRecord.cs          <-- Attendance entry entity
    │   └── AuditLog.cs                  <-- Operational audit log
    ├── Repositories/
    │   ├── IStudentRepository.cs        <-- Repository interface
    │   └── StudentRepository.cs         <-- EF Core data access implementation
    ├── Services/
    │   ├── IStudentService.cs           <-- Student business service interface
    │   ├── StudentService.cs            <-- Business rules, bulk CSV & audit mapping
    │   ├── ICourseService.cs            <-- Course service interface
    │   ├── CourseService.cs             <-- Course & enrollment business logic
    │   ├── IAttendanceService.cs        <-- Attendance service interface
    │   └── AttendanceService.cs         <-- Attendance recording & % calculation
    ├── wwwroot/                         <-- Interactive Modern Frontend UI
    │   ├── index.html                   <-- Tabbed dashboard interface
    │   ├── css/style.css
    │   └── js/app.js                    <-- Fetch API, Chart.js & Modals
    ├── appsettings.json                 <-- Configuration (DB provider switch)
    ├── Program.cs                       <-- Dependency Injection & Pipeline setup
    └── StudentManagementAPI.csproj      <-- Dependencies & Build config
```

---

## 🚀 Quick Start Guide

### 1. Prerequisites
- **.NET 8 SDK** (Download from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0))

### 2. Run the Application
Open PowerShell or Command Prompt:

```powershell
cd C:\Users\ELCOT\Desktop\StudentManagementSystem\StudentManagementAPI
dotnet run
```

### 3. Open in Browser
- **Modern Dashboard UI**: [http://localhost:5000](http://localhost:5000) or [https://localhost:5001](https://localhost:5001)
- **Swagger API Explorer**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## 🔌 Switching to Microsoft SQL Server

By default, the project runs on **SQLite** for instant zero-configuration execution.

To switch to **Microsoft SQL Server**:
1. Open `appsettings.json`.
2. Change `"DatabaseProvider": "Sqlite"` to `"DatabaseProvider": "SqlServer"`.
3. Update the `"SqlServerConnection"` string if your SQL Server instance name differs.
4. Restart the app (`dotnet run`).

---

## 📡 API Endpoints Overview

### 👨‍🎓 Students API (`/api/students`)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/students` | Get paginated students with dynamic filter, search, sort |
| `GET` | `/api/students/{id}` | Get student by ID |
| `GET` | `/api/students/regno/{regNo}` | Get student by registration number |
| `POST` | `/api/students` | Create student record |
| `PUT` | `/api/students/{id}` | Update student record |
| `DELETE` | `/api/students/{id}` | Soft delete student record |
| `GET` | `/api/students/export/csv` | Download active students CSV file |
| `POST` | `/api/students/import/csv` | Bulk import students from CSV file |
| `GET` | `/api/students/audit-logs` | Retrieve system audit logs |

### 📚 Courses & Enrollments API (`/api/courses`)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/courses` | List all academic courses with enrolled student count |
| `GET` | `/api/courses/{id}` | Get course details |
| `POST` | `/api/courses` | Create new course in curriculum |
| `DELETE` | `/api/courses/{id}` | Soft delete course |
| `POST` | `/api/courses/enroll` | Enroll a student in a course |
| `GET` | `/api/courses/{id}/enrollments` | View all students enrolled in a course |
| `GET` | `/api/courses/student/{studentId}` | View all courses a student is taking |

### 📅 Attendance API (`/api/attendance`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/attendance/mark` | Record/update student daily attendance |
| `GET` | `/api/attendance/student/{studentId}` | Student attendance summary & percentage |
| `GET` | `/api/attendance/date` | Daily attendance sheet for selected date |

### 📊 Analytics API (`/api/analytics`)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/analytics/dashboard` | Aggregated metrics & department breakdown |

---

## 📖 Tamil Interview Guide
Refer to **`INTERVIEW_GUIDE_TAMIL.md`** located in the root folder for step-by-step interview explanations, architecture justification, and common .NET Core interview questions.
