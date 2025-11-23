@extends('layout')

@section('content')
<h2>Get Facility Availability</h2><br>

<form id="availabilityFacilityForm">
    <div class="form-group">
        <label>ID</label><br>
        <input type="number" id="id" autocomplete="off" min="1" value="1" required>
    </div>

    <button type="submit">Fetch Availability</button>
</form>

<script>

document.getElementById('availabilityFacilityForm').onsubmit = async (e) => {
    e.preventDefault();

    const id = Number(document.getElementById("id").value);

    const res = await apiRequest(`/api/facility/${id}/availability`);
}

</script>

@endsection