@extends('layout')

@section('content')
<h2>Visualization Dataset</h2><br>

<form id="vizForm">

    <div class="form-group">
        <label>From</label><br>
        <input type="datetime-local" id="from" required>
    </div>

    <div class="form-group">
        <label>To</label><br>
        <input type="datetime-local" id="to" required>
    </div>

    <button type="submit">Load Dataset</button>
</form>

<br><br>

<canvas id="vizChart" width="900" height="350"
        style="background:#000; border:1px solid #444; padding:10px"></canvas>

<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<script>
let vizChart = null;

document.getElementById("vizForm").onsubmit = async (e) => {
    e.preventDefault();

    const from = new Date(document.getElementById("from").value).toISOString();
    const to   = new Date(document.getElementById("to").value).toISOString();

    const data = await apiRequest(
        `/api/statistics/analytics/visualization?from=${from}&to=${to}`,
        "GET"
    );

    if (!data) return;

    renderVizChart(data);
};

function renderVizChart(data) {
    const ctx = document.getElementById("vizChart").getContext("2d");

    if (vizChart) vizChart.destroy();

    vizChart = new Chart(ctx, {
        type: data.chartType ?? "line",
        data: {
            labels: data.labels,
            datasets: [{
                label: "Usage",
                data: data.values,
                borderWidth: 2,
                tension: 0.25
            }]
        },
        options: {
            responsive: true,
            scales: {
                x: {
                    ticks: { color: "#bdbdbd" },
                    grid: { color: "#333" }
                },
                y: {
                    ticks: { color: "#bdbdbd" },
                    grid: { color: "#333" }
                }
            },
            plugins: {
                legend: {
                    labels: { color: "#fff" }
                },
                tooltip: {
                    backgroundColor: "#111",
                    borderColor: "#555",
                    borderWidth: 1
                }
            }
        }
    });
}
</script>

@endsection
