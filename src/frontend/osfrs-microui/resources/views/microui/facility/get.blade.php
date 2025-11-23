@extends('layout')

@section('content')
<h2>Get facility by ID</h2><br>
<form id="getFacilityForm">
    <div class="form-group">
        <label>ID</label><br>
        <input type="number" id="id" autocomplete="off" min="1" value="1" required>
    </div>

    <button type="submit">Fetch Facility</button>
</form>

<script>
document.getElementById('getFacilityForm').onsubmit = async (e) => {
    e.preventDefault();

    const payload = {
        id: document.getElementById('id').value
    };

    const res = await apiRequest(`/api/facility/${payload.id}`, "GET");
}
</script>

@endsection