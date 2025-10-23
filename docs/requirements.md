# OSFRS – Requirements

This document outlines the functional and non-functional requirements for the **Online Sports Facility Reservation System (OSFRS)**. It serves as the basis for development, testing, and project validation.

---

## Table of Contents

1. [Functional Requirements](#functional-requirements)  
   1.1 [User Management](#user-management)  
   1.2 [Reservations](#reservations)  
   1.3 [Facility Management](#facility-management)  
   1.4 [Statistics](#statistics)
2. [Non-Functional Requirements](#non-functional-requirements)
3. [Constraints](#constraints)
4. [References](#references)

---

## Functional Requirements

### User Management

- **Registration:** Users can create accounts with username, name, email, and password.
- **Authentication & Login:** Users must be able to log in securely using sessions or tokens.
- **Profile Management:** Users can view and update personal information.
- **Admin User Management:** Admins can create, update, and delete user accounts, and assign roles.

### Reservations

- **View Availability:** Users can view available time slots for facilities.
- **Create Reservations:** Users can book available slots.
- **Update / Cancel Reservations:** Users can modify or cancel their reservations.
- **Admin Reservation Management:** Admins can update, delete, or override reservations.

### Facility Management

- **Facility Catalog Management:** Admins can add, update, or remove facilities.
- **Availability Tracking:** System maintains real-time availability status of facilities.
- **Maintenance and Status:** Admins can schedule maintenance and update facility status.
- **Admin Facility Control:** Admins can monitor all facilities and reservations.

### Statistics

- **Usage Data Collection:** System collects data on reservations and facility usage.
- **Report Generation:** Admins can generate reports on users, reservations, and facilities.
- **Analytics & Trends:** System provides insights on usage trends and statistics.

---

## Non-Functional Requirements

- **Performance:** System must handle concurrent users efficiently.
- **Security:** Passwords must be hashed; authentication must be secure (token/session).
- **Scalability:** Architecture should support adding new modules and APIs without major changes.
- **Maintainability:** Codebase should follow modular patterns for easy updates.
- **Usability:** User interface should be intuitive and mobile-friendly.
- **Reliability:** System must ensure accurate reservation tracking and data integrity.

---

## Constraints

- **Database:** PostgreSQL is used as the primary database.
- **Backend:** ASP.NET 7 for API and business logic.
- **Frontend:** PHP 8 with HTML/CSS/JavaScript for UI.
- **Deployment:** Designed for local school project deployment initially, but extendable for cloud.

---

## References

- [Modules Overview](modules.md)
- [Submodules Overview](submodules.md)
- [Use Cases](use-cases.md)
- [Architecture Overview](architecture.md)
