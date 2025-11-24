@extends('layout')

@section('content')
<h2>Cancel My Reservations</h2><br>

<table id="myReservationsTable" border="1" cellpadding="6" class="dataTable">
    <thead>
        <tr>
            <th>ID</th>
            <th>Facility</th>
            <th>Start</th>
            <th>End</th>
            <th>Status</th>
            <th>Action</th>
        </tr>
    </thead>
    <tbody></tbody>
</table>

<h3 id="successMessage"></h3>

<script>

// Load reservations on page load
window.onload = loadMyReservations;

async function loadMyReservations() {
    const data = await apiRequest("/api/reservations/my", "GET");
    if (!data) return;

    const tbody = document.querySelector("#myReservationsTable tbody");
    tbody.innerHTML = "";

    data
        .filter(r => r.status !== "Cancelled")
        .forEach(r => {
            const row = document.createElement("tr");

            row.innerHTML = `
                <td>${r.id}</td>
                <td>${r.facilityId}</td>
                <td>${new Date(r.startTime).toLocaleString()}</td>
                <td>${new Date(r.endTime).toLocaleString()}</td>
                <td>${r.status}</td>
                <td>
                    <button onclick="cancelReservation(${r.id})">Cancel</button>
                </td>
            `;

            tbody.appendChild(row);
        });
}

async function cancelReservation(id) {
    const confirmed = confirm(`Cancel reservation #${id}?`);

    if (!confirmed) return;

    const res = await apiRequest(`/api/reservations/cancel/${id}`, "PUT");

    if (res && res.message) {
        document.getElementById("successMessage").innerHTML =
            `Reservation ${id} cancelled.`;
        loadMyReservations();
    }
}

</script>

@endsection