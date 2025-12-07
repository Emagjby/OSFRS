@extends('layout')

@section('content')
<h2>Daily Usage Trends</h2><br>

<form id="trendForm">
    <div class="form-group">
        <label>From</label><br>
        <input type="date" id="from" required>
    </div>

    <div class="form-group">
        <label>To</label><br>
        <input type="date" id="to" required>
    </div>

    <button type="submit">Load Trends</button>
</form>

<div id="stats" style="margin-top:20px;"></div>

<canvas id="trendChart" height="120" style="margin-top:30px;"></canvas>
<canvas id="changeChart" height="80" style="margin-top:40px;"></canvas>

<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<script>
document.getElementById("trendForm").onsubmit = async (e) => {
    e.preventDefault();

    const from = document.getElementById("from").value;
    const to   = document.getElementById("to").value;

    const res = await apiRequest(
        `/api/statistics/analytics/trends/daily?from=${from}&to=${to}`,
        "GET"
    );

    if (!res) return;

    renderStats(res);
    renderTrendChart(res);
    renderChangeChart(res);
};

function renderStats(data){
    document.getElementById("stats").innerHTML = `
        <h3>Statistics</h3>
        <p><b>Range:</b> ${data.rangeLabel}</p>
        <p><b>Total Count:</b> ${data.totalCount}</p>
        <p><b>Average Per Point:</b> ${data.averagePerPoint.toFixed(2)}</p>
    `;
}

function renderTrendChart(data) {
    const ctx = document.getElementById("trendChart").getContext("2d");

    // Make sure no residue from old chart
    if (window.trendChart instanceof Chart) {
        window.trendChart.destroy();
    }

    const labels = data.points.map(p =>
        new Date(p.timestamp).toLocaleDateString()
    );

    const values = data.points.map(p => p.count);

    window.trendChart = new Chart(ctx, {
        type: "line",
        data: {
            labels,
            datasets: [{
                label: "Daily Usage",
                data: values,
                borderColor: "#4fc3f7",
                backgroundColor: "rgba(79, 195, 247, 0.2)",
                borderWidth: 2,
                tension: 0.3,
                pointRadius: 3
            }]
        },
        options: {
            responsive: true,
            scales: {
                x: {
                    ticks: { color: "#ddd" }
                },
                y: {
                    ticks: { color: "#ddd" }
                }
            }
        }
    });
}

function renderChangeChart(data){
    const ctx = document.getElementById("changeChart").getContext("2d");
    if (window.changeChart) window.changeChart.destroy();

    const changes = data.percentageChange;
    const labels  = data.points.slice(1).map(
        p => new Date(p.timestamp).toLocaleDateString()
    );

    window.changeChart = new Chart(ctx, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [
                {
                    label: "% Change",
                    data: changes,
                    backgroundColor: changes.map(v =>
                        v >= 0 ? "rgba(76,175,80,0.7)" : "rgba(244,67,54,0.7)"
                    )
                }
            ]
        },
        options: {
            scales: {
                y: {
                    ticks: { color: "#ccc" },
                    grid: { color: "#333" }
                },
                x: {
                    ticks: { color: "#ccc" },
                    grid: { color: "#333" }
                }
            },
            plugins: {
                legend: {
                    labels: { color: "#eee" }
                }
            }
        }
    });
}
</script>

@endsection
