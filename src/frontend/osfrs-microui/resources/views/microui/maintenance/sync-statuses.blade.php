@extends('layout')

@section('content')
<h2>Sync Statuses</h2>

<button onclick="sync()">Sync</button>

<script>
    async function sync(){
        const res = await apiRequest("/api/maintenance/sync-statuses", "POST");
    }
</script>

@endsection