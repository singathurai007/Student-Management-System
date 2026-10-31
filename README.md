# 🎓 Enterprise Student Management System (.NET 8 Web API)

A production-ready, clean-architecture **Student Management System RESTful API** built using **ASP.NET Core 8**, **Entity Framework Core**, **Repository Pattern**, and **C# 12**.

Designed specifically for **.NET Software Developer / Intern Interviews** (e.g., Xiphias Software Technologies) to demonstrate professional software engineering standards beyond standard tutorial CRUD projects.

---

## 🌟 Key Architectural Highlights (What Makes This "Advanced")

1. **Clean Layered Architecture**:
   - `Controllers` &rarr; `Services` (Business Logic & Validation) &rarr; `Repositories` (Data Access) &rarr; `Entity Framework Core`
2. **Repository & Service Pattern with Dependency Injection**:
   - Loose coupling, testability, and separation of concerns.
3. **Dual Database Provider Support**:
   - **SQLite** enabled out of the box (zero configuration, no local database server setup required).
   - **SQL Server / LocalDB** ready (switchable with a single line in `appsettings.json`).
4. **Soft Delete & Audit Tracking**:
   - Uses `CreatedAt`, `UpdatedAt`, and `IsDeleted` on `BaseEntity`.
   - Utilizes EF Core **Global Query Filters** (`HasQueryFilter(e => !e.IsDeleted)`).
5. **Data Transfer Objects (DTOs) & Model Validation**:
   - Prevents over-posting vulnerabilities.
   - Comprehensive validation attributes with regex for phone numbers and registration codes.
6. **Advanced Querying**:
   - Pagination (`pageNumber`, `pageSize`, metadata envelope).
   - Dynamic search across multiple fields (Name, Register Number, Email, Department).
   - Filtering by Department and Year.
   - Dynamic Sorting (`SortBy=Name|RegNo|CGPA|CreatedAt`).
7. **Global Exception Handling Middleware**:
   - Catches unhandled server errors and returns RFC-compliant standardized JSON responses without crashing or leaking stack traces.
8. **Built-in Analytics & CSV Export**:
   - `/api/analytics/dashboard` provides department distribution and CGPA metrics.
   - `/api/students/export/csv` generates downloadable reports directly from stream.
9. **Interactive Single Page Dashboard UI**:
   - Embedded directly in `wwwroot`, powered by Bootstrap 5, Chart.js, SweetAlert2, and FontAwesome.
10. **Swagger / OpenAPI Documentation**:
    - Complete API schema definitions with XML documentation comments at `/swagger`.

---

## 📁 Project Structure

```text
StudentManagementSystem/
├── README.md
├── INTERVIEW_GUIDE_TAMIL.md             <-- Complete Tamil Interview Prep Guide
└── StudentManagementAPI/
    ├── Controllers/
    │   ├── StudentsController.cs        <-- CRUD, Search, Filter, Export
    │   └── AnalyticsController.cs       <-- Dashboard metrics & chart data
    ├── Data/
    │   ├── ApplicationDbContext.cs      <-- EF Core DbContext & Global Filters
    │   └── DbInitializer.cs             <-- Auto-migration & Sample Data Seeder
    ├── DTOs/
    │   ├── ApiResponse.cs               <-- Unified API response wrapper
    │   ├── CreateStudentDto.cs          <-- Validated creation payload
    │   ├── UpdateStudentDto.cs          <-- Validated update payload
    │   ├── StudentResponseDto.cs        <-- Public output view model
    │   ├── PaginationParams.cs          <-- Pagination & search query parameters
    │   └── DashboardStatsDto.cs         <-- Analytics payload
    ├── Middlewares/
    │   └── GlobalExceptionMiddleware.cs <-- Centralized error handler
    ├── Models/
    │   ├── BaseEntity.cs                <-- Audit & soft-delete base
    │   └── Student.cs                   <-- Domain Entity
    ├── Repositories/
    │   ├── IStudentRepository.cs        <-- Repository interface
    │   └── StudentRepository.cs         <-- EF Core data access implementation
    ├── Services/
    │   ├── IStudentService.cs           <-- Business service interface
    │   └── StudentService.cs            <-- Business rules & DTO mapping
    ├── wwwroot/                         <-- Interactive Modern Frontend UI
    │   ├── index.html                   <-- Single-page dashboard
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
Open PowerShell or Command Prompt, navigate to the project directory:

```powershell
cd C:\Users\ELCOT\Desktop\StudentManagementSystem\StudentManagementAPI
dotnet run
```

The application will start, automatically create the SQLite database `StudentManagement.db`, seed realistic student records, and launch the web server.

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

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/students` | Get paginated students (supports `searchTerm`, `department`, `year`, `sortBy`, `pageNumber`, `pageSize`) |
| `GET` | `/api/students/{id}` | Get single student by ID |
| `GET` | `/api/students/regno/{regNo}` | Get student by unique Registration Number |
| `POST` | `/api/students` | Create student (with DTO validation & duplicate check) |
| `PUT` | `/api/students/{id}` | Update student details |
| `DELETE` | `/api/students/{id}` | Soft delete student |
| `GET` | `/api/students/export/csv` | Export active students to CSV file |
| `GET` | `/api/analytics/dashboard` | Aggregated dashboard metrics & department chart data |

---

