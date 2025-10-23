# OSFRS – Architecture Overview

This document provides a **high-level overview of the OSFRS system architecture**, including its modules, submodules, and internal interactions. It is designed as a reference for developers, project stakeholders, and anyone looking to understand the structure of the system.

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Modules Overview](#modules-overview)
3. [Backend Architecture](#backend-architecture)
4. [Frontend Overview](#frontend-overview)
5. [Database Architecture](#database-architecture)
6. [Diagrams & Mind Maps](#diagrams--mind-maps)
7. [References](#references)

---

## System Overview

OSFRS (Online Sports Facility Reservation System) is built to efficiently manage users, bookings, and sports facilities.  
Key points:

- **Headless backend:** modular and scalable
- **Frontend:** PHP/HTML/CSS/JS rendering user interfaces
- **Backend:** ASP.NET 7 providing API endpoints and business logic
- **Database:** PostgreSQL storing all persistent data

---

## Modules Overview

### Base Modules

1. **User Management** – Registration, login, profiles, roles
2. **Reservations** – Booking, calendar, time blocking
3. **Facility Management** – Admin operations, facility availability
4. **Statistics** – Reports, analytics, usage tracking

### Future Modules

- Notifications – Email/SMS/in-app confirmations
- Map / Geolocation – Show facility locations
- Admin Dashboard – System overview and pending tasks

For details, see: `modules.md` and `submodules.md`

---

## Backend Architecture

The backend is responsible for:

- Processing all **business logic**
- Exposing **API endpoints** consumed by the frontend
- Managing **CRUD operations** for users, reservations, and facilities

Major components:

- **User Management APIs**
- **Reservations APIs**
- **Facility Management APIs**
- **Statistics APIs**

The backend is fully modular to allow future expansion without impacting existing functionality.

---

## Frontend Overview

- **PHP 8**: Renders UI pages
- **HTML/CSS/JS**: Structure, styling, and interactivity
- Interacts with backend through **RESTful APIs**

---

## Database Architecture

- **PostgreSQL 16**: Main relational database
- Tables correspond to modules and submodules: Users, Reservations, Facilities, Statistics
- Relationships enforce consistency and support reporting/analytics

---

## Diagrams & Mind Maps

### General Architecture

- [Backend Core Map](mindmaps_and_diagrams/backend_core_map.jpg)
- [Modules Diagram](mindmaps_and_diagrams/modules_diagram.png)
- [Tech Stack Diagram](mindmaps_and_diagrams/tech_stack_diagram.png)

### Submodule Diagrams

- **User Management:** `mindmaps_and_diagrams/submodule_diagrams/user_management_diagrams/`
- **Reservations:** `mindmaps_and_diagrams/submodule_diagrams/reservations_diagrams/`
- **Facility Management:** `mindmaps_and_diagrams/submodule_diagrams/facility_management_diagrams/`
- **Statistics:** `mindmaps_and_diagrams/submodule_diagrams/statistics_diagrams/`

Includes workflow diagrams, internal interactions, and example sequences.

---

## References

- [Assignment & Requirements](assignment.md)
- [Tech Stack](tech_stack_diagram.png)
- [Modules & Submodules](modules.md, submodules.md)
- [Use Cases](use-cases.md)
- [Roadmap](roadmap.md)
- [Database Design](requirements.md)
- [Mind Maps & Diagrams](mindmaps_and_diagrams/)
