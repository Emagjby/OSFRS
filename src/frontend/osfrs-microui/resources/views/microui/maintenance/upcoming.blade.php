@extends('layout')

@section('content')
<h2>Get Upcoming Maintenances</h2><br>

<button onclick="getUpcoming()">Fetch Maintenances</button>

<script>

async function getUpcoming(){
    const res = await apiRequest("/api/maintenance/upcoming", "GET");
}

</script>

@endsection