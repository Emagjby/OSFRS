@extends('layout')

@section('content')
<h2>Daily Usage Report</h2><br>

<form id="dailyReportForm">
    <div class="form-group">
        <label>Date (optional)</label><br>
        <input type="date" id="date" autocomplete="off">
    </div>

    <button type="submit">Generate Report</button>
</form>

<script>
document.getElementById("dailyReportForm").onsubmit = async (e) => {
    e.preventDefault();

    const date = document.getElementById("date").value;

    let url = "/api/statistics/reports/daily";
    if (date) {
        // convert to UTC (safe)
        url += "?date=" + toUtcIso(date);
    }

    await apiRequest(url, "GET");
};
</script>

@endsection
