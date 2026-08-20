# Pollynx — Architecture

## Overview

Pollynx is a polling REST API built with the ASP.NET Core 10 (clean layered) architecture.
Each layer has a single responsibility and depends only on the layers below it.

```
          ┌─────────────────────────────────────────┐
          │           Pollynx.API                   │
          │  Controllers · Middleware · Swagger     │
          └────────────────┬────────────────────────┘
                           │
          ┌────────────────▼────────────────────────┐
          │         Pollynx.Application             │
          │  Services · DTOs · Validators · I/Faces │
          └────────────────┬────────────────────────┘
                           │
          ┌────────────────▼────────────────────────┐
          │           Pollynx.Domain                │
          │  Entities · Enums (no dependencies)     │
          └────────────────┬────────────────────────┘
                           │
          ┌────────────────▼────────────────────────┐
          │        Pollynx.Infrastructure           │
          │  EF Core / SQL Server · Repositories    │
          │  JWT · Seeding                          │
          └─────────────────────────────────────────┘
                           │
                    SQL Server LocalDB
```

## Projects

| Project | Responsibility |
| --- | --- |
| `Pollynx.API` | HTTP layer: controllers, middleware, JWT/Swagger wiring. Controllers stay thin. |
| `Pollynx.Application` | Business rules, use cases, DTOs, FluentValidation validators, service/interface definitions. |
| `Pollynx.Domain` | Pure entities (`User`, `Poll`, `PollOption`, `Vote`, `RefreshToken`) and enums. No package dependencies. |
| `Pollynx.Infrastructure` | EF Core + SQL Server, repositories, `JwtService`, `DbSeeder`. |
| `Pollynx.Tests` | xUnit + Moq unit tests for the core business rules. |

## Dependency rules

- `API` references `Application` and `Infrastructure`.
- `Application` references `Domain`. Application NEVER references API or Infrastructure.
- `Infrastructure` references `Application` and `Domain`.
- `Domain` references nothing. It stays framework-free.

These rules keep the domain portable and the business logic testable in isolation.

## Request flow

```
HTTP request
   → ExceptionHandlingMiddleware (uniform error envelope)
   → Routing → Controller
   → Service (business rules, DTO mapping)
   → Repository (EF Core)
   → SQL Server
   → Response mapped back through AutoMapper → JSON
```

- Authentication: JWT bearer tokens (issued by `AuthService` / `JwtService`).
- Authorization: role claims enforced with `[Authorize(Roles="Admin")]`; a *User* hitting an admin endpoint gets **403**.
- Duplicate voting is prevented twice:
  1. `VoteService` checks `HasUserVotedAsync` before inserting.
  2. `Vote` has a unique composite index on `(UserId, PollId)` at the database level.
- Polls store times as UTC; `PollService` normalizes request times before persisting so window
  comparisons (`not started` / `ended`) are always done against `DateTime.UtcNow`.

## Why these layers?

- **API layer** – handles HTTP concerns only; keeps controllers thin and swappable.
- **Application layer** – contains the business rules (poll lifecycle, one-vote-per-user,
  refresh-token rotation, validations), DTOs and mappings; platforms can reuse it.
- **Domain layer** – the heart of the model, with zero external dependencies.
- **Infrastructure layer** – isolates EF Core / SQL Server / JWT details behind interfaces
  defined in Application, so business logic never depends on a concrete database.

## Mapping

AutoMapper (`MappingProfile`) converts Domain entities ↔ DTOs, so internal entities are never
exposed directly over HTTP.

## Error handling

`ExceptionHandlingMiddleware` (`Pollynx.API/Middleware`) converts exceptions into a consistent
`{ code, message, traceId, timestamp }` envelope:

| Exception | HTTP |
| --- | --- |
| `UnauthorizedAccessException` | 401 |
| `KeyNotFoundException` | 404 |
| `InvalidOperationException` | 409 |
| `ArgumentException` / model validation | 400 |
| any unhandled | 500 |