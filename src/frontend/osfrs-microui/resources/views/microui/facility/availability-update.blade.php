@extends('layout')

@section('content')
<h2>Update Facility Availability</h2><br>

<form id="availabilityUpdateFacilityForm">
    <div class="form-group">
        <label>ID</label><br>
        <input type="number" id="id" autocomplete="off" min="1" value="1" required>
    </div>

    <div class="form-group">
        <select id="availability" autocomplete="off" style="width: 312px;">
            <option value="Select here..." selected disabled>Select here...</option>
            <option value="Available">Available</option>
            <option value="Unavailable">Unavailable</option>
        </select>
    </div>

    <button type="submit">Update Availability</button>
</form>

<script>

document.getElementById('availabilityUpdateFacilityForm').onsubmit = async (e) => {
    e.preventDefault();

    const id = Number(document.getElementById("id").value);
    const availability = document.getElementById("availability").value;

    if(availability === "Select here...")
        return showResponse("Please select the availability to update to.");

    const payload = availability == "Available" ? true : false;

    const res = await apiRequest(`/api/facility/${id}/availability`, "PATCH", payload);

    console.log(res);
}

</script>

@endsection