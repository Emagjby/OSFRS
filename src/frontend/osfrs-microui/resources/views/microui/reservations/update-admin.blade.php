@extends('layout')

@section('content')
<h2>Admin Update Reservation</h2><br>

<form id="adminUpdateForm">

    <div class="form-group">
        <label>Reservation ID</label><br>
        <input type="number" id="id" min="1" required>
    </div>

    <div class="form-group">
        <label>New Start Time (optional)</label><br>
        <input type="datetime-local" id="startTime">
    </div>

    <div class="form-group">
        <label>New End Time (optional)</label><br>
        <input type="datetime-local" id="endTime">
    </div>

    <div class="form-group">
        <label>Status (optional)</label><br>
        <select id="status" style="width:312px;">
            <option value="" selected disabled>Optional…</option>
            <option value="Pending">Pending</option>
            <option value="Confirmed">Confirmed</option>
            <option value="Cancelled">Cancelled</option>
        </select>
    </div>

    <button type="submit">Apply Update</button>

    <h3 id="successMessage"></h3>
</form>

<script>
document.getElementById('adminUpdateForm').onsubmit = async (e) => {
    e.preventDefault();

    const id = Number(document.getElementById("id").value);

    const payload = {};

    const start = document.getElementById("startTime").value;
    if (start)
        payload.startTime = new Date(start).toISOString();

    const end = document.getElementById("endTime").value;
    if (end)
        payload.endTime = new Date(end).toISOString();

    const status = document.getElementById("status").value;
    if (status && status !== "")
        payload.status = status;

    if (Object.keys(payload).length === 0)
        return dump({ message: "No changes to update." });

    const res = await apiRequest(`/api/reservations/admin/update/${id}`, "PUT", payload);

    if (res && res.id) {
        document.getElementById("successMessage").innerText =
            `Reservation #${id} updated successfully.`;
    }
}
</script>

@endsection