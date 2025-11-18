# OSFRS API Specification – Alpha v1

This document defines the REST API for **OSFRS (Online Sports Facility Reservation System)**
**Backend:** ASP.NET Core, **Style:** JSON over HTTP, **Version:** Alpha v1

---

## 1. General

### 1.1 Base URL

All endpoints are prefixed with:

```
/api/
```

### 1.2 Authentication

- Auth type: **JWT Bearer**
- Header:

```
Authorization: Bearer <token>
```

- Roles:
  - `User`
  - `Admin`

### 1.3 Date / Time

- All date/time values use **ISO 8601** strings.
- Timestamps are stored internally as UTC.

### 1.4 Error Format (Recommended)

```json
{
  "message": "Human readable message"
}
```

---

## 2. Auth & Profile

### 2.1 Register

**POST** `/api/auth/register`
**Auth:** No

Body: `UserRegistrationDto`

### 2.2 Login

**POST** `/api/auth/login`
**Auth:** No

Returns only:

```json
{
  "token": "jwt-token-string"
}
```

### 2.3 Get Profile

**GET** `/api/profile`

### 2.4 Update Profile

**PUT** `/api/profile`

---

## 3. Reservations

### 3.1 Get Availability

**GET** `/api/reservations/availability/{facilityId}`

### 3.2 Get Reservations by Facility

**GET** `/api/reservations/facility/{facilityId}`

### 3.3 Search Reservations

**GET** `/api/reservations/search`

### 3.4 Create Reservation

**POST** `/api/reservations/create`

### 3.5 Update Reservation

**PUT** `/api/reservations/update/{id}`

### 3.6 Cancel Reservation

**PUT** `/api/reservations/cancel/{id}`

### 3.7 Get My Reservations

**GET** `/api/reservations/my`

### 3.8 Admin – Get All Reservations

**GET** `/api/reservations/admin/all`

### 3.9 Admin – Update Reservation

**PUT** `/api/reservations/admin/update/{id}`

### 3.10 Admin – Delete Reservation

**DELETE** `/api/reservations/admin/delete/{id}`

---

## 4. Facilities

### 4.1 Get All Facilities

**GET** `/api/facility`

### 4.2 Get Facility by Id

**GET** `/api/facility/{id}`

### 4.3 Create Facility

**POST** `/api/facility`

### 4.4 Update Facility

**PUT** `/api/facility/{id}`

### 4.5 Delete Facility

**DELETE** `/api/facility/{id}`

### 4.6 Get Facility Availability

**GET** `/api/facility/{id}/availability`

### 4.7 Update Facility Availability

**PATCH** `/api/facility/{id}/availability`

---

## 5. Maintenance

### 5.1 Schedule Maintenance

**POST** `/api/maintenance`

### 5.2 Update Maintenance

**PUT** `/api/maintenance/{id}`

### 5.3 Delete Maintenance

**DELETE** `/api/maintenance/{id}`

### 5.4 Get Maintenance Records for Facility

**GET** `/api/maintenance/facility/{facilityId}`

### 5.5 Get Upcoming Maintenance

**GET** `/api/maintenance/upcoming`

### 5.6 Sync Statuses

**POST** `/api/maintenance/sync-statuses`

---

## 6. Statistics & Usage

### 6.1 Get Events

**GET** `/api/statistics/events`

### 6.2 Get Daily Aggregates

**GET** `/api/statistics/aggregate/daily`

### 6.3 Get Monthly Aggregates

**GET** `/api/statistics/aggregate/monthly`

### 6.4 Run Aggregation

**POST** `/api/statistics/aggregate/run`

### 6.5 Get Daily Report

**GET** `/api/statistics/reports/daily`

### 6.6 Get Monthly Report

**GET** `/api/statistics/reports/monthly`

### 6.7 Export CSV

**GET** `/api/statistics/export/csv`

### 6.8 Export PDF

**GET** `/api/statistics/export/pdf`

---

## 7. Analytics

### 7.1 Daily Trends

**GET** `/api/statistics/analytics/trends/daily`

### 7.2 Monthly Trends

**GET** `/api/statistics/analytics/trends/monthly`

### 7.3 Peak Usage

**GET** `/api/statistics/analytics/peaks`

### 7.4 Detect Anomalies

**GET** `/api/statistics/analytics/anomalies`

### 7.5 Visualization Data

**GET** `/api/statistics/analytics/visualization`

**End of OSFRS API Specification – Alpha v1**
