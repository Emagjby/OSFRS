@extends('layout')

@section('content')
<h2>Search Reservations</h2><br>

<form id="searchForm">

    <div class="form-group">
        <label>User ID (optional)</label><br>
        <input type="number" id="userId" min="1" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Facility ID (optional)</label><br>
        <input type="number" id="facilityId" min="1" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Start Time (optional)</label><br>
        <input type="datetime-local" id="start" autocomplete="off">
    </div>

    <div class="form-group">
        <label>End Time (optional)</label><br>
        <input type="datetime-local" id="end" autocomplete="off">
    </div>

    <button type="submit">Search</button>
</form>

<br>

<table id="resultsTable" border="1" cellpadding="6" class="dataTable">
    <thead>
        <tr>
            <th>ID</th>
            <th>User</th>
            <th>Facility</th>
            <th>Start</th>
            <th>End</th>
            <th>Status</th>
        </tr>
    </thead>
    <tbody></tbody>
</table>

<script>

document.getElementById('searchForm').onsubmit = async (e) => {
    e.preventDefault();

    const params = new URLSearchParams();

    const userId = document.getElementById("userId").value;
    const facilityId = document.getElementById("facilityId").value;
    const start = document.getElementById("start").value;
    const end = document.getElementById("end").value;

    if (userId) params.append("userId", userId);
    if (facilityId) params.append("facilityId", facilityId);
    if (start) params.append("start", new Date(start).toISOString());
    if (end) params.append("end", new Date(end).toISOString());

    const url = "/api/reservations/search?" + params.toString();

    const data = await apiRequest(url, "GET");
    if (!data) return;

    renderResults(data);
};

function renderResults(list) {
    const tbody = document.querySelector("#resultsTable tbody");
    tbody.innerHTML = "";

    list.forEach(r => {
        const row = document.createElement("tr");

        row.innerHTML = `
            <td>${r.id}</td>
            <td>${r.userId}</td>
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