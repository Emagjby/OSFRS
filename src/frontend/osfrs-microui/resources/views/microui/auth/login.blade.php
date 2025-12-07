@extends('layout')

@section('content')
<h2>Login</h2>

<form id="loginForm">

    <div class="form-group">
        <label>Username or Email</label><br>
        <input type="text" id="usernameoremail" autocomplete="off">
    </div>

    <div class="form-group">
        <label>Password</label><br>
        <input type="password" id="password" autocomplete="off">
    </div>

    <button type="submit">Login</button>
</form>

<script>
    document.getElementById('loginForm').onsubmit = async (e) => {
        e.preventDefault();

        const payload = {
            usernameoremail: document.getElementById('usernameoremail').value,
            password: document.getElementById('password').value
        };

        res = await apiRequest('/api/auth/login', "POST", payload);

        if(res && res.token){
            localStorage.setItem("jwt", res.token);
            showResponse({
                message: "Login successfull",
                token: res.token
            })
            refreshAuthUI();
        }
    }
</script>

@endsection