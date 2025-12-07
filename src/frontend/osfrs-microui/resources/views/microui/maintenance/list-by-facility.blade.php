@extends('layout')

@section('content')
<h2>Get Maintenances by Facility</h2><br>
<form id="listMaintenanceByFacilityForm">
    <div class="form-group">
        <label>Facility ID</label><br>
        <input type="number" id="facilityId" autocomplete="off" min="1" value="1" required>
    </div>

    <button type="submit">Fetch Maintenances</button>
</form>

<script>
document.getElementById('listMaintenanceByFacilityForm').onsubmit = async (e) => {
    e.preventDefault();

    const payload = {
        facilityId: document.getElementById('facilityId').value
    };

    const res = await apiRequest(`/api/maintenance/facility/${payload.facilityId}`, "GET");
}
</script>

@endsection