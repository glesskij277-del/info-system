# Hospital Patient Management System

Portfolio project on **C# / ASP.NET Core MVC + PostgreSQL + Docker**.

## Overview

This project implements a full-featured hospital information system with CRUD operations, validation, scheduling constraints, dashboard analytics, and CSV export.

The application covers key hospital entities:

- Patients
- Doctors
- Appointments
- Medical records
- Departments

## Key Features

- Full CRUD for all core entities
- Validation and business rules at model/controller/database levels
- Search and filtering across registries
- Dashboard with operational metrics
- Reports page with analytics and CSV export
- User-friendly adaptive UI (Bootstrap + custom styling)
- Dockerized runtime (web + database)

## Business Rules

- Unique patient OMS policy number
- Unique patient SNILS
- Unique medical record number
- One medical record per patient
- Scheduling conflict protection for doctor/patient at the same appointment time
- Prevent deleting doctors/patients with related appointments
- Input normalization and friendly validation messages

## Tech Stack

- .NET 8 (ASP.NET Core MVC)
- Entity Framework Core + Npgsql
- PostgreSQL 16
- Docker / Docker Compose
- Bootstrap 5 + custom CSS

## Project Structure

- `HospitalIS.Web/Models` - domain models and validation
- `HospitalIS.Web/Controllers` - app logic and CRUD flows
- `HospitalIS.Web/Data` - DbContext and seed initializer
- `HospitalIS.Web/Infrastructure` - input normalization and DB error helpers
- `HospitalIS.Web/ViewModels` - dashboard/report view models
- `HospitalIS.Web/Views` - Razor UI
- `sql/schema.sql` - PostgreSQL schema
- `docker-compose.yml` - local orchestration
- `scripts/smoke.sh` - smoke checks for core routes

## Quick Start (Docker)

From the project root:

```bash
docker compose up -d --build
```

Open:

- http://localhost:8080

Health check:

```bash
curl -fsS http://127.0.0.1:8080/health
```

Smoke test:

```bash
./scripts/smoke.sh
```

Stop services:

```bash
docker compose down
```

Clean all data volumes:

```bash
docker compose down -v
```

## Local Run (without Docker)

Requirements:

- .NET 8 SDK
- PostgreSQL

Run:

```bash
cd HospitalIS.Web
dotnet restore
dotnet run
```

Make sure the connection string is configured in `appsettings.json`.

## Sample Data

On first startup, seed data is created automatically to simplify demo/testing:

- Departments
- Doctors
- Patient
- Medical record
- Appointment

## Portfolio Notes

This repository is intentionally structured as a showcase project:

- clear domain modeling
- business-oriented constraints
- practical filtering/reporting
- production-like Docker setup for quick demonstration

## License

Add your preferred license (MIT/Apache-2.0/etc.) before public release.

