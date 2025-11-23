@extends('layout')

@section('content')
<h2>Delete Facility</h2><br>

<table id="facilityTable" border="1" cellpadding="6" style="border-color:#444; color:#ddd; width: 100%; background:#222;">
    <thead>
        <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Type</th>
            <th>Capacity</th>
            <th>Status</th>
            <th>Action</th>
        </tr>
    </thead>
    <tbody>

    </tbody>
</table>

<h3 id="successMessage"></h3>

<script>

window.onload = loadFacilities;

async function loadFacilities() {
    const data = await apiRequest("/api/facility", "GET");
    if (!data) return;

    const tbody = document.querySelector("#facilityTable tbody");
    tbody.innerHTML = "";

    data.forEach(f => {
        const row = document.createElement("tr");

        row.innerHTML = `
            <td>${f.id}</td>
            <td>${f.name}</td>
            <td>${f.type}</td>
            <td>${f.capacity}</td>
            <td>${f.status}</td>
            <td>
                <button onclick="deleteFacility(${f.id})">Delete</button>
            </td>
        `;

        tbody.appendChild(row);
    });
}

async function deleteFacility(id) {
    const res = await apiRequest(`/api/facility/${id}`, "DELETE");

    if (res && res.message) {
        document.getElementById("successMessage").innerHTML = `Facility ${id} deleted.`
        loadFacilities(); 
    }
}

</script>

@endsection