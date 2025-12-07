# OSFRS Micro-UI (Laravel)

This project is a **minimal Laravel frontend** built purely as a developer cockpit for the OSFRS backend.  
It is _not_ a real frontend.  
It’s a collection of small, simple Blade pages that act like a GUI for manual API testing.

Think of it as **Postman on HTML**, wired to your ASP.NET Core backend.

## 🚀 Purpose

- Test every OSFRS endpoint manually
- Validate request payloads
- View raw JSON responses
- Store and reuse JWT tokens
- Provide a clean structure for building micro-UIs per endpoint
- Zero styling, zero complexity, zero JS frameworks

This project is intentionally **lean** — only the pieces of Laravel required to render pages and call APIs through `fetch()`.

## 🧩 Features

- Micro-UI pages for each OSFRS endpoint
- Dedicated Auth pages (Login/Register)
- Automatic JWT storage & injection
- No database, queues, mailers, caches, or Laravel backend logic
- No Vite, no Node, no npm, no migrations
- Pure Blade, pure routes, pure fetch requests

## 🛠 Tech

**Laravel (minimal install)**  
Used only for routing + Blade views.

**Fetch API**  
Used to call your C# backend.

**JWT**  
Stored in `localStorage` and appended automatically for authenticated endpoints.

## 📦 Project Structure (Cleaned)

```
app/
bootstrap/
config/
public/
resources/
    views/
        layout.blade.php
        microui/
            auth/
            facility/
            reservations/
            maintenance/
            profile/
            statistics/
routes/
    web.php
storage/
vendor/

.env
artisan
composer.json
composer.lock
.gitignore
README.md
```

Removed:

- Vite
- node_modules
- package.json
- tests
- phpunit
- default JS/CSS
- unnecessary configs

## 🧭 Getting Started

### 1. Install PHP

```
brew install php
php -v
```

### 2. Install Composer

```
brew install composer
composer -V
```

### 3. Run Laravel

```
php artisan serve
```

App runs at:

```
http://127.0.0.1:8000
```

## 🔐 Authentication Flow

1. Open `/microui/auth/login`
2. Enter email + password
3. On success:
    - JWT is saved to `localStorage`
4. All other pages use the JWT automatically via a shared `api()` function

Once logged in, you can test any protected route in OSFRS.

## 🗂 Endpoints Covered (via micro-pages)

- **Auth**
- **Profile**
- **Facilities**
- **Maintenance**
- **Reservations**
- **Statistics & Analytics**

Each endpoint has its own Blade page with a form and a JSON output panel.

## 🛑 What This Project Is _Not_

- Not a real frontend
- Not styled
- Not using Vue/React
- Not using Laravel controllers/models/services
- Not connected to a database
- Not a replacement for the final Beta UI

This is purely a **developer tool**.

## 📜 License

Internal use for OSFRS project development.  
Not intended for public distribution.
