// Student Management System - Frontend App
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

document.addEventListener('DOMContentLoaded', () => {
  addModal = new bootstrap.Modal(document.getElementById('addStudentModal'));
  editModal = new bootstrap.Modal(document.getElementById('editStudentModal'));
  viewModal = new bootstrap.Modal(document.getElementById('viewStudentModal'));

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

// Fetch Students
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

    if (!data.success) {
      throw new Error(data.message || 'Failed to fetch students');
    }

    const paged = data.data;
    state.totalCount = paged.totalCount;
    state.totalPages = paged.totalPages;

    renderTable(paged.items);
    renderPagination(paged);
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

// Render Table
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
        <td><span class="text-truncate d-inline-block" style="max-width: 200px;">${escapeHtml(s.department)}</span></td>
        <td>Year ${s.year}</td>
        <td>${escapeHtml(s.phone)}</td>
        <td><span class="cgpa-pill ${cgpaPillClass}">${cgpa}</span></td>
        <td><span class="badge-status ${statusClass}">${escapeHtml(s.status)}</span></td>
        <td class="text-end text-nowrap">
          <button class="btn btn-outline-info action-btn me-1" title="View Details" onclick="viewStudent(${s.id})">
            <i class="fa-solid fa-eye"></i>
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

// Render Pagination Controls
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

// Fetch Analytics
async function loadAnalytics() {
  try {
    const res = await fetch(`${API_BASE}/analytics/dashboard`);
    const data = await res.json();
    if (!data.success) return;

    const stats = data.data;
    document.getElementById('statTotalStudents').innerText = stats.totalStudents;
    document.getElementById('statActiveStudents').innerText = stats.activeStudents;
    document.getElementById('statTotalDepartments').innerText = stats.totalDepartments;
    document.getElementById('statAvgCGPA').innerText = stats.averageCGPA.toFixed(2);

    // Populate Department Filter dropdown if empty
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

// Render Chart.js
function renderChart(deptDistribution) {
  const ctx = document.getElementById('deptChart').getContext('2d');
  const labels = Object.keys(deptDistribution);
  const data = Object.values(deptDistribution);

  if (deptChart) {
    deptChart.destroy();
  }

  deptChart = new Chart(ctx, {
    type: 'bar',
    data: {
      labels: labels.map(l => l.length > 20 ? l.substring(0, 18) + '...' : l),
      datasets: [{
        label: 'Number of Students',
        data: data,
        backgroundColor: [
          '#3b82f6', '#10b981', '#8b5cf6', '#f59e0b', '#ec4899', '#06b6d4'
        ],
        borderRadius: 6
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: { stepSize: 1 }
        }
      }
    }
  });
}

// Add Student Form Handler
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

// Open Edit Modal
async function openEditModal(id) {
  try {
    const res = await fetch(`${API_BASE}/students/${id}`);
    const data = await res.json();
    if (!data.success) {
      Swal.fire({ icon: 'error', title: 'Error', text: data.message });
      return;
    }

    const s = data.data;
    document.getElementById('editStudentId').value = s.id;
    document.getElementById('editRegisterNumber').value = s.registerNumber;
    document.getElementById('editName').value = s.name;
    document.getElementById('editEmail').value = s.email;
    document.getElementById('editPhone').value = s.phone;
    document.getElementById('editDepartment').value = s.department;
    document.getElementById('editYear').value = s.year;
    document.getElementById('editCGPA').value = s.cgpa || '';

    // Map strings to enum integers
    const genderMap = { 'Male': 1, 'Female': 2, 'Other': 3 };
    const statusMap = { 'Active': 1, 'Inactive': 2, 'Suspended': 3, 'Graduated': 4 };
    document.getElementById('editGender').value = genderMap[s.gender] || 1;
    document.getElementById('editStatus').value = statusMap[s.status] || 1;

    editModal.show();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Error', text: err.message });
  }
}

// Update Student Form Handler
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
    Swal.fire({ icon: 'success', title: 'Updated!', text: 'Student details updated successfully', timer: 1800, showConfirmButton: false });
    loadStudents();
    loadAnalytics();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Network Error', text: err.message });
  }
}

// View Student Details
async function viewStudent(id) {
  try {
    const res = await fetch(`${API_BASE}/students/${id}`);
    const data = await res.json();
    if (!data.success) return;

    const s = data.data;
    const content = document.getElementById('viewStudentContent');
    content.innerHTML = `
      <div class="text-center mb-3">
        <div class="avatar-circle mx-auto mb-2 bg-primary text-white d-inline-flex align-items-center justify-content-center rounded-circle" style="width: 64px; height: 64px; font-size: 24px;">
          ${escapeHtml(s.name.charAt(0))}
        </div>
        <h5 class="fw-bold mb-0">${escapeHtml(s.name)}</h5>
        <span class="badge bg-primary-subtle text-primary mt-1 font-monospace">${escapeHtml(s.registerNumber)}</span>
      </div>
      <table class="table table-sm">
        <tr><td class="text-muted">Email</td><td class="fw-semibold">${escapeHtml(s.email)}</td></tr>
        <tr><td class="text-muted">Phone</td><td class="fw-semibold">${escapeHtml(s.phone)}</td></tr>
        <tr><td class="text-muted">Department</td><td class="fw-semibold">${escapeHtml(s.department)}</td></tr>
        <tr><td class="text-muted">Year of Study</td><td class="fw-semibold">Year ${s.year}</td></tr>
        <tr><td class="text-muted">CGPA</td><td class="fw-semibold text-success">${s.cgpa ? s.cgpa.toFixed(2) : 'N/A'}</td></tr>
        <tr><td class="text-muted">Gender</td><td class="fw-semibold">${escapeHtml(s.gender)}</td></tr>
        <tr><td class="text-muted">Status</td><td class="fw-semibold">${escapeHtml(s.status)}</td></tr>
        <tr><td class="text-muted">Registered On</td><td class="small text-muted">${new Date(s.createdAt).toLocaleString()}</td></tr>
      </table>
    `;
    viewModal.show();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Error', text: err.message });
  }
}

// Delete Student
async function deleteStudent(id, name) {
  const result = await Swal.fire({
    title: 'Are you sure?',
    text: `Delete student "${name}"? (Soft Delete will be applied)`,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#ef4444',
    cancelButtonColor: '#64748b',
    confirmButtonText: 'Yes, Delete!'
  });

  if (!result.isConfirmed) return;

  try {
    const res = await fetch(`${API_BASE}/students/${id}`, { method: 'DELETE' });
    const data = await res.json();

    if (!res.ok || !data.success) {
      Swal.fire({ icon: 'error', title: 'Delete Failed', text: data.message });
      return;
    }

    Swal.fire({ icon: 'success', title: 'Deleted!', text: 'Student has been removed.', timer: 1500, showConfirmButton: false });
    loadStudents();
    loadAnalytics();
  } catch (err) {
    Swal.fire({ icon: 'error', title: 'Error', text: err.message });
  }
}

// Export CSV
function exportCsv() {
  window.location.href = `${API_BASE}/students/export/csv`;
}

// Utility: Escape HTML
function escapeHtml(text) {
  if (!text) return '';
  return String(text)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}

