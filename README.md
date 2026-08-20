# Pollynx

An ASP.NET Core polling REST API with JWT authentication, role-based access control,
one-vote-per-user enforcement, results with percentages, and voting trends.

## Features

- JWT authentication with rotating refresh tokens and BCrypt password hashing
- Role-based access control (`User` vs `Admin`, 401 vs 403)
- Poll CRUD, public/private, scheduling, and closing
- One vote per user per poll — enforced in service **and** via a DB unique index
- Results with vote counts and percentages plus trend analytics
- FluentValidation + global exception middleware returning a consistent error envelope
- Swagger UI with a working **Authorize** button
- Postman collection and xUnit/Moq unit tests

## Tech stack

- .NET 10 · ASP.NET Core · EF Core 10 · SQL Server (LocalDB)
- JWT Bearer · BCrypt-Net · AutoMapper · FluentValidation · Swashbuckle · xUnit · Moq

## Solution layout

```
Pollynx.sln
├── Pollynx.API            # Controllers, middleware, Swagger/JWT wiring
├── Pollynx.Application    # Services, DTOs, validators, interfaces
├── Pollynx.Domain         # Entities + enums (no dependencies)
├── Pollynx.Infrastructure # EF Core/SQL Server, repositories, JWT, seeder
├── Pollynx.Tests          # xUnit + Moq unit tests
└── docs/
    ├── architecture/architecture.md
    ├── api/API.md
    ├── database/ER-Diagram.png
    └── postman/Pollynx API.postman_collection.json
```

## Getting started

### 1. Prerequisites

- .NET 10 SDK
- SQL Server LocalDB

### 2. Database

```powershell
sqllocaldb start MSSQLLocalDB
dotnet ef database update --project Pollynx.Infrastructure --startup-project Pollynx.API
```

The connection string lives in `Pollynx.API/appsettings.json`
(`Server=(localdb)\MSSQLLocalDB;Database=PollynxDb;Trusted_Connection=True;TrustServerCertificate=True;`).

### 3. Run

```powershell
dotnet run --project Pollynx.API
```

Open Swagger at `https://localhost:7260/swagger`. The app seeds two users on first start:

| Email | Password | Role |
| --- | --- | --- |
| `admin@pollynx.com` | `Admin@123` | Admin |
| `user@pollynx.com` | `User@123` | User |

### 4. Test

```powershell
dotnet test
```

### 5. Import the Postman collection

Import `docs/postman/Pollynx API.postman_collection.json`, create an environment with
`base_url = http://localhost:5177` (or `https://localhost:7260`), and run the folders in order:
**Authentication → Critical Path → RBAC**. Tokens, `pollId` and `pollOptionId` are captured
automatically as environment variables.

## Quick API tour

| Action | Request | Result |
| --- | --- | --- |
| Login | `POST /api/Auth/login` | `200` + tokens |
| Create poll | `POST /api/Polls` (Admin) | `201` |
| Vote | `POST /api/polls/{id}/votes` | `201` |
| Vote again | same request | `409` |
| Results | `GET /api/polls/{id}/results` | `200` percentages |
| User → Admin endpoint | `POST /api/Polls` (User) | `403` |
| No token → protected endpoint | any | `401` |

See [docs/api/API.md](docs/api/API.md) for the full reference and
[docs/architecture/architecture.md](docs/architecture/architecture.md) for the design.

## License

Private evaluation project.