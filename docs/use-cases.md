# OSFRS – Use Cases

This document outlines the main **use cases** for OSFRS, describing typical interactions between users, administrators, and the system. Each use case highlights the actors, preconditions, main flow, and expected outcomes.

---

## Table of Contents

1. [User Management Use Cases](#user-management-use-cases)
2. [Reservations Use Cases](#reservations-use-cases)
3. [Facility Management Use Cases](#facility-management-use-cases)
4. [Statistics Use Cases](#statistics-use-cases)
5. [References](#references)

---

## User Management Use Cases

### 1. User Registration

**Actor:** User  
**Preconditions:** No existing account  
**Main Flow:**

1. User navigates to registration page.
2. Inputs username, name, email, and password.
3. System validates data and stores in database.
4. Confirmation sent to user.  
   **Postconditions:** New user account created and ready for login.

### 2. Authentication & Login

**Actor:** User  
**Preconditions:** User has registered  
**Main Flow:**

1. User submits login credentials.
2. System verifies credentials (token/session generated).
3. User gains access to the system.  
   **Postconditions:** User session started; roles enforced.

### 3. Profile Management

**Actor:** User  
**Preconditions:** User logged in  
**Main Flow:**

1. User accesses profile page.
2. Updates personal information.
3. System validates and saves updates.  
   **Postconditions:** User profile updated.

### 4. User Data Management / CRUD

**Actor:** Admin  
**Preconditions:** Admin logged in  
**Main Flow:**

1. Admin views user list.
2. Creates, updates, or deletes user accounts.
3. System logs actions and updates database.  
   **Postconditions:** User data maintained accurately.

---

## Reservations Use Cases

### 1. View Availability

**Actor:** User  
**Preconditions:** User logged in  
**Main Flow:**

1. User requests calendar view.
2. System displays available time slots.  
   **Postconditions:** User sees accurate availability.

### 2. Create Reservations

**Actor:** User  
**Preconditions:** User logged in  
**Main Flow:**

1. User selects available slot.
2. System validates and saves reservation.
3. Confirmation sent to user.  
   **Postconditions:** Reservation confirmed.

### 3. Update / Cancel Reservations

**Actor:** User  
**Preconditions:** User has existing reservation  
**Main Flow:**

1. User selects reservation to modify/cancel.
2. System updates or deletes the reservation.  
   **Postconditions:** Reservation updated/canceled and availability refreshed.

### 4. Admin Reservation Management

**Actor:** Admin  
**Preconditions:** Admin logged in  
**Main Flow:**

1. Admin views all reservations.
2. Updates or deletes reservations if needed.
3. System logs actions.  
   **Postconditions:** Reservations properly managed.

---

## Facility Management Use Cases

### 1. Facility Catalog Management

**Actor:** Admin  
**Main Flow:** Add, update, or delete facilities.  
**Postconditions:** Catalog is up-to-date.

### 2. Availability Tracking

**Actor:** System  
**Main Flow:** Syncs facility availability with reservations.  
**Postconditions:** Accurate availability displayed to users.

### 3. Maintenance and Status

**Actor:** Admin  
**Main Flow:** Schedule maintenance; mark facility status.  
**Postconditions:** Facility marked unavailable during maintenance.

### 4. Admin Facility Control

**Actor:** Admin  
**Main Flow:** Full oversight of all facilities.  
**Postconditions:** Admin manages operations efficiently.

---

## Statistics Use Cases

### 1. Usage Data Collection

**Actor:** System  
**Main Flow:** Collects usage and reservation data continuously.  
**Postconditions:** Data ready for reporting.

### 2. Report Generation

**Actor:** Admin  
**Main Flow:** Generates reports on reservations, users, and facilities.  
**Postconditions:** Reports available for review.

### 3. Analytics & Trends

**Actor:** Admin  
**Main Flow:** System analyzes historical data to generate insights.  
**Postconditions:** Trends and analytics accessible for decision-making.

---

## References

- [Modules Overview](modules.md)
- [Submodules Overview](submodules.md)
- [Architecture Overview](architecture.md)
