@extends('layout')

@section('content')
<h2>List Reservations</h2>

<button onclick="fetchReservations()">Fetch Reservations</button>

<script>
async function fetchReservations(){
    const res = await apiRequest("/api/reservations", "GET");
}
</script>

@endsection