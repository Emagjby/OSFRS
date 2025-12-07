@extends('layout')

@section('content')
<h2>Update My Reservation</h2><br>

<form id="updateReservationForm">

    <div class="form-group">
        <label>Reservation ID</label><br>
        <input type="number" id="reservationId" min="1" required>
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
        <select id="status" style="width: 312px;">
            <option value="" selected disabled>Optional...</option>
            <option value="Active">Active</option>
            <option value="Cancelled">Cancelled</option>
            <option value="Completed">Completed</option>
        </select>
    </div>

    <button type="submit">Update</button>

    <h3 id="successMessage"></h3>
</form>

<script>

document.getElementById('updateReservationForm').onsubmit = async (e) => {
    e.preventDefault();

    const id = Number(document.getElementById("reservationId").value);

    let payload = {};

    const startVal = document.getElementById("startTime").value;
    const endVal = document.getElementById("endTime").value;
    const statusVal = document.getElementById("status").value;

    if (startVal && startVal.trim() !== "")
        payload.startTime = new Date(startVal).toISOString();

    if (endVal && endVal.trim() !== "")
        payload.endTime = new Date(endVal).toISOString();

    if (statusVal && statusVal.trim() !== "")
        payload.status = statusVal;

    if (Object.keys(payload).length === 0)
        return dump({ message: "No changes to update." });

    const res = await apiRequest(`/api/reservations/${id}`, "PUT", payload);

    if (res && res.id) {
        document.getElementById("successMessage").innerText =
            `Reservation ${id} updated successfully.`;
    }
};

</script>

@endsection