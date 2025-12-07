@extends('layout')

@section('content')
<h2>Reservations for Facility</h2><br>

<form id="facilityReservationsForm">

    <div class="form-group">
        <label>Facility ID</label><br>
        <input type="number" id="facilityId" min="1" value="1" required>
    </div>

    <div class="form-group">
        <label>Start (optional)</label><br>
        <input type="datetime-local" id="start" autocomplete="off">
    </div>

    <div class="form-group">
        <label>End (optional)</label><br>
        <input type="datetime-local" id="end" autocomplete="off">
    </div>

    <button type="submit">Fetch</button>
</form>

<script>

document.getElementById('facilityReservationsForm').onsubmit = async (e) => {
    e.preventDefault();

    const facilityId = Number(document.getElementById("facilityId").value);
    const start = document.getElementById("start").value;
    const end = document.getElementById("end").value;

    const params = new URLSearchParams();

    if (start) params.append("start", new Date(start).toISOString());
    if (end) params.append("end", new Date(end).toISOString());

    const url = `/api/reservations/facility/${facilityId}?${params.toString()}`;

    const data = await apiRequest(url, "GET");
    if (!data) return;
};

</script>

@endsection