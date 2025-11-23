@extends('layout')

@section('content')
<h2>Update Facility</h2><br>
<form id="updateFacilityForm">
    <div class="form-group">
        <label>ID</label><br>
        <input type="number" id="id" value="1" min="1" autocomplete="off">
    </div>

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
        <input type="number" placeholder="1" id="capacity" min="1" step="1" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Status</label><br>
        <select id="status" autocomplete="off" style="width: 312px;">
            <option value="Optional..." selected disabled>Optional...</option>
            <option value="Available">Available</option>
            <option value="Unavailable">Unavailable</option>
        </select>
    </div>

    <button type="submit">Update</button>

    <h3 id="successMessage"></h3>
</form>

<script>
document.getElementById('updateFacilityForm').onsubmit = async (e) => {
    e.preventDefault();

    const id = Number(document.getElementById('id').value);
    const name = document.getElementById('name').value;
    const type = document.getElementById('type').value;
    const capacity = document.getElementById('capacity').value;
    const status = document.getElementById('status').value;

    let payload = {};

    if(name)
        payload["name"] = name;

    if(type)
        payload["type"] = type;

    if(capacity)
        payload["capacity"] = Number(capacity);

    if(status != "Optional...")
        payload["status"] = status;

    if(Object.keys(payload).length === 0)
        return showResponse("No changes made.");

    const res = await apiRequest(`/api/facility/${id}`, "PUT", payload);

    console.log(res);

    if(res["id"]){
        document.getElementById("successMessage").innerText = "Facility updated successfully.";
    }
}
</script>

@endsection