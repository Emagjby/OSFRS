@extends('layout')

@section('content')
<h2>Delete Maintenance</h2><br>

<table id="maintenanceTable" border="1" cellpadding="6" class="dataTable">
    <thead>
        <tr>
            <th>ID</th>
            <th>Facility ID</th>
            <th>Description</th>
            <th>Start</th>
            <th>End</th>
            <th>Status</th>
            <th>Action</th>
        </tr>
    </thead>
    <tbody>

    </tbody>
</table>

<h3 id="successMessage"></h3>

<script>

window.onload = loadMaintenance;

async function loadMaintenance() {
    const data = await apiRequest("/api/maintenance/upcoming", "GET");
    if (!data) return;

    const tbody = document.querySelector("#maintenanceTable tbody");
    tbody.innerHTML = "";

    data.forEach(m => {
        const row = document.createElement("tr");

        row.innerHTML = `
            <td>${m.id}</td>
            <td>${m.facilityId}</td>
            <td>${m.description}</td>
            <td>${new Date(m.startTime).toLocaleString()}</td>
            <td>${new Date(m.endTime).toLocaleString()}</td>
            <td>${m.status}</td>
            <td>
                <button onclick="deleteMaintenance(${m.id})">Delete</button>
            </td>
        `;

        tbody.appendChild(row);
    });
}

async function deleteMaintenance(id) {
    const res = await apiRequest(`/api/maintenance/${id}`, "DELETE");

    if (res && res.message) {
        document.getElementById("successMessage").innerHTML = `Maintenance ${id} deleted.`;
        loadMaintenance();
    }
}

</script>

@endsection