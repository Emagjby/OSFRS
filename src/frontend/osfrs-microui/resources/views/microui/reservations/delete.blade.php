@extends('layout')

@section('content')
<h2>Delete Reservation</h2><br>

<table id="reservationTable" border="1" cellpadding="6" class="dataTable">
    <thead>
        <tr>
            <th>ID</th>
            <th>Facility</th>
            <th>User</th>
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

window.onload = loadReservations;

async function loadReservations() {
    const data = await apiRequest("/api/reservations", "GET");
    if (!data) return;

    const tbody = document.querySelector("#reservationTable tbody");
    tbody.innerHTML = "";

    data.forEach(r => {
        const row = document.createElement("tr");

        const start = new Date(r.startTime).toLocaleString();
        const end   = new Date(r.endTime).toLocaleString();

        row.innerHTML = `
            <td>${r.id}</td>
            <td>${r.facilityId}</td>
            <td>${r.userId}</td>
            <td>${start}</td>
            <td>${end}</td>
            <td>${r.status}</td>
            <td>
                <button onclick="deleteReservation(${r.id})">Delete</button>
            </td>
        `;

        tbody.appendChild(row);
    });
}

async function deleteReservation(id) {
    const confirmed = confirm(`Delete reservation #${id}?`);

    if (!confirmed) return;

    const res = await apiRequest(`/api/reservations/${id}`, "DELETE");

    if (res && res.message) {
        document.getElementById("successMessage").innerHTML =
            `Reservation ${id} deleted.`;

        loadReservations();
    }
}

</script>

@endsection