@extends('layout')

@section('content')
<h2>Schedule Maintenance</h2><br>

<form id="scheduleMaintenanceForm">
    
    <div class="form-group">
        <label>Facility ID</label><br>
        <input type="number" id="facilityId" min="1" value="1" autocomplete="off" required>
    </div>

    <div class="form-group">
        <label>Description</label><br>
        <input type="text" id="description" placeholder="ex. Deep cleaning" autocomplete="off" required>
    </div>

    <div class="form-group">
        <label>Start Time</label><br>
        <input type="datetime-local" id="startTime" autocomplete="off" required>
    </div>

    <div class="form-group">
        <label>End Time</label><br>
        <input type="datetime-local" id="endTime" autocomplete="off" required>
    </div>

    <div class="form-group">
        <label>Status</label><br>
        <select id="status" autocomplete="off" style="width: 312px;">
            <option value="Scheduled" selected>Scheduled</option>
            <option value="InProgress">InProgress</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
        </select>
    </div>

    <button type="submit">Create</button>
    <h3 id="successMessage"></h3>
</form>

<script>
document.getElementById('scheduleMaintenanceForm').onsubmit = async (e) => {
    e.preventDefault();

    const payload = {
        facilityId: Number(document.getElementById('facilityId').value),
        description: document.getElementById('description').value,
        startTime: new Date(document.getElementById('startTime').value).toISOString(),
        endTime: new Date(document.getElementById('endTime').value).toISOString(),
        status: document.getElementById('status').value
    };

    const res = await apiRequest("/api/maintenance", "POST", payload);

    if (res && res.id) {
        document.getElementById("successMessage").innerText = "Maintenance scheduled successfully.";
        e.target.reset();
    }
}
</script>

@endsection