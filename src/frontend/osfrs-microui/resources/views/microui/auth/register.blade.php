@extends('layout')

@section('content')
<h2>Register</h2>

<form id="registerForm">

    <div class="form-group">
        <label>Name</label><br>
        <input type="text" id="name" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Username</label><br>
        <input type="text" id="username" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Email</label><br>
        <input type="text" id="email" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Password</label><br>
        <input type="password" id="password" autocomplete="off">
    </div>

    <button type="submit">Register</button>
</form>

<script>
document.getElementById('registerForm').onsubmit = async (e) => {
    e.preventDefault();

    const payload = {
        name: document.getElementById('name').value,
        username: document.getElementById('username').value,
        email: document.getElementById('email').value,
        password: document.getElementById('password').value
    };

    const res = await apiRequest('/api/auth/register', "POST", payload);

    if (res && res.id) {
        showResponse({
            message: "User registered successfully",
            user: res
        });
    }
};
</script>

@endsection