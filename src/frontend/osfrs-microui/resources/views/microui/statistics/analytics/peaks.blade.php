@extends('layout')

@section('content')
<h2>Peak Usage Analysis</h2><br>

<form id="peaksForm">

    <div class="form-group">
        <label>From</label><br>
        <input type="datetime-local" id="from" required>
    </div>

    <div class="form-group">
        <label>To</label><br>
        <input type="datetime-local" id="to" required>
    </div>

    <button type="submit">Analyze Peaks</button>
</form>

<script>
document.getElementById("peaksForm").onsubmit = async (e) => {
    e.preventDefault();

    const from = new Date(document.getElementById("from").value).toISOString();
    const to   = new Date(document.getElementById("to").value).toISOString();

    const res = await apiRequest(`/api/statistics/analytics/peaks?from=${from}&to=${to}`, "GET");
};
</script>
@endsection
