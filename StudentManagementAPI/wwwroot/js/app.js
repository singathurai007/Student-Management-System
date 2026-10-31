// Student Management System - Advanced Frontend App
const API_BASE = '/api';

let state = {
  pageNumber: 1,
  pageSize: 8,
  searchTerm: '',
  department: '',
  year: '',
  sortBy: 'Name',
  isDescending: false,
  totalCount: 0,
  totalPages: 1
};

let deptChart = null;
let addModal = null;
let editModal = null;
let viewModal = null;
let bulkImportModal = null;
let addCourseModal = null;
let enrollModal = null;
let markAttendanceModal = null;
let attendanceSummaryModal = null;

let allStudentsCache = [];
let allCoursesCache = [];

document.addEventListener('DOMContentLoaded', () => {
  // Initialize Bootstrap Modals
  addModal = new bootstrap.Modal(document.getElementById('addStudentModal'));
  editModal = new bootstrap.Modal(document.getElementById('editStudentModal'));
  viewModal = new bootstrap.Modal(document.getElementById('viewStudentModal'));
  bulkImportModal = new bootstrap.Modal(document.getElementById('bulkImportModal'));
  addCourseModal = new bootstrap.Modal(document.getElementById('addCourseModal'));
  enrollModal = new bootstrap.Modal(document.getElementById('enrollModal'));
  markAttendanceModal = new bootstrap.Modal(document.getElementById('markAttendanceModal'));
  attendanceSummaryModal = new bootstrap.Modal(document.getElementById('studentAttendanceSummaryModal'));

  // Default date inputs to today
  const todayStr = new Date().toISOString().split('T')[0];
  document.getElementById('attendanceDateInput').value = todayStr;
  document.getElementById('markDateInput').value = todayStr;

  initEventListeners();
  loadStudents();
  loadAnalytics();
});

function initEventListeners() {
  const searchInput = document.getElementById('searchInput');
  let searchTimeout = null;
  searchInput.addEventListener('input', (e) => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
      state.searchTerm = e.target.value;
      state.pageNumber = 1;
      loadStudents();
    }, 300);
  });

  document.getElementById('departmentFilter').addEventListener('change', (e) => {
    state.department = e.target.value;
    state.pageNumber = 1;
    loadStudents();
  });

  document.getElementById('yearFilter').addEventListener('change', (e) => {
    state.year = e.target.value;
    state.pageNumber = 1;
    loadStudents();
  });

  document.getElementById('sortBy').addEventListener('change', (e) => {
    state.sortBy = e.target.value;
    state.pageNumber = 1;
    loadStudents();
  });
}

// -------------------------------------------------------------
// 1. STUDENTS MANAGEMENT
// -------------------------------------------------------------
async function loadStudents() {
  const tbody = document.getElementById('studentTableBody');
  tbody.innerHTML = `
    <tr>
      <td colspan="9" class="text-center py-4 text-muted">
        <i class="fa-solid fa-spinner fa-spin me-2"></i> Loading students...
      </td>
    </tr>
  `;

  const queryParams = new URLSearchParams({
    pageNumber: state.pageNumber,
    pageSize: state.pageSize,
    searchTerm: state.searchTerm || '',
    department: state.department || '',
    year: state.year || '',
    sortBy: state.sortBy || 'Name',
    isDescending: state.isDescending
  });

  try {
    const res = await fetch(`${API_BASE}/students?${queryParams}`);
    const data = await res.json();

    if (!data.success) throw new Error(data.message || 'Failed to fetch students');

    const paged = data.data;
    state.totalCount = paged.totalCount;
    state.totalPages = paged.totalPages;

    renderTable(paged.items);
    renderPagination(paged);
    updateStudentSelectDropdowns();
  } catch (err) {
    tbody.innerHTML = `
      <tr>
        <td colspan="9" class="text-center py-4 text-danger">
          <i class="fa-solid fa-triangle-exclamation me-2"></i> Error: ${err.message}
        </td>
      </tr>
    `;
  }
}

function renderTable(students) {
  const tbody = document.getElementById('studentTableBody');
  if (!students || students.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="9" class="text-center py-5 text-muted">
          <i class="fa-solid fa-folder-open fs-2 d-block mb-2 text-secondary"></i>
          No student records found matching the criteria.
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = students.map(s => {
    const statusClass = `badge-${(s.status || 'Active').toLowerCase()}`;
    const cgpa = s.cgpa ? s.cgpa.toFixed(2) : 'N/A';
    const cgpaPillClass = s.cgpa >= 8.5 ? 'cgpa-high' : (s.cgpa >= 7.0 ? 'cgpa-mid' : 'cgpa-low');

    return `
      <tr>
        <td class="fw-semibold text-primary font-monospace">${escapeHtml(s.registerNumber)}</td>
        <td>
          <div class="fw-semibold">${escapeHtml(s.name)}</div>
          <small class="text-muted">${escapeHtml(s.gender || 'Not Specified')}</small>
        </td>
        <td><a href="mailto:${escapeHtml(s.email)}" class="text-decoration-none text-muted">${escapeHtml(s.email)}</a></td>
        <td><span class="text-truncate d-inline-block" style="max-width: 180px;">${escapeHtml(s.department)}</span></td>
        <td>Year ${s.year}</td>
        <td>${escapeHtml(s.phone)}</td>
        <td><span class="cgpa-pill ${cgpaPillClass}">${cgpa}</span></td>
        <td><span class="badge-status ${statusClass}">${escapeHtml(s.status)}</span></td>
        <td class="text-end text-nowrap">
          <button class="btn btn-outline-info action-btn me-1" title="View Profile & Courses" onclick="viewStudent(${s.id})">
            <i class="fa-solid fa-eye"></i>
          </button>
          <button class="btn btn-outline-success action-btn me-1" title="View Attendance %" onclick="viewStudentAttendanceSummary(${s.id})">
            <i class="fa-solid fa-calendar-check"></i>
          </button>
          <button class="btn btn-outline-warning action-btn me-1" title="Edit" onclick="openEditModal(${s.id})">
            <i class="fa-solid fa-pencil"></i>
          </button>
          <button class="btn btn-outline-danger action-btn" title="Delete" onclick="deleteStudent(${s.id}, '${escapeHtml(s.name)}')">
            <i class="fa-solid fa-trash-can"></i>
          </button>
        </td>
      </tr>
    `;
  }).join('');
}

function renderPagination(paged) {
  const info = document.getElementById('paginationInfo');
  const start = paged.totalCount === 0 ? 0 : (paged.pageNumber - 1) * paged.pageSize + 1;
  const end = Math.min(paged.pageNumber * paged.pageSize, paged.totalCount);
  info.innerText = `Showing ${start} to ${end} of ${paged.totalCount} students (Page ${paged.pageNumber} of ${paged.totalPages || 1})`;

  const controls = document.getElementById('paginationControls');
  controls.innerHTML = `
    <button class="btn btn-outline-secondary" ${!paged.hasPreviousPage ? 'disabled' : ''} onclick="changePage(${paged.pageNumber - 1})">
      <i class="fa-solid fa-chevron-left"></i> Previous
    </button>
    <button class="btn btn-outline-secondary active">${paged.pageNumber}</button>
    <button class="btn btn-outline-secondary" ${!paged.hasNextPage ? 'disabled' : ''} onclick="changePage(${paged.pageNumber + 1})">
      Next <i class="fa-solid fa-chevron-right"></i>
    </button>
  `;
}

function changePage(page) {
  if (page < 1 || page > state.totalPages) return;
  state.pageNumber = page;
  loadStudents();
}

async function handleCreateStudent(e) {
  e.preventDefault();
  const form = e.target;
  const formData = new FormData(form);

  const payload = {
    name: formData.get('name').trim(),
    registerNumber: formData.get('registerNumber').trim().toUpperCase(),
    email: formData.get('email').trim().toLowerCase(),
    phone: formData.get('phone').trim(),
    department: formData.get('department'),
    year: parseInt(formData.get('year')),
    cgpa: formData.get('cgpa') ? parseFloat(formData.get('cgpa')) : null,
    gender: parseInt(formData.get('gender')),
    status: parseInt(formData.get('status'))
  };

  try {
    const res = await fetch(`${API_BASE}/students`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await res.json();

    if (!res.ok || !data.success) {
      const errMsg = data.message || (data.errors ? Object.values(data.errors).flat().join('<br>') : 'Creation failed');
      Swal.fire({ icon: 'error', title: 'Error', html: errMsg });
      return;
    }

    addModal.hide();
    form.reset();
    Swal.fire({ icon: 'success', title: 'Created!', text: 'Student registered successfully', timer: 1800, showConfirmButton: false });
    loadStudents();
    loadAnalytics();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Network Error', text: err.message });
  }
}

async function openEditModal(id) {
  try {
    const res = await fetch(`${API_BASE}/students/${id}`);
    const data = await res.json();
    if (!data.success) return Swal.fire({ icon: 'error', title: 'Error', text: data.message });

    const s = data.data;
    document.getElementById('editStudentId').value = s.id;
    document.getElementById('editRegisterNumber').value = s.registerNumber;
    document.getElementById('editName').value = s.name;
    document.getElementById('editEmail').value = s.email;
    document.getElementById('editPhone').value = s.phone;
    document.getElementById('editDepartment').value = s.department;
    document.getElementById('editYear').value = s.year;
    document.getElementById('editCGPA').value = s.cgpa || '';

    const genderMap = { 'Male': 1, 'Female': 2, 'Other': 3 };
    const statusMap = { 'Active': 1, 'Inactive': 2, 'Suspended': 3, 'Graduated': 4 };
    document.getElementById('editGender').value = genderMap[s.gender] || 1;
    document.getElementById('editStatus').value = statusMap[s.status] || 1;

    editModal.show();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Error', text: err.message });
  }
}

async function handleUpdateStudent(e) {
  e.preventDefault();
  const form = e.target;
  const id = document.getElementById('editStudentId').value;
  const formData = new FormData(form);

  const payload = {
    name: formData.get('name').trim(),
    email: formData.get('email').trim().toLowerCase(),
    phone: formData.get('phone').trim(),
    department: formData.get('department'),
    year: parseInt(formData.get('year')),
    cgpa: formData.get('cgpa') ? parseFloat(formData.get('cgpa')) : null,
    gender: parseInt(formData.get('gender')),
    status: parseInt(formData.get('status'))
  };

  try {
    const res = await fetch(`${API_BASE}/students/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await res.json();

    if (!res.ok || !data.success) {
      const errMsg = data.message || (data.errors ? Object.values(data.errors).flat().join('<br>') : 'Update failed');
      Swal.fire({ icon: 'error', title: 'Error', html: errMsg });
      return;
    }

    editModal.hide();
    Swal.fire({ icon: 'success', title: 'Updated!', text: 'Student details updated', timer: 1800, showConfirmButton: false });
    loadStudents();
    loadAnalytics();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Error', text: err.message });
  }
}

async function viewStudent(id) {
  try {
    const [resStudent, resEnroll] = await Promise.all([
      fetch(`${API_BASE}/students/${id}`).then(r => r.json()),
      fetch(`${API_BASE}/courses/student/${id}`).then(r => r.json())
    ]);

    if (!resStudent.success) return;
    const s = resStudent.data;
    const enrollments = resEnroll.success ? resEnroll.data : [];

    let coursesHtml = '<p class="text-muted small mb-0">No courses enrolled yet.</p>';
    if (enrollments.length > 0) {
      coursesHtml = `
        <div class="list-group list-group-flush border rounded-2 small mt-2">
          ${enrollments.map(e => `
            <div class="list-group-item d-flex justify-content-between align-items-center">
              <div>
                <strong>${escapeHtml(e.courseCode)}</strong> - ${escapeHtml(e.courseTitle)}
                <div class="text-muted small">${escapeHtml(e.semester)}</div>
              </div>
              <span class="badge bg-primary">${escapeHtml(e.grade || 'In Progress')}</span>
            </div>
          `).join('')}
        </div>
      `;
    }

    const content = document.getElementById('viewStudentContent');
    content.innerHTML = `
      <div class="row">
        <div class="col-md-5 text-center border-end">
          <div class="avatar-circle mx-auto mb-2 bg-primary text-white d-inline-flex align-items-center justify-content-center rounded-circle" style="width: 70px; height: 70px; font-size: 26px;">
            ${escapeHtml(s.name.charAt(0))}
          </div>
          <h5 class="fw-bold mb-0">${escapeHtml(s.name)}</h5>
          <span class="badge bg-primary-subtle text-primary font-monospace mt-1">${escapeHtml(s.registerNumber)}</span>
          <div class="mt-3 text-start small">
            <p class="mb-1"><strong>Status:</strong> <span class="badge-status badge-${s.status.toLowerCase()}">${s.status}</span></p>
            <p class="mb-1"><strong>CGPA:</strong> ${s.cgpa ? s.cgpa.toFixed(2) : 'N/A'}</p>
            <p class="mb-1"><strong>Year:</strong> Year ${s.year}</p>
            <p class="mb-1"><strong>Gender:</strong> ${s.gender}</p>
          </div>
        </div>
        <div class="col-md-7">
          <h6 class="fw-bold text-primary mb-2"><i class="fa-solid fa-address-book me-1"></i>Contact & Department</h6>
          <p class="small mb-1"><strong>Email:</strong> ${escapeHtml(s.email)}</p>
          <p class="small mb-1"><strong>Phone:</strong> ${escapeHtml(s.phone)}</p>
          <p class="small mb-3"><strong>Department:</strong> ${escapeHtml(s.department)}</p>
          
          <h6 class="fw-bold text-primary mb-1"><i class="fa-solid fa-graduation-cap me-1"></i>Enrolled Courses (${enrollments.length})</h6>
          ${coursesHtml}
        </div>
      </div>
    `;
    viewModal.show();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Error', text: err.message });
  }
}

async function deleteStudent(id, name) {
  const result = await Swal.fire({
    title: 'Are you sure?',
    text: `Delete student "${name}"? (Soft Delete will be applied)`,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#ef4444',
    confirmButtonText: 'Yes, Delete!'
  });

  if (!result.isConfirmed) return;

  try {
    const res = await fetch(`${API_BASE}/students/${id}`, { method: 'DELETE' });
    const data = await res.json();
    if (!res.ok || !data.success) return Swal.fire({ icon: 'error', title: 'Failed', text: data.message });

    Swal.fire({ icon: 'success', title: 'Deleted!', text: 'Student removed (Soft Deleted)', timer: 1500, showConfirmButton: false });
    loadStudents();
    loadAnalytics();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Error', text: err.message });
  }
}

function exportCsv() {
  window.location.href = `${API_BASE}/students/export/csv`;
}

// -------------------------------------------------------------
// 2. BULK CSV IMPORT
// -------------------------------------------------------------
async function handleBulkImport(e) {
  e.preventDefault();
  const fileInput = document.getElementById('csvFileInput');
  if (!fileInput.files.length) return Swal.fire({ icon: 'warning', text: 'Select a CSV file first' });

  const formData = new FormData();
  formData.append('file', fileInput.files[0]);

  Swal.fire({ title: 'Importing...', text: 'Validating records and inserting...', didOpen: () => Swal.showLoading() });

  try {
    const res = await fetch(`${API_BASE}/students/import/csv`, { method: 'POST', body: formData });
    const data = await res.json();

    if (!res.ok || !data.success) {
      Swal.fire({ icon: 'error', title: 'Import Failed', text: data.message || 'Error importing CSV' });
      return;
    }

    bulkImportModal.hide();
    fileInput.value = '';

    const summary = data.data;
    let detailsHtml = `<strong>Success:</strong> ${summary.successCount} students added.<br><strong>Failed/Skipped:</strong> ${summary.failureCount} rows.`;
    if (summary.errors && summary.errors.length) {
      detailsHtml += `<br><br><div class="text-danger small text-start" style="max-height: 120px; overflow-y:auto;"><strong>Errors:</strong><br>${summary.errors.join('<br>')}</div>`;
    }

    Swal.fire({ icon: 'success', title: 'Bulk Import Finished', html: detailsHtml });
    loadStudents();
    loadAnalytics();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Network Error', text: err.message });
  }
}

function downloadSampleCsv() {
  const sample = "Name,RegisterNumber,Email,Department,Year,Phone,CGPA\nAnand Raj,REG2024CS07,anand.r@college.edu,Computer Science and Engineering,2,+919876543220,8.50\nKavitha S,REG2024IT08,kavitha.s@college.edu,Information Technology,3,+919876543221,9.05";
  const blob = new Blob([sample], { type: 'text/csv' });
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.setAttribute('href', url);
  a.setAttribute('download', 'sample_students_import.csv');
  a.click();
}

// -------------------------------------------------------------
// 3. COURSES & ENROLLMENT
// -------------------------------------------------------------
async function loadCourses() {
  const container = document.getElementById('coursesContainer');
  container.innerHTML = `<div class="col-12 text-center py-4 text-muted"><i class="fa-solid fa-spinner fa-spin me-2"></i>Loading courses...</div>`;

  try {
    const res = await fetch(`${API_BASE}/courses`);
    const data = await res.json();
    if (!data.success) throw new Error(data.message);

    allCoursesCache = data.data;
    document.getElementById('statTotalCourses').innerText = allCoursesCache.length;

    if (!allCoursesCache.length) {
      container.innerHTML = `<div class="col-12 text-center py-4 text-muted">No courses found. Add a course to start.</div>`;
      return;
    }

    container.innerHTML = allCoursesCache.map(c => `
      <div class="col-12 col-md-6 col-lg-4">
        <div class="course-card p-3 h-100 d-flex flex-column justify-content-between">
          <div>
            <div class="d-flex justify-content-between align-items-start mb-2">
              <span class="badge bg-primary font-monospace">${escapeHtml(c.courseCode)}</span>
              <span class="badge bg-secondary-subtle text-secondary">${c.credits} Credits</span>
            </div>
            <h6 class="fw-bold mb-1">${escapeHtml(c.title)}</h6>
            <p class="text-muted small mb-2"><i class="fa-solid fa-building-columns me-1"></i>${escapeHtml(c.department)}</p>
            <p class="small text-secondary mb-3">${escapeHtml(c.description || 'No description provided.')}</p>
          </div>
          <div class="border-top pt-2 d-flex justify-content-between align-items-center">
            <span class="small text-muted"><i class="fa-solid fa-users me-1 text-primary"></i><strong>${c.enrolledStudentCount}</strong> enrolled</span>
            <div>
              <button class="btn btn-sm btn-outline-danger" onclick="deleteCourse(${c.id}, '${escapeHtml(c.courseCode)}')"><i class="fa-solid fa-trash"></i></button>
            </div>
          </div>
        </div>
      </div>
    `).join('');

    updateCourseSelectDropdowns();
  } catch (err) {
    container.innerHTML = `<div class="col-12 text-center text-danger py-4">Error: ${err.message}</div>`;
  }
}

async function handleCreateCourse(e) {
  e.preventDefault();
  const form = e.target;
  const formData = new FormData(form);

  const payload = {
    courseCode: formData.get('courseCode').trim().toUpperCase(),
    title: formData.get('title').trim(),
    department: formData.get('department'),
    credits: parseInt(formData.get('credits')),
    description: formData.get('description')?.trim()
  };

  try {
    const res = await fetch(`${API_BASE}/courses`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await res.json();

    if (!res.ok || !data.success) return Swal.fire({ icon: 'error', title: 'Error', text: data.message });

    addCourseModal.hide();
    form.reset();
    Swal.fire({ icon: 'success', title: 'Course Added', timer: 1500, showConfirmButton: false });
    loadCourses();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Error', text: err.message });
  }
}

async function deleteCourse(id, code) {
  const confirm = await Swal.fire({
    title: 'Delete Course?',
    text: `Soft delete course ${code}?`,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#ef4444'
  });
  if (!confirm.isConfirmed) return;

  try {
    const res = await fetch(`${API_BASE}/courses/${id}`, { method: 'DELETE' });
    const data = await res.json();
    if (!res.ok || !data.success) return Swal.fire({ icon: 'error', title: 'Error', text: data.message });

    Swal.fire({ icon: 'success', title: 'Deleted', timer: 1500, showConfirmButton: false });
    loadCourses();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Error', text: err.message });
  }
}

async function handleEnrollStudent(e) {
  e.preventDefault();
  const form = e.target;
  const formData = new FormData(form);

  const payload = {
    studentId: parseInt(formData.get('studentId')),
    courseId: parseInt(formData.get('courseId')),
    semester: formData.get('semester').trim(),
    grade: formData.get('grade')
  };

  try {
    const res = await fetch(`${API_BASE}/courses/enroll`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await res.json();
    if (!res.ok || !data.success) return Swal.fire({ icon: 'error', title: 'Enrollment Failed', text: data.message });

    enrollModal.hide();
    Swal.fire({ icon: 'success', title: 'Enrolled Successfully!', timer: 1800, showConfirmButton: false });
    loadCourses();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Error', text: err.message });
  }
}

// -------------------------------------------------------------
// 4. ATTENDANCE MANAGEMENT
// -------------------------------------------------------------
async function loadTodayAttendance() {
  const dateVal = document.getElementById('attendanceDateInput').value;
  await fetchAndRenderAttendance(dateVal);
}

async function loadAttendanceBySelectedDate() {
  const dateVal = document.getElementById('attendanceDateInput').value;
  await fetchAndRenderAttendance(dateVal);
}

async function fetchAndRenderAttendance(dateStr) {
  const tbody = document.getElementById('attendanceTableBody');
  tbody.innerHTML = `<tr><td colspan="6" class="text-center py-4 text-muted"><i class="fa-solid fa-spinner fa-spin me-2"></i>Loading attendance...</td></tr>`;

  try {
    const res = await fetch(`${API_BASE}/attendance/date?date=${dateStr}`);
    const data = await res.json();
    if (!data.success) throw new Error(data.message);

    const records = data.data;
    if (!records || records.length === 0) {
      tbody.innerHTML = `
        <tr>
          <td colspan="6" class="text-center py-5 text-muted">
            <i class="fa-solid fa-calendar-xmark fs-2 d-block mb-2 text-secondary"></i>
            No attendance records logged for ${dateStr}. Click "Mark Attendance" to record.
          </td>
        </tr>
      `;
      return;
    }

    tbody.innerHTML = records.map(r => {
      const badgeClass = `badge-${r.status.toLowerCase()}`;
      return `
        <tr>
          <td class="font-monospace fw-semibold text-primary">${escapeHtml(r.registerNumber)}</td>
          <td class="fw-semibold">${escapeHtml(r.studentName)}</td>
          <td>${new Date(r.date).toLocaleDateString()}</td>
          <td><span class="badge-status ${badgeClass}">${escapeHtml(r.status)}</span></td>
          <td class="text-muted small">${escapeHtml(r.remarks || '-')}</td>
          <td class="text-end">
            <button class="btn btn-outline-success btn-sm" onclick="viewStudentAttendanceSummary(${r.studentId})">
              <i class="fa-solid fa-chart-pie me-1"></i> % Summary
            </button>
          </td>
        </tr>
      `;
    }).join('');
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="6" class="text-center text-danger py-4">Error: ${err.message}</td></tr>`;
  }
}

async function handleMarkAttendance(e) {
  e.preventDefault();
  const form = e.target;
  const formData = new FormData(form);

  const payload = {
    studentId: parseInt(formData.get('studentId')),
    date: formData.get('date'),
    status: parseInt(formData.get('status')),
    remarks: formData.get('remarks')?.trim()
  };

  try {
    const res = await fetch(`${API_BASE}/attendance/mark`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await res.json();
    if (!res.ok || !data.success) return Swal.fire({ icon: 'error', title: 'Error', text: data.message });

    markAttendanceModal.hide();
    Swal.fire({ icon: 'success', title: 'Attendance Saved!', timer: 1500, showConfirmButton: false });
    loadTodayAttendance();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Error', text: err.message });
  }
}

async function viewStudentAttendanceSummary(studentId) {
  try {
    const res = await fetch(`${API_BASE}/attendance/student/${studentId}`);
    const data = await res.json();
    if (!data.success) return Swal.fire({ icon: 'error', title: 'Error', text: data.message });

    const s = data.data;
    const pctColor = s.attendancePercentage >= 75 ? 'bg-success' : (s.attendancePercentage >= 60 ? 'bg-warning' : 'bg-danger');

    const content = document.getElementById('attendanceSummaryContent');
    content.innerHTML = `
      <div class="text-center mb-3">
        <h5 class="fw-bold mb-0">${escapeHtml(s.studentName)}</h5>
        <span class="badge bg-primary-subtle text-primary font-monospace">${escapeHtml(s.registerNumber)}</span>
      </div>

      <div class="card p-3 mb-3 border-0 bg-light">
        <div class="d-flex justify-content-between align-items-center mb-1">
          <span class="small fw-semibold">Attendance Percentage</span>
          <span class="fw-bold fs-5">${s.attendancePercentage}%</span>
        </div>
        <div class="progress" style="height: 10px;">
          <div class="progress-bar ${pctColor}" style="width: ${s.attendancePercentage}%"></div>
        </div>
        <small class="text-muted mt-2 d-block">
          ${s.attendancePercentage >= 75 ? '✅ Eligible for semester exams (>75%)' : '⚠️ Shortage of attendance (<75%)'}
        </small>
      </div>

      <div class="row text-center g-2 mb-3">
        <div class="col-3">
          <div class="border rounded p-2"><span class="small text-muted d-block">Total</span><strong>${s.totalDays}</strong></div>
        </div>
        <div class="col-3">
          <div class="border rounded p-2 text-success"><span class="small text-muted d-block">Present</span><strong>${s.presentDays}</strong></div>
        </div>
        <div class="col-3">
          <div class="border rounded p-2 text-danger"><span class="small text-muted d-block">Absent</span><strong>${s.absentDays}</strong></div>
        </div>
        <div class="col-3">
          <div class="border rounded p-2 text-warning"><span class="small text-muted d-block">Late</span><strong>${s.lateDays}</strong></div>
        </div>
      </div>

      <h6 class="fw-bold small text-muted mb-2">Recent Attendance Log</h6>
      <div style="max-height: 160px; overflow-y:auto;" class="border rounded">
        <table class="table table-sm mb-0 small">
          <tbody>
            ${s.recentRecords.map(r => `
              <tr>
                <td>${new Date(r.date).toLocaleDateString()}</td>
                <td><span class="badge-status badge-${r.status.toLowerCase()}">${r.status}</span></td>
                <td class="text-muted">${escapeHtml(r.remarks || '-')}</td>
              </tr>
            `).join('')}
          </tbody>
        </table>
      </div>
    `;

    attendanceSummaryModal.show();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Error', text: err.message });
  }
}

// -------------------------------------------------------------
// 5. AUDIT LOGS
// -------------------------------------------------------------
async function loadAuditLogs() {
  const tbody = document.getElementById('auditTableBody');
  tbody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-muted"><i class="fa-solid fa-spinner fa-spin me-2"></i>Loading audit trails...</td></tr>`;

  try {
    const res = await fetch(`${API_BASE}/students/audit-logs?count=30`);
    const data = await res.json();
    if (!data.success) throw new Error(data.message);

    const logs = data.data;
    if (!logs || !logs.length) {
      tbody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-muted">No audit logs recorded yet.</td></tr>`;
      return;
    }

    tbody.innerHTML = logs.map(l => {
      let badgeClass = 'bg-secondary';
      if (l.action.includes('CREATE') || l.action.includes('IMPORT')) badgeClass = 'bg-success';
      if (l.action.includes('UPDATE')) badgeClass = 'bg-warning text-dark';
      if (l.action.includes('DELETE')) badgeClass = 'bg-danger';

      return `
        <tr>
          <td class="small text-muted font-monospace">${new Date(l.timestamp).toLocaleString()}</td>
          <td><span class="badge ${badgeClass}">${escapeHtml(l.action)}</span></td>
          <td class="fw-semibold">${escapeHtml(l.entityName)}</td>
          <td class="font-monospace small">${l.entityId || '-'}</td>
          <td class="small">${escapeHtml(l.details)}</td>
        </tr>
      `;
    }).join('');
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="5" class="text-center text-danger py-4">Error: ${err.message}</td></tr>`;
  }
}

// -------------------------------------------------------------
// 6. ANALYTICS & HELPERS
// -------------------------------------------------------------
async function loadAnalytics() {
  try {
    const res = await fetch(`${API_BASE}/analytics/dashboard`);
    const data = await res.json();
    if (!data.success) return;

    const stats = data.data;
    document.getElementById('statTotalStudents').innerText = stats.totalStudents;
    document.getElementById('statActiveStudents').innerText = stats.activeStudents;
    document.getElementById('statAvgCGPA').innerText = stats.averageCGPA.toFixed(2);

    const deptSelect = document.getElementById('departmentFilter');
    if (deptSelect.options.length <= 1) {
      Object.keys(stats.departmentDistribution).forEach(dept => {
        const opt = document.createElement('option');
        opt.value = dept;
        opt.innerText = dept;
        deptSelect.appendChild(opt);
      });
    }

    renderChart(stats.departmentDistribution);
  } catch (e) {
    console.error('Analytics load error:', e);
  }
}

function renderChart(deptDistribution) {
  const ctx = document.getElementById('deptChart').getContext('2d');
  const labels = Object.keys(deptDistribution);
  const data = Object.values(deptDistribution);

  if (deptChart) deptChart.destroy();

  deptChart = new Chart(ctx, {
    type: 'bar',
    data: {
      labels: labels.map(l => l.length > 20 ? l.substring(0, 18) + '...' : l),
      datasets: [{
        label: 'Students',
        data: data,
        backgroundColor: ['#3b82f6', '#10b981', '#8b5cf6', '#f59e0b', '#ec4899', '#06b6d4'],
        borderRadius: 6
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } }
    }
  });
}

async function updateStudentSelectDropdowns() {
  try {
    const res = await fetch(`${API_BASE}/students?pageSize=50`);
    const data = await res.json();
    if (!data.success) return;

    const students = data.data.items;
    allStudentsCache = students;

    const enrollSelect = document.getElementById('enrollStudentSelect');
    const attendanceSelect = document.getElementById('attendanceStudentSelect');

    const optionsHtml = students.map(s => `
      <option value="${s.id}">${escapeHtml(s.registerNumber)} - ${escapeHtml(s.name)}</option>
    `).join('');

    if (enrollSelect) enrollSelect.innerHTML = optionsHtml;
    if (attendanceSelect) attendanceSelect.innerHTML = optionsHtml;
  } catch (e) {
    console.error(e);
  }
}

function updateCourseSelectDropdowns() {
  const enrollCourseSelect = document.getElementById('enrollCourseSelect');
  if (enrollCourseSelect && allCoursesCache.length) {
    enrollCourseSelect.innerHTML = allCoursesCache.map(c => `
      <option value="${c.id}">${escapeHtml(c.courseCode)} - ${escapeHtml(c.title)}</option>
    `).join('');
  }
}

function escapeHtml(text) {
  if (!text) return '';
  return String(text)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}
