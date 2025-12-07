@extends('layout')

@section('content')
<h2>Run Usage Aggregation</h2><br>

<p>
    This will trigger the backend to recompute <strong>daily</strong> and <strong>monthly</strong> usage aggregates
    based on raw events.
</p>

<button id="runAggregationBtn">Run aggregation</button>

<h3 id="aggregationMessage"></h3>

<script>
document.getElementById("runAggregationBtn").onclick = async () => {
    const confirmRun = confirm(
        "Run aggregation now?\n\n" +
        "This will recompute daily and monthly statistics from usage events."
    );

    if (!confirmRun) return;

    const res = await apiRequest("/api/statistics/aggregate/run", "POST");

    if (res && res.message) {
        document.getElementById("aggregationMessage").innerText = res.message;
    } else {
        document.getElementById("aggregationMessage").innerText =
            "Aggregation request completed (check JSON box below for details).";
    }
};
</script>

@endsection
