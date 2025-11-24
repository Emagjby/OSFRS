@extends('layout')

@section('content')
<h2>Create Reservation</h2><br>

<form id="createReservationForm">

    <div class="form-group">
        <label>Facility ID</label><br>
        <input type="number" id="facilityId" min="1" required>
    </div>

    <div class="form-group">
        <label>Start Time</label><br>
        <input type="datetime-local" id="startTime" required>
    </div>

    <div class="form-group">
        <label>End Time</label><br>
        <input type="datetime-local" id="endTime" required>
    </div>

    <button type="submit">Create</button>

    <h3 id="successMessage"></h3>
</form>

<script>

document.getElementById('createReservationForm').onsubmit = async (e) => {
    e.preventDefault();

    const payload = {
        facilityId: Number(document.getElementById("facilityId").value),
        startTime: new Date(document.getElementById("startTime").value).toISOString(),
        endTime: new Date(document.getElementById("endTime").value).toISOString()
    };

    const res = await apiRequest("/api/reservations", "POST", payload);

    if (res && res.id) {
        document.getElementById("successMessage").innerText =
            `Reservation ${res.id} created successfully.`;
        e.target.reset();
    }
}

</script>

@endsection