# 🎯 Xiphias Software Technologies – .NET Internship Interview Guide (தமிழ்)

இந்த Guide உன்னுடைய **Student Management System** project-ஐ interview-ல் எப்படி விளக்குவது, என்னென்ன கேள்விகள் கேட்பார்கள், அதற்கு எப்படி சிறப்பான பதிலளிப்பது என்பதை முழுமையாகக் கற்பிக்கும்.

---

## 📌 1. Interview-ல் Project-ஐ எப்படி அறிமுகப்படுத்துவது? (Project Pitch)

**Interviewer கேட்கலாம்:** *"Can you explain about the project you built in .NET Core?"*

**உன்னுடைய பதில் (English + Tamil கலந்து சொல்லலாம்):**
> "I developed an **Enterprise-grade Student Management System** using **ASP.NET Core 8 Web API** and **Entity Framework Core**.
>
> சாதாரணமான Basic CRUD Project போல இல்லாமல், industry standard **Layered Architecture (Repository & Service Pattern)**-ஐ பயன்படுத்தியுள்ளேன்.
>
> **முக்கிய சிறப்பம்சங்கள் (Key Highlights):**
> 1. **Clean Separation of Concerns**: Controller &rarr; Service &rarr; Repository &rarr; Database.
> 2. **Security & DTOs**: Entity Model-ஐ நேரடியாக expose செய்யாமல் DTOs (Data Transfer Objects) மற்றும் Data Annotation Validations பயன்படுத்தியுள்ளேன் (Over-posting protection).
> 3. **Soft Delete**: மாணவர்களின் records-ஐ permanent-ஆக அழிக்காமல் `IsDeleted` flag மற்றும் EF Core **Global Query Filter** மூலமாக handle செய்துள்ளேன்.
> 4. **Search, Pagination & Filtering**: அதிக தரவுகளை efficiently load செய்ய Server-side Pagination, Dynamic Sorting மற்றும் Multi-field Search வசதி உண்டு.
> 5. **Global Exception Handling Middleware**: Server crash ஆகாமல் RFC standard JSON Error Response தரும் Middleware எழுதியுள்ளேன்.
> 6. **Interactive Dashboard**: API உடன் Bootstrap 5 & Chart.js கொண்ட Responsive Dashboard UI இணைக்கப்பட்டுள்ளது."

---

## 🏗️ 2. Project Architecture விளக்கம்

```text
[ Browser / Swagger UI / Client ]
              │ (HTTP Requests)
              ▼
    [ StudentsController ]         <-- API Endpoint, HTTP Status Codes
              │
              ▼
      [ StudentService ]           <-- Business Logic, Validations, DTO Mapping
              │
              ▼
    [ StudentRepository ]          <-- LINQ Queries, EF Core Operations
              │
              ▼
   [ ApplicationDbContext ]        <-- Entity Framework Core ORM
              │
              ▼
   [ SQLite / SQL Server ]         <-- Database Storage
```

### ஏன் இந்த Layered Architecture-ஐ தேர்வு செய்தாய்?
1. **Maintainability**: Controller-ல் database code எழுதாமல் business logic தனியாகவும், data access தனியாகவும் பிரிக்கப்பட்டுள்ளது.
2. **Reusability**: `IStudentService` மற்றும் `IStudentRepository` interface மூலம் எழுதப்பட்டதால் Unit Testing (Moq) செய்வது மிகவும் எளிது.
3. **Decoupling**: எதிர்காலத்தில் Database-ஐ SQLite-லிருந்து SQL Server அல்லது PostgreSQL-க்கு மாற்றினாலும் Controller அல்லது Service-ஐ மாற்ற வேண்டிய அவசியமில்லை.

---

## 💡 3. இந்த Project-ல் உள்ள Advanced Features

### அ) Soft Delete (Interviews-ல் அதிகம் விரும்பப்படும் தலைப்பு!)
- பலரும் `DELETE` method-ல் `context.Students.Remove(student)` என்று Database-ல் இருந்தே அழித்துவிடுவார்கள் (Hard Delete).
- ஆனால் உண்மையான நிறுவனங்களில் records-ஐ நிரந்தரமாக அழிக்க மாட்டார்கள்.
- நாம் `BaseEntity`-ல் `IsDeleted = true` என்று மாற்றிவிடுகிறோம்.
- EF Core `OnModelCreating`-ல்:
  ```csharp
  entity.HasQueryFilter(e => !e.IsDeleted);
  ```
  என்று எழுதியுள்ளோம். இதனால் நாம் எங்கு `_context.Students` என்று call செய்தாலும், deleted records தானாகவே filter ஆகிவிடும்!

### ஆ) Data Transfer Objects (DTOs) & Data Annotations
- **Entity**: Database table-ன் நேரடி பிரதிபலிப்பு.
- **DTO**: Client-க்கு அனுப்ப வேண்டிய மற்றும் client-டமிருந்து பெற வேண்டிய தேர்ந்தெடுக்கப்பட்ட தரவுகள் மட்டும்.
- **ஏன் DTO பயன்படுத்த வேண்டும்?**
  - **Over-posting Attack தடுப்பு**: தேவையில்லாத fields (உதாரணமாக `IsDeleted`, `CreatedAt`, Internal IDs) hacker மாற்றாமல் தடுக்க.
  - Validation: Email Format, Phone Number Regex, CGPA Range (0-10) போன்றவற்றை API மட்டத்திலேயே சரிபார்க்கலாம்.

### இ) Global Exception Middleware
- Controller-ல் ஒவ்வொரு முறையும் `try-catch` எழுத வேண்டிய அவசியமில்லை.
- ஏதேனும் unhandled error வந்தால் `GlobalExceptionMiddleware` அதை catch செய்து, standard JSON response:
  ```json
  {
    "success": false,
    "message": "An unexpected server error occurred...",
    "errors": ["Details"],
    "timestamp": "2026-10-31T10:00:00Z"
  }
  ```
  ஆக மாற்றி 500 Internal Server Error தரும்.

### ஈ) Dual Database Support (Zero Configuration)
- `appsettings.json`-ல்:
  `"DatabaseProvider": "Sqlite"` &rarr; SQLite-ல் உடனே இயங்கும் (SQL Server தேவையில்லை).
  `"DatabaseProvider": "SqlServer"` &rarr; Microsoft SQL Server-க்கு எளிதாக மாற்றிவிடலாம்.

---

## ❓ 4. Top 10 Interview Questions & Model Answers

### Q1: What is Dependency Injection (DI) in ASP.NET Core?
**பதில்:**
- Dependency Injection என்பது ஒரு Design Pattern. ஒரு class-க்குத் தேவையான object-ஐ அதுவே `new` போட்டு உருவாக்காமல், external container (Inversion of Control - IoC) மூலமாக pass செய்வதாகும்.
- இதில் 3 Service Lifetimes உள்ளன:
  1. **Transient**: ஒவ்வொரு முறையும் புதிய instance உருவாக்கப்படும் (`AddTransient`).
  2. **Scoped**: ஒரு HTTP Request முடிவடையும் வரை ஒரே instance பகிரப்படும் (`AddScoped`). நாம் Repository மற்றும் Service-க்கு `AddScoped` பயன்படுத்தியுள்ளோம்.
  3. **Singleton**: Application இயங்கும் வரை ஒரே ஒரு instance மட்டுமே இருக்கும் (`AddSingleton`).

### Q2: What is Entity Framework Core (EF Core)?
**பதில்:**
- EF Core என்பது .NET-க்கான Object-Relational Mapper (ORM).
- SQL queries (`SELECT * FROM Students`) எழுதாமல், C# Objects மற்றும் LINQ (`_context.Students.Where(s => s.Year == 2)`) மூலமாக database உடன் பேச உதவுகிறது.
- நாம் **Code-First Approach** பயன்படுத்தியுள்ளோம். அதாவது C# Classes எழுதி, அதிலிருந்து Database Tables உருவாக்கப்படுகின்றன.

### Q3: What is the difference between `IEnumerable` and `IQueryable`?
**பதில்:**
- `IEnumerable`: In-memory data collections-க்கு பயன்படும். Database-ல் இருந்து எல்லா records-ஐயும் memory-க்கு கொண்டு வந்த பிறகே filter செய்யும் (Client-side evaluation).
- `IQueryable`: Database queries-க்கு பயன்படும். LINQ expression tree ஆக மாறி SQL Query-ஆக Server-ல் சென்று execute ஆகும் (Server-side evaluation). அதனால் performance மிக அதிகமாக இருக்கும்.

### Q4: Why use `async` and `await` in API Controller/Service?
**பதில்:**
- Database call அல்லது Network call நடக்கும் போது Thread Block ஆகாமல் இருக்க Asynchronous programming பயன்படுகிறது.
- `Task<IActionResult>`, `SaveChangesAsync()` பயன்படுத்துவதால் குறைவான server resources-ல் ஆயிரக்கணக்கான பயனர்களின் requests-ஐ handle செய்ய முடிகிறது.

### Q5: What are the common HTTP Methods and Status Codes used?
**பதில்:**
- `GET /api/students` &rarr; 200 OK
- `POST /api/students` &rarr; 201 Created (with `CreatedAtAction`)
- Invalid Input &rarr; 400 Bad Request
- Record Not Found &rarr; 404 Not Found
- Duplicate Reg Number &rarr; 400 Bad Request / 409 Conflict
- Server Error &rarr; 500 Internal Server Error

---

## 💻 5. Step-by-Step Demo Walkthrough (Interview நேரத்திற்கு)

1. Command Prompt அல்லது Terminal-ல்:
   ```powershell
   cd C:\Users\ELCOT\Desktop\StudentManagementSystem\StudentManagementAPI
   dotnet run
   ```
2. Browser-ல் [http://localhost:5000](http://localhost:5000)-ஐ திறக்கவும்:
   - Dashboard-ல் Total Students, Department Distribution Chart காண்பிக்கப்படும்.
3. [http://localhost:5000/swagger](http://localhost:5000/swagger)-ஐ திறந்து:
   - `GET /api/students` execute செய்து pagination மற்றும் search parameter-ஐ காண்பிக்கவும்.
   - `POST /api/students` மூலமாக ஒரு மாணவரை பதிவு செய்து காண்பிக்கவும்.
   - `DELETE /api/students/{id}` செய்துவிட்டு, Database-ல் record அழியாமல் `IsDeleted = true` ஆவதை விளக்கலாம்.

