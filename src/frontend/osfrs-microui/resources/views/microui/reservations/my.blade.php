@extends('layout')

@section('content')
<h2>My Reservations</h2><br>

<table id="myReservationsTable" border="1" cellpadding="6" class="dataTable">
    <thead>
        <tr>
            <th>ID</th>
            <th>Facility</th>
            <th>Start</th>
            <th>End</th>
            <th>Status</th>
        </tr>
    </thead>
    <tbody></tbody>
</table>

<script>

window.onload = loadMyReservations;

async function loadMyReservations() {
    const data = await apiRequest("/api/reservations/my", "GET");
    if (!data) return;

    const tbody = document.querySelector("#myReservationsTable tbody");
    tbody.innerHTML = "";

    data.forEach(r => {
        const row = document.createElement("tr");

        row.innerHTML = `
            <td>${r.id}</td>
            <td>${r.facilityId}</td>
            <td>${new Date(r.startTime).toLocaleString()}</td>
            <td>${new Date(r.endTime).toLocaleString()}</td>
            <td>${r.status}</td>
        `;

        tbody.appendChild(row);
    });
}

</script>

@endsection