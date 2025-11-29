@extends('layout')

@section('content')
<h2>Daily Usage Aggregate</h2><br>

<form id="dailyAggregateForm">
    <div class="form-group">
        <label>Date</label><br>
        <input type="date" id="date" autocomplete="off">
    </div>

    <button type="submit">Fetch Daily Aggregate</button>
</form>

<script>
document.getElementById("date").value =
    new Date().toISOString().split("T")[0];

document.getElementById("dailyAggregateForm").onsubmit = async (e) => {
    e.preventDefault();

    const date = toUtcIso(document.getElementById("date").value);
    const query = date ? `?date=${date}` : "";

    const res = await apiRequest(`/api/statistics/aggregate/daily${query}`, "GET");
};
</script>

@endsection
