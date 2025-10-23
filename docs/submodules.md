# OSFRS – Submodules Overview

This document provides a **detailed breakdown of all OSFRS submodules** under each core module. Each submodule contains iterative submodules (tasks) that will later include detailed task pages and UML diagrams.

---

## Table of Contents

1. [User Management Submodules](#user-management-submodules)
2. [Reservations Submodules](#reservations-submodules)
3. [Facility Management Submodules](#facility-management-submodules)
4. [Statistics Submodules](#statistics-submodules)
5. [References](#references)

---

## User Management Submodules

Handles all user account and authentication features.

### Iterative Submodules:

1. **User Registration** – Handles new user creation, input validation, and storing data.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/user_management_diagrams/user_registration_workflow_diagram.jpg)

2. **Authentication & Login** – Manages login, session/token generation, and verification.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/user_management_diagrams/auth_and_login_workflow_diagram.jpg)

3. **Profile Management** – Handles updating user information and profile settings.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/user_management_diagrams/profile_management_workflow_diagram.jpg)

4. **User Data Management / CRUD Operations** – Supports full CRUD for user records.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/user_management_diagrams/user_data_mgmt_workflow_diagram.jpg)

---

## Reservations Submodules

Manages bookings and reservation workflows.

### Iterative Submodules:

1. **View Availability** – Display available time slots to users.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/reservations_diagrams/view_availability_workflow_diagram.jpg)

2. **Create Reservations** – Handles new booking creation and validation.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/reservations_diagrams/create_reservations_workflow_diagram.jpg)

3. **Update / Cancel Reservations** – Allows modification or cancellation of existing bookings.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/reservations_diagrams/update_delete_reservation_workflow_diagram.jpg)

4. **Admin Reservation Management** – Admin oversight and management of all reservations.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/reservations_diagrams/admin_reservation_mgmt_workflow_diagram.jpg)

---

## Facility Management Submodules

Administrative management of facilities and operational status.

### Iterative Submodules:

1. **Facility Catalog Management** – CRUD operations for facilities.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/facility_management_diagrams/facility_catalog_management_workflow_diagram.jpg)

2. **Availability Tracking** – Tracks availability and syncs with reservations.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/facility_management_diagrams/availability_tracking_workflow_diagram.jpg)

3. **Maintenance and Status** – Schedule maintenance and update operational status.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/facility_management_diagrams/maintenance_and_status_workflow_diagram.jpg)

4. **Admin Facility Control** – Provides full admin control over all facilities.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/facility_management_diagrams/admin_facility_control_workflow_diagram.jpg)

---

## Statistics Submodules

Generates usage analytics and reports.

### Iterative Submodules:

1. **Usage Data Collection** – Gather all system and reservation usage data.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/statistics_diagrams/usage_data_collection_workflow_diagram.jpg)

2. **Report Generation** – Generate reports and summaries from collected data.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/statistics_diagrams/report_generation_workflow_diagram.jpg)

3. **Analytics & Trends** – Analyze historical data and provide insights.  
   [Details & Tasks](mindmaps_and_diagrams/submodule_diagrams/statistics_diagrams/analytics_and_trends_workflow_diagram.jpg)

---

## References

- [Modules Overview](modules.md)
- [Architecture Overview](architecture.md)
- [Mind Maps & Diagrams](mindmaps_and_diagrams/)
