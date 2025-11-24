<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>OSFRS Micro-UI</title>

    <link rel="stylesheet" href="{{ asset('css/layout.css') }}">
    <script src="{{ asset('js/main.js ')}}"></script>
</head>
<body>
    
    <nav>

        <div class="left">
            <div class="dropdown">
                <a class="dropbtn">Facilities</a>
                <div class="dropdown-content">
                    <a href="/microui/facility/list">List Facilities</a>
                    <a href="/microui/facility/create">Create Facility</a>
                    <a href="/microui/facility/get">Get Facility by ID</a>
                    <a href="/microui/facility/update">Update Facility</a>
                    <a href="/microui/facility/delete">Delete Facility</a>
                    <a href="/microui/facility/availability">Get Availability</a>
                    <a href="/microui/facility/availability-update">Update Availability</a>
                </div>
            </div>

            <div class="dropdown">
                <a class="dropbtn">Maintenance</a>
                <div class="dropdown-content">
                    <a href="/microui/maintenance/list-by-facility">Get Maintenance by Facility ID</a>
                    <a href="/microui/maintenance/upcoming">Get Upcoming Maintenances</a>
                    <a href="/microui/maintenance/schedule">Schedule Maintenance</a>
                    <a href="/microui/maintenance/update">Update Maintenance</a>
                    <a href="/microui/maintenance/delete">Delete Maintenance</a>
                    <a href="/microui/maintenance/sync-statuses">Sync Statuses</a>
                </div>
            </div>

            <div class="dropdown">
                <a class="dropbtn">Reservations</a>
                <div class="dropdown-content">
                    <a href="/microui/reservations/list">List Reservations</a>
                    <a href="/microui/reservations/calendar">Availability Calendar</a>
                    <a href="/microui/reservations/get">Get Reservations for Facility</a>
                    <a href="/microui/reservations/search">Search Reservations</a>
                    <a href="/microui/reservations/create">Create Reservation</a>
                    <a href="/microui/reservations/update">Update Reservation (U)</a>
                    <a href="/microui/reservations/delete">Delete Reservation</a>
                    <a href="/microui/reservations/cancel">Cancel Reservation</a>
                    <a href="/microui/reservations/my">My Reservation</a>
                    <a href="/microui/reservations/update-admin">Update Reservation (A)</a>
                </div>
            </div>
        </div>

        <div class="right">
            <a href="/microui/auth/login" class="loggedOut dropbtn">Login</a>
            <a href="/microui/auth/register" class="loggedOut dropbtn">Register</a>
            <a href="#" onclick="logout()" class="loggedIn dropbtn">Logout</a>
        </div>
        
    </nav>

    <p id="greeter"></p>

    <div class="container">
        @yield('content')
    </div>

    <div id="response"></div>

    <script>
        refreshAuthUI();
    </script>

</body>
</html>