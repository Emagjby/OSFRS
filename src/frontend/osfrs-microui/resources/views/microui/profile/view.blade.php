@extends('layout')

@section('content')
<h2>My Profile</h2><br>

<button onclick="loadProfile()">Load Profile</button>

<table id="profileTable" border="1" cellpadding="6" class="dataTable" style="margin-top:20px;">
    <thead>
        <tr>
            <th>Field</th>
            <th>Value</th>
        </tr>
    </thead>
    <tbody></tbody>
</table>

<script>

async function loadProfile() {
    const data = await apiRequest("/api/profile", "GET");
    if (!data) return;

    const tbody = document.querySelector("#profileTable tbody");
    tbody.innerHTML = "";

    Object.keys(data).forEach(key => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${key}</td>
            <td>${data[key]}</td>
        `;
        tbody.appendChild(row);
    });
}

</script>

@endsection
