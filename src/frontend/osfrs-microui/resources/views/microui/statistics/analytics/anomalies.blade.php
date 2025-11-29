@extends('layout')

@section('content')
<h2>Anomaly Detection</h2><br>

<form id="anomalyForm">

    <div class="form-group">
        <label>From</label><br>
        <input type="datetime-local" id="from" required>
    </div>

    <div class="form-group">
        <label>To</label><br>
        <input type="datetime-local" id="to" required>
    </div>

    <div class="form-group">
        <label>Mode</label><br>
        <select id="mode" style="width: 312px;">
            <option value="z-score" selected>Z-Score</option>
            <option value="mad">MAD (Robust)</option>
        </select>
    </div>

    <button type="submit">Run Detection</button>
</form>

<script>
document.getElementById("anomalyForm").onsubmit = async (e) => {
    e.preventDefault();

    const from = new Date(document.getElementById("from").value).toISOString();
    const to   = new Date(document.getElementById("to").value).toISOString();
    const mode = document.getElementById("mode").value;

    const res = await apiRequest(
        `/api/statistics/analytics/anomalies?from=${from}&to=${to}&mode=${mode}`,
        "GET"
    );
};
</script>
@endsection
