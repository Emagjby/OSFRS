@extends('layout')

@section('content')
<h2>Facility Availability Calendar</h2><br>

<form id="availabilityForm">
    <div class="form-group">
        <label>Facility ID</label><br>
        <input type="number" id="facilityId" min="1" value="1" required>
    </div>

    <div class="form-group">
        <label>Date</label><br>
        <input type="date" id="datePicker" required>
    </div>

    <button type="submit">Load Calendar</button>
</form>

<div style="margin-top: 20px;">
    <table id="calendarTable" border="1" cellpadding="4" class="dataTable">
        <thead>
            <tr>
                <th>Hour</th>
                <th>Status</th>
            </tr>
        </thead>
        <tbody></tbody>
    </table>
</div>

<script>

document.getElementById("datePicker").value =
    new Date().toISOString().split("T")[0];

document.getElementById("availabilityForm").onsubmit = async (e) => {
    e.preventDefault();

    const facilityId = Number(document.getElementById("facilityId").value);
    const date = document.getElementById("datePicker").value;

    const res = await apiRequest(
        `/api/reservations/availability/${facilityId}?date=${date}`,
        "GET"
    );

    if (!res) return;

    renderCalendar(res);
};

function renderCalendar(calendarData) {
    const tbody = document.querySelector("#calendarTable tbody");
    tbody.innerHTML = "";

    const selectedDate = new Date(document.getElementById("datePicker").value);

    const dayStart = new Date(selectedDate);
    dayStart.setHours(0, 0, 0, 0);

    const dayEnd = new Date(selectedDate);
    dayEnd.setHours(23, 59, 59, 999);

    function intersects(aStart, aEnd, bStart, bEnd) {
        return aStart < bEnd && bStart < aEnd;
    }

    for (let h = 0; h < 24; h++) {
        const hourStart = new Date(dayStart.getTime() + h * 3600000);
        const hourEnd   = new Date(hourStart.getTime() + 3600000);

        let status = "Free";

        for (const r of calendarData) {
            const rStart = new Date(r.startTime);
            const rEnd   = new Date(r.endTime);

            if (intersects(hourStart, hourEnd, rStart, rEnd)) {
                status = "Reserved";
                break;
            }
        }

        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${h.toString().padStart(2, "0")}:00</td>
            <td ${status === "Reserved" ? 'style="background:#660000;"' : ""}>${status}</td>
        `;
        tbody.appendChild(row);
    }
}

</script>

@endsection