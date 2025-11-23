function decodeJwtPayload(token) {
    const base64 = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");

    const json = decodeURIComponent(
        atob(base64)
            .split("")
            .map((c) => "%" + c.charCodeAt(0).toString(16).padStart(2, "0"))
            .join(""),
    );

    return JSON.parse(json);
}

function refreshAuthUI() {
    const token = localStorage.getItem("jwt");
    if (!token) return showLoggedOutUI();

    const payload = decodeJwtPayload(token);
    if (payload.exp * 1000 < Date.now()) {
        console.log("JWT expired, logging out.");
        return logout();
    }

    if (payload && payload.unique_name) {
        document.getElementById("greeter").innerText =
            `Welcome back, ${payload.unique_name}! (${payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]})`;

        document.querySelectorAll(".loggedIn").forEach((el) => {
            el.style.display = "flex";
        });
        document.querySelectorAll(".loggedOut").forEach((el) => {
            el.style.display = "none";
        });
    }
}

function showLoggedOutUI() {
    document.getElementById("greeter").innerText = "";

    document.querySelectorAll(".loggedIn").forEach((el) => {
        el.style.display = "none";
    });
    document.querySelectorAll(".loggedOut").forEach((el) => {
        el.style.display = "flex";
    });
}

async function apiRequest(path, method = "GET", body = null) {
    const token = localStorage.getItem("jwt");

    const headers = {
        "Content-Type": "application/json",
    };

    if (token) {
        headers["Authorization"] = "Bearer " + token;
    }

    try {
        console.log(
            "attempting to fetch from " + "http://localhost:5025" + path,
        );

        const res = await fetch("http://localhost:5025" + path, {
            method,
            headers,
            body: body ? JSON.stringify(body) : body,
        });

        if (res.status == 403) {
            showResponse({ error: "This action requires admin priviledges." });
            return;
        }

        const text = await res.text();

        let data;
        try {
            data = JSON.parse(text);
        } catch {
            data = text;
        }

        showResponse(data);
        return data;
    } catch (err) {
        showResponse({ error: "Request failed", detail: err });
        return null;
    }
}

function showResponse(data) {
    document.getElementById("response").textContent = JSON.stringify(
        data,
        null,
        4,
    );
}

function logout() {
    localStorage.removeItem("jwt");
    showResponse({ message: "Logged out." });
    refreshAuthUI();
}
