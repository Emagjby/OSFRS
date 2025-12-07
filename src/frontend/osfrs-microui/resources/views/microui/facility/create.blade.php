@extends('layout')

@section('content')
<h2>Create Facility</h2><br>
<form id="createFacilityForm">
    <div class="form-group">
        <label>Name</label><br>
        <input type="text" id="name" placeholder="ex. Kristal" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Type</label><br>
        <input type="text" id="type" placeholder="ex. Gym" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Capacity</label><br>
        <input type="number" value="1" id="capacity" min="1" step="1" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Status</label><br>
        <select id="status" autocomplete="off" style="width: 312px;">
            <option value="Available" selected>Available</option>
            <option value="Unavailable">Unavailable</option>
        </select>
    </div>

    <button type="submit">Create</button>

    <h3 id="successMessage"></h3>
</form>

<script>
document.getElementById('createFacilityForm').onsubmit = async (e) => {
    e.preventDefault();

    const payload = {
        name: document.getElementById('name').value,
        type: document.getElementById('type').value,
        capacity: document.getElementById('capacity').value,
        status: document.getElementById('status').value,
    };

    const res = await apiRequest("/api/facility", "POST", payload);

    console.log(res);

    if(res["id"]){
        document.getElementById("successMessage").innerText = "Facility created successfully.";
        e.target.reset();
    }
}
</script>

@endsection