# 🎯 Xiphias Software Technologies – .NET Internship Interview Guide (தமிழ்)

இந்த விரிவான வழிகாட்டி உன்னுடைய **Enterprise Student Management System** project-ல் உள்ள அனைத்து Advanced அம்சங்களையும் interview-ல் தெளிவாக விளக்கி, interview-ல் கேட்கப்படும் கடினமான தொழில்நுட்பக் கேள்விகளுக்கு நம்பிக்கையுடன் பதிலளிக்க உதவும்.

---

## 📌 1. Interview-ல் Project-ஐ எப்படி அறிமுகப்படுத்துவது? (Project Pitch)

**Interviewer கேட்கலாம்:** *"Can you explain the architecture and key features of your .NET project?"*

**உன்னுடைய பதில் (English + Tamil கலந்து சொல்லலாம்):**
> "I developed an **Enterprise-grade Student Management System** using **ASP.NET Core 8 Web API** and **Entity Framework Core**.
>
> ஒரு சாதாரண Basic CRUD API போல இல்லாமல், உண்மையான IT நிறுவனங்களில் பயன்படுத்தப்படும் **Layered Architecture (Controller &rarr; Service &rarr; Repository)** மற்றும் Relational Database Design-ஐ அமைத்துள்ளேன்.
>
> **முக்கிய சிறப்பம்சங்கள் (Key Highlights):**
> 1. **Relational Database Design**: 
>    - `Student` &harr; `Course` இடையே **Many-to-Many Relationship** (via `Enrollment` table with Grade & Semester details).
>    - `Student` &harr; `AttendanceRecord` இடையே **One-to-Many Relationship**.
> 2. **Bulk CSV Data Import**: Excel/CSV sheet மூலம் பல மாணவர்களை ஒரே நேரத்தில் validate செய்து batch-ஆக register செய்யும் வசதி (`/api/students/import/csv`).
> 3. **Attendance & Eligibility Tracking**: மாணவர்களின் தினசரி வருகைப்பதிவு (Attendance) மற்றும் தேர்வு எழுத தகுதி உள்ளதா என்பதை 75% percentage அடிப்படையில் கணக்கிடும் அல்காரிதம்.
> 4. **Audit Trail Logging**: Database-ல் நடக்கும் ஒவ்வொரு மாற்றமும் (`CREATE`, `UPDATE`, `SOFT_DELETE`, `BULK_IMPORT`) AuditLogs அட்டவணையில் பதிவு செய்யப்படுகிறது.
> 5. **Soft Delete via Global Query Filters**: Records database-ல் இருந்து நிரந்தரமாக அழியாமல் `IsDeleted` flag மூலம் மறைக்கப்படும்.
> 6. **Zero-Setup Database**: எந்தவித configuration-ம் இல்லாமல் உடனே ரன் செய்ய **SQLite**-லும், ஒரே வரியில் **Microsoft SQL Server**-க்கும் மாறும் வகையில் அமைக்கப்பட்டுள்ளது."

---

## 🏗️ 2. Project Architecture & Database Relationships

```text
[ Browser / Swagger UI / Client ]
              │ (HTTP Requests)
              ▼
    [ API Controllers ]           <-- Students, Courses, Attendance, Analytics
              │
              ▼
     [ Business Services ]        <-- Business Logic, DTO Mapping, Bulk CSV Import
              │
              ▼
     [ Repositories / DbContext ] <-- EF Core LINQ, Transactions, Audit Logging
              │
              ▼
   [ SQLite / SQL Server ]        <-- Storage
```

### Database Entity Relationships (ER Design):
* **Student (1) &rarr; (M) Enrollment (M) &larr; (1) Course**:
  - மாணவர் பல பாடங்களை எடுக்கலாம், ஒரு பாடத்தில் பல மாணவர்கள் சேரலாம்.
  - இதற்காக `Enrollment` என்ற Join Entity உருவாக்கப்பட்டு, அதில் `Semester`, `Grade`, `EnrolledAt` போன்ற கூடுதல் தகவல்கள் சேமிக்கப்படுகின்றன.
* **Student (1) &rarr; (M) AttendanceRecord**:
  - ஒவ்வொரு மாணவருக்கும் தினமும் ஒரு வருகை பதிவு (`Present`, `Absent`, `Late`, `OnLeave`).
* **AuditLog**:
  - சிஸ்டத்தில் யார் என்ன மாற்றம் செய்தார்கள் என்பதற்கான காலவரிசைப் பதிவு.

---

## 💡 3. இந்த Project-ல் உள்ள Advanced தலைப்புகள் (Deep Dive)

### அ) Eager Loading vs Lazy Loading (EF Core `Include`)
**கேள்வி:** மாணவர் எடுக்கும் பாடங்களை Database-ல் இருந்து எப்படி எடுக்கிறீர்கள்?
**பதில்:**
- நாம் **Eager Loading** பயன்படுத்துகிறோம். EF Core-ல் `.Include(e => e.Course)` மற்றும் `.Include(e => e.Student)` என்று கொடுக்கும் போது, Single SQL `JOIN` Query மூலம் தொடர்புடைய டேட்டாவை உடனே கொண்டுவந்துவிடும்.
- இது `N+1 Query Problem`-ஐ தடுக்கிறது.

### ஆ) Bulk CSV Import handling (`IFormFile`)
**கேள்வி:** CSV Upload-ஐ API-ல் எப்படி செய்தீர்கள்?
**பதில்:**
- `IFormFile` மூலம் வரும் file stream-ஐ `StreamReader` கொண்டு வரிசையாகப் படித்து validate செய்கிறோம்.
- Register Number அல்லது Email ஏற்கனவே உள்ளதா என database-ல் சரிபார்த்து, தவறான வரிகளை (Invalid Rows) தனி லிஸ்ட்டாகவும், சரியான மாணவர்களை `AddRangeAsync()` மூலம் batch-ஆக insert செய்கிறோம்.
- Transaction முடிந்ததும் `AuditLog`-ல் பதிவு செய்யப்படுகிறது.

### இ) Soft Delete (Interviews-ல் அதிகம் பாராட்டப்படும் அம்சம்!)
- பல தொடக்க நிலை developer-கள் `_context.Students.Remove(student)` (Hard Delete) செய்வார்கள்.
- ஆனால் enterprise மென்பொருளில் data recovery மற்றும் audit-க்காக `IsDeleted = true` மட்டுமே செய்வார்கள்.
- EF Core `ApplicationDbContext`-ல் நாம்:
  ```csharp
  modelBuilder.Entity<Student>().HasQueryFilter(e => !e.IsDeleted);
  modelBuilder.Entity<Course>().HasQueryFilter(e => !e.IsDeleted);
  ```
  என்று அமைத்துள்ளதால், நாம் சாதாரணமாக `_context.Students.ToList()` என எழுதினாலும் Deleted ஆன records SQL query-லேயே filter ஆகிவிடும் (`WHERE IsDeleted = 0`).

### ஈ) Dependency Injection Lifetimes
1. **Transient (`AddTransient`)**: கேட்கும் போதெல்லாம் புதிய object.
2. **Scoped (`AddScoped`)**: ஒரு HTTP Request முழுவதும் ஒரே instance (நாம் Repository மற்றும் Service-க்கு இதைப் பயன்படுத்துகிறோம்).
3. **Singleton (`AddSingleton`)**: Application முழுவதும் ஒரே instance.

---

## ❓ 4. Top 10 Interview Questions & Model Answers

### Q1: What is the difference between ASP.NET Core and .NET Framework?
**பதில்:**
- ASP.NET Core என்பது Cross-Platform (Windows, Linux, macOS-ல் இயங்கும்), High Performance, Modular, மற்றும் Cloud-Ready architecture கொண்டது.
- பழைய .NET Framework Windows-ல் மட்டுமே இயங்கும்.

### Q2: Why use DTOs instead of passing Entities to Controllers?
**பதில்:**
1. **Security (Over-posting protection)**: Hacker ஒரு மாணவரின் தகவலை update செய்யும் போது `IsDeleted = false` அல்லது `Id = 99` என்று அனுப்பினால் entity-யை நேரடியாகப் பயன்படுத்தினால் hacker நினைத்தது நடந்துவிடும். DTO பயன்படுத்துவதால் அனுமதிக்கப்பட்ட fields மட்டுமே மாறும்.
2. **Decoupling**: Database schema மாறினாலும் Client API contract மாறாது.

### Q3: What is Global Exception Middleware?
**பதில்:**
- ஒவ்வொரு controller action-லும் `try-catch` எழுதுவதற்குப் பதிலாக, ASP.NET Core request pipeline-ல் ஒரு centralized middleware அமைத்து, ஏதேனும் unhandled exception வந்தால் அதை catch செய்து, standard JSON response ஆக (HTTP 500) அனுப்புகிறோம்.

### Q4: How is attendance percentage computed?
**பதில்:**
- ஒரு மாணவரின் மொத்த நாட்கள் (`TotalDays`), அதில் வந்த நாட்கள் (`PresentDays`) மற்றும் பாதி நாள் கணக்கீடான (`LateDays * 0.5`) கொண்டு:
  $$\text{Percentage} = \frac{\text{Present} + (\text{Late} \times 0.5)}{\text{Total}} \times 100$$
  என்ற formula மூலம் calculate செய்யப்படுகிறது. 75%-க்கு மேல் இருந்தால் Semester Exam-க்கு தகுதியானவர் என்று UI-ல் காட்டப்படுகிறது.

---

## 💻 5. Step-by-Step Interview Demo Walkthrough

1. Terminal-ல்:
   ```powershell
   cd C:\Users\ELCOT\Desktop\StudentManagementSystem\StudentManagementAPI
   dotnet run
   ```
2. Browser-ல் [http://localhost:5000](http://localhost:5000) திறந்து:
   - **Students Tab**: Filter, Pagination, CSV Export மற்றும் "Bulk Import CSV" பட்டனை demo செய்து காட்டலாம்.
   - **Courses Tab**: புதிய பாடம் உருவாக்குவது மற்றும் மாணவரை அதில் enroll செய்வது.
   - **Attendance Tracker Tab**: தேதியைத் தேர்ந்தெடுத்து வருகைப்பதிவு செய்வது மற்றும் மாணவரின் வருகை சதவீதத்தை (75% Exam Eligibility) பார்ப்பது.
   - **Audit Logs Tab**: இதுவரை நடந்த அனைத்து நடவடிக்கைகளும் பதிவாகியிருப்பதை நேர்காணல் செய்பவருக்குக் காட்டி வியக்க வைக்கலாம்!
