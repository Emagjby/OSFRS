@extends('layout')

@section('content')
<h2>List Facilities</h2>

<button onclick="fetchFacilities()">Fetch Facilities</button>

<script>
    async function fetchFacilities(){
        const res = await apiRequest("/api/facility", "GET");
    }
</script>

@endsection