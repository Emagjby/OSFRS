@extends('layout')

@section('content')
<h2>Update Profile</h2><br>

<form id="updateProfileForm">

    <div class="form-group">
        <label>Name (optional)</label><br>
        <input type="text" id="name" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Username (optional)</label><br>
        <input type="text" id="username" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Email (optional)</label><br>
        <input type="email" id="email" autocomplete="off">
    </div>

    <button type="submit">Update</button>

    <h3 id="successMessage"></h3>

</form>

<script>

document.getElementById('updateProfileForm').onsubmit = async (e) => {
    e.preventDefault();

    const payload = {};

    const name = document.getElementById("name").value;
    if (name) payload.name = name;

    const username = document.getElementById("username").value;
    if (username) payload.username = username;

    const email = document.getElementById("email").value;
    if (email) payload.email = email;

    if (Object.keys(payload).length === 0)
        return dump({ message: "No changes to apply." });

    const res = await apiRequest("/api/profile", "PUT", payload);

    if (res && res.id) {
        document.getElementById("successMessage").innerText =
            "Profile updated successfully.";
    }
}

</script>

@endsection
