@extends('layout')

@section('content')
<h2>Monthly Usage Trends</h2><br>

<form id="monthlyTrendsForm">
    <div class="form-group">
        <label>Year</label><br>
        <input type="number" id="yearInput" autocomplete="off" min="2000" max="2100" required>
    </div>

    <button type="submit">Load Trends</button>
</form>

<div style="margin-top: 25px; width: 100%; max-width: 900px;">
    <canvas id="trendChart"></canvas>
</div>

<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<script>
// -------------------------------------------------------------
// Default year = current year
// -------------------------------------------------------------
document.getElementById("yearInput").value = new Date().getFullYear();

// Month labels
const MONTH_LABELS = [
    "Jan","Feb","Mar","Apr","May","Jun",
    "Jul","Aug","Sep","Oct","Nov","Dec"
];

document.getElementById("monthlyTrendsForm").onsubmit = async (e) => {
    e.preventDefault();

    const year = document.getElementById("yearInput").value;

    const res = await apiRequest(`/api/statistics/analytics/trends/monthly?year=${year}`, "GET");

    if (!res) return;

    renderMonthlyChart(res);
};

// -------------------------------------------------------------
// Render chart with 12 fixed monthly buckets
// -------------------------------------------------------------
function renderMonthlyChart(data) {
    const ctx = document.getElementById("trendChart").getContext("2d");

    // Destroy existing chart
    if (window.trendChart instanceof Chart) {
        window.trendChart.destroy();
    }

    // Prepare array of 12 months, all 0 by default
    const monthlyValues = new Array(12).fill(0);

    // Fill known values
    data.points.forEach(p => {
        const dt = new Date(p.timestamp);
        const monthIndex = dt.getMonth(); // 0..11
        monthlyValues[monthIndex] = p.count;
    });

    // Create chart
    window.trendChart = new Chart(ctx, {
        type: "bar",
        data: {
            labels: MONTH_LABELS,
            datasets: [{
                label: `Monthly Usage (${data.rangeLabel})`,
                data: monthlyValues,
                backgroundColor: "rgba(79,195,247,0.45)",
                borderColor: "#4fc3f7",
                borderWidth: 2,
                hoverBackgroundColor: "rgba(79,195,247,0.65)"
            }]
        },
        options: {
            responsive: true,
            scales: {
                x: {
                    ticks: { color: "#ddd" },
                    title: { display: true, text: "Month", color: "#ddd" }
                },
                y: {
                    beginAtZero: true,
                    ticks: { color: "#ddd" },
                    title: { display: true, text: "Usage Count", color: "#ddd" }
                }
            },
            plugins: {
                legend: {
                    labels: { color: "#ddd" }
                }
            }
        }
    });
}

</script>
@endsection
