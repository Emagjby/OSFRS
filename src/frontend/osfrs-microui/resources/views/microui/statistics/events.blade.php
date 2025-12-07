@extends('layout')

@section('content')
<h2>Usage Events</h2><br>

<form id="eventsForm">
    <div class="form-group">
        <label>Event Type</label><br>
        <input type="text" id="eventType" placeholder="Optional...">
    </div>

    <div class="form-group">
        <label>User ID</label><br>
        <input type="number" id="userId" min="1">
    </div>

    <div class="form-group">
        <label>Facility ID</label><br>
        <input type="number" id="facilityId" min="1">
    </div>

    <div class="form-group">
        <label>From</label><br>
        <input type="datetime-local" id="from">
    </div>

    <div class="form-group">
        <label>To</label><br>
        <input type="datetime-local" id="to">
    </div>

    <button type="submit">Fetch Events</button>
</form>

<script>

document.getElementById("eventsForm").onsubmit = async (e) => {
    e.preventDefault();

    const params = new URLSearchParams();

    const et = document.getElementById("eventType").value;
    const uid = document.getElementById("userId").value;
    const fid = document.getElementById("facilityId").value;
    const from = document.getElementById("from").value;
    const to   = document.getElementById("to").value;

    if(et) params.append("eventType", et);
    if(uid) params.append("userId", uid);
    if(fid) params.append("facilityId", fid);
    if(from) params.append("from", new Date(from).toISOString());
    if(to) params.append("to", new Date(to).toISOString());

    const res = await apiRequest(`/api/statistics/events?${params.toString()}`, "GET");
};

</script>

@endsection
