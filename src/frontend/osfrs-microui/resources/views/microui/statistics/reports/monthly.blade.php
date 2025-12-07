@extends('layout')

@section('content')
<h2>Monthly Usage Report</h2><br>

<form id="monthlyReportForm">
    <div class="form-group">
        <label>Year</label><br>
        <input type="number" id="year" min="2000" max="2100" value="{{ date('Y') }}" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Month</label><br>
        <input type="number" id="month" min="1" max="12" value="{{ date('m') }}" autocomplete="off">
    </div>

    <button type="submit">Generate Report</button>
</form>

<script>
document.getElementById("monthlyReportForm").onsubmit = async (e) => {
    e.preventDefault();

    const year = document.getElementById("year").value;
    const month = document.getElementById("month").value;

    let url = `/api/statistics/reports/monthly?year=${year}&month=${month}`;

    await apiRequest(url, "GET");
};
</script>

@endsection
