@extends('layout')

@section('content')
<h2>Export Daily Usage Report (PDF)</h2><br>

<form id="pdfExportForm">
    <div class="form-group">
        <label>Date (optional)</label><br>
        <input type="date" id="date" autocomplete="off">
    </div>

    <button type="submit">Download PDF</button>
</form>

<script>
document.getElementById("pdfExportForm").onsubmit = async (e) => {
    e.preventDefault();

    const date = document.getElementById("date").value;

    let url = `/api/statistics/export/pdf`;
    if (date) url += `?date=${toUtcIso(date)}`;

    const token = localStorage.getItem("jwt");

    const res = await fetch("http://localhost:5025" + url, {
        method: "GET",
        headers: {
            "Authorization": token ? "Bearer " + token : ""
        }
    });

    const blob = await res.blob();

    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = "usage_report.pdf";
    link.click();
};
</script>

@endsection
