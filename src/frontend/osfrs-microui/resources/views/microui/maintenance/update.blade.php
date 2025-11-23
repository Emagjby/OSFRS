@extends('layout')

@section('content')
<h2>Update Maintenance Record</h2><br>

<form id="updateMaintenanceForm">
    <div class="form-group">
        <label>ID</label><br>
        <input type="number" id="id" value="1" min="1" autocomplete="off" required>
    </div>

    <div class="form-group">
        <label>Description</label><br>
        <input type="text" id="description" placeholder="Optional..." autocomplete="off">
    </div>

    <div class="form-group">
        <label>Start Time</label><br>
        <input type="datetime-local" id="startTime" autocomplete="off">
    </div>

    <div class="form-group">
        <label>End Time</label><br>
        <input type="datetime-local" id="endTime" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Status</label><br>
        <select id="status" autocomplete="off" style="width: 312px;">
            <option value="Optional..." selected disabled>Optional...</option>
            <option value="Scheduled">Scheduled</option>
            <option value="InProgress">InProgress</option>
            <option value="Completed">Completed</option>
            <option value="Cancelled">Cancelled</option>
        </select>
    </div>

    <button type="submit">Update</button>

    <h3 id="successMessage"></h3>
</form>

<script>

document.getElementById('updateMaintenanceForm').onsubmit = async (e) => {
    e.preventDefault();

    const id = Number(document.getElementById('id').value);

    const description = document.getElementById('description').value;
    const startTime   = document.getElementById('startTime').value;
    const endTime     = document.getElementById('endTime').value;
    const status      = document.getElementById('status').value;

    let payload = {};

    if (description)
        payload["description"] = description;

    if (startTime)
        payload["startTime"] = new Date(startTime).toISOString();

    if (endTime)
        payload["endTime"] = new Date(endTime).toISOString();

    if (status !== "Optional...")
        payload["status"] = status;

    if (Object.keys(payload).length === 0) {
        return dump("No changes made.");
    }

    const res = await apiRequest(`/api/maintenance/${id}`, "PUT", payload);

    if (res && res.id) {
        document.getElementById("successMessage").innerText =
            `Maintenance record ${id} updated successfully.`;
    }
}

</script>

@endsection