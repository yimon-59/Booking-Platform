# Setup Instructions

Follow the steps below to clone, configure, and run the Rezerv Booking Engine locally.

## 1. Prerequisites

Install:
- Git
- .NET 9 SDK
- MySQL 8+
- Redis
- Visual Studio 2022 

Redis should be available on :
localhost:6379

## 2. Clone the Repository

Clone the GitHub repository :
Open terminal and type the following command and press Enter :
git clone <REPOSITORY_URL>

## 3. Restore NuGet Packages

Navigate to the directory containing project file and run:
dotnet restore

## 4. Configure MySQL

Create the development database:
Open MySql workbench and run below script - 

'CREATE DATABASE RezervBooking;'

Open:
Rezerv.Api/appsettings.json
Configure:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=RezervBooking;User=root;Password=YOUR_PASSWORD;"
  }
}
Replace `YOUR_PASSWORD` with your local MySQL password.

## 5. Start Redis

The application expects Redis at:
localhost:6379
If using Docker:
From powershell :
docker run --name rezerv-redis -p 6379:6379 -d redis

## 6. Configure Redis
In:
Rezerv.Api/appsettings.json
use:
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}

## 7. Apply Database Migrations

From the project repository root:
From powershell or terminal -
dotnet ef migrations add InitialCreate --project Rezerv.Infrastructure --startup-project Rezerv.Api
dotnet ef database update --project Rezerv.Infrastructure --startup-project Rezerv.Api

This creates the database tables.

## 8. Seed Development Data

When the API starts, the database seeder populates the development database with the sample users, businesses, packages, schedules, bookings, and waitlist entries.
The seeder does not run again when users already exist.

## 9. Build the Solution

From powershell -
dotnet build

## 10. Run the API
From powershell -
dotnet run --project Rezerv.Api

After the application starts, open Swagger using the URL displayed by ASP.NET Core.

Example:
https://localhost:<port>/swagger/index.html
Swagger provides interactive documentation and allows the API endpoints to be tested directly.

Keep the terminal running.

## 11. Test the API
Use the seeded users, packages, and timetable schedules to test the booking scenarios.

## 12. Create the Test Database
CREATE DATABASE RezervBooking_test;
Open:
Rezerv.Tests/TestDbContextFactory.cs
Update:
private const string ConnectionString =
    "Server=localhost;Port=3306;Database=RezervBooking_test;User=root;Password=YOUR_PASSWORD;";

Replace 'YOUR_PASSWORD' with your local MySQL password.

## 13. Run All Tests
From powershell -
dotnet test Rezerv.Tests/Rezerv.Tests.csproj

---

# API Endpoints

GET /api/packages
Return all available packages

GET /api/userpackages
Returns packages purchased by the user.

POST /api/packages/purchase
Purchases a package for a user.

GET /api/timetable
Supports timetable filtering by business and date .

POST /api/bookings
Creates a confirmed booking.

POST /api/bookings/cancel
Cancels a confirmed booking and applies the refund rules.

POST /api/waitlist
Allows a customer to join a full timetable schedule.
The waitlist follows FIFO ordering.

---

# ERD

[Entity Relationship Diagram](docs/ERD.png)

---
# Architecture

The project follows a Clean Architecture-style structure.

Rezerv.Booking.sln
├── Rezerv.Api/
├── Rezerv.Application/
├── Rezerv.Domain/
└── Rezerv.Infrastructure/
└── Rezerv.Tests/


## Project Responsibilities

### Rezerv.Domain

Contains the core domain model:

- User
- Business
- Package
- UserPackage
- TimetableSchedule
- Booking
- WaitlistEntry

It also contains domain enums such as:

- BookingStatus
- WaitlistStatus

The domain layer does not depend on infrastructure or API concerns.

### Rezerv.Application

Contains application business logic and abstractions.

Examples:

- BookingService
- PackageService
- WaitlistService
- DTOs
- Application interfaces

Important abstractions include:

- IApplicationDbContext
- IDistributedLockService

### Rezerv.Infrastructure

Contains implementation details such as:

- Entity Framework Core
- MySQL database access
- Redis distributed locking
- DbContext
- Database configuration
- Database seeding

### Rezerv.Api

Contains:

- API controllers
- Dependency injection configuration
- API configuration
- Swagger configuration

---

# Booking Rules

A package:

- Must belong to the same Business as the timetable schedule.
- Must not be expired.
- Must have at least one remaining credit.

A successful booking deducts one credit immediately.

A user cannot have overlapping confirmed bookings.

A schedule cannot exceed its available slot count.

```text
ConfirmedBookings >= AvailableSlots
```

results in:

```text
Schedule is full.
```

---

# Cancellation Rules

If a booking is cancelled more than four hours before the class starts:

```text
1 credit is refunded
```

If cancelled within four hours:

```text
No credit refund
```

The booking status changes from `Confirmed` to `Cancelled`.

When a confirmed booking is cancelled, the first eligible waiting customer can be automatically promoted and one credit is deducted.

---

# Waitlist

When a schedule is full, customers can join the waitlist.

Waitlist ordering is FIFO based on:

```text
JoinedAt
```

Joining the waitlist does not immediately deduct a credit.

When a booking is cancelled:

1. The earliest waiting customer is selected.
2. Their package is validated.
3. One credit is deducted.
4. A confirmed booking is created.
5. The waitlist entry becomes `Promoted`.

Waiting entries remaining after the class ends can be marked `Expired`.

---

# Background Processing

Hangfire is used to periodically process waitlist expiration.

A recurring job runs every minute and calls the waitlist service to
find waiting entries whose timetable schedules have ended. These
entries are marked as `Expired`.

All time comparisons use UTC.

---

# Concurrency Strategy
The system uses two levels of protection:

Redis Distributed Lock
        +
MySQL Transaction + FOR UPDATE

## Redis Distributed Lock

A Redis lock is acquired using the timetable schedule ID:
booking:schedule:{scheduleId}

This prevents multiple application instances from processing the same schedule simultaneously.
The lock has a short expiration time to avoid permanently holding a lock if an application instance fails.

## MySQL Row-Level Lock

Inside the database transaction, the schedule is locked using:
"SELECT *
FROM TimetableSchedules
WHERE Id = ?
FOR UPDATE"

The User and UserPackage rows are also locked when required.
This provides database-level protection against concurrent updates.

## Why Both Redis and MySQL?

Redis provides a distributed application-level lock.

MySQL row-level locking provides database-level consistency.

Using both provides protection at different layers:

Redis
=> prevents concurrent application processing

MySQL
=> guarantees transactional database consistency

## Tradeoffs

The Redis lock introduces an external dependency and additional operational complexity.

MySQL row locking can reduce concurrency when many requests target the same schedule because requests must wait for the transaction holding the row lock.
The current implementation favors correctness and preventing overbooking over maximizing throughput.
For a larger system, the locking strategy could be further optimized depending on traffic patterns and deployment architecture.

---


# Assumptions

1. Available packages are defined by expired date.
2. Package expiry is defined by the Package and applies to users purchasing that package.
3. One successful booking consumes exactly one credit.
4. Cancelled bookings do not count toward available slots.
5. Only confirmed bookings are considered when checking schedule capacity.
6. Overlapping checks apply to confirmed bookings belonging to the same user.
7. Waitlist entries are ordered by JoinedAt.
8. Credits are deducted when a waitlisted customer is promoted, not when joining the waitlist.
9. A waitlisted customer must have a valid package with sufficient credits when promotion occurs.
10. Waitlist promotion follows FIFO order. If the first user does not have a valid package, sufficient credits, or a matching business, the system skips them and checks the next user.
11. UTC is used for persisted DateTime values.
---

# Future Improvements

Potential improvements for a production-scale implementation include:

- Redis integration tests
- More comprehensive API integration tests
- Structured error response models
- Authentication and authorization
- More granular database/index optimization
- More validation for api request
- Add proper log

---
