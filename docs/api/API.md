# Pollynx — API Reference

Base URL (local development): `https://localhost:7260` or `http://localhost:5177`.
All endpoints return JSON. Success/error responses use the documented status codes below.

## Authentication

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| POST | `/api/Auth/register` | Public | Create a user account |
| POST | `/api/Auth/login` | Public | Obtain Access + Refresh tokens |
| POST | `/api/Auth/refresh-token` | Public | Rotate a refresh token into a new token pair |

### Register

```json
POST /api/Auth/register
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "Password@123"
}
```

- `201` on success.
- `409` if the email already exists.
- `400` on validation failure.

### Login

```json
POST /api/Auth/login
{
  "email": "admin@pollynx.com",
  "password": "Admin@123"
}
```

`200` →

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-64-random-bytes",
  "userId": 1008,
  "fullName": "System Admin",
  "email": "admin@pollynx.com",
  "role": "Admin"
}
```

`401` if credentials are invalid.

### Refresh token

```json
POST /api/Auth/refresh-token
{
  "refreshToken": "the-token-from-login"
}
```

- `200` returns a fresh access/refresh pair and **revokes** the old refresh token.
- Reusing an old refresh token returns `401` (rotation defense).

## Polls

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| GET | `/api/Polls` | Public | List all polls |
| GET | `/api/Polls/active` | Public | List currently active polls |
| GET | `/api/Polls/{id}` | Public | Get one poll with its options |
| POST | `/api/Polls` | **Admin** | Create a poll |
| PUT | `/api/Polls/{id}` | **Admin** | Update a poll |
| DELETE | `/api/Polls/{id}` | **Admin** | Delete a poll (no votes) |
| POST | `/api/Polls/{id}/close` | **Admin** | Close a poll |

### Create poll

```json
POST /api/Polls
{
  "title": "Favorite programming language",
  "description": "Pick one",
  "startTime": "2026-08-01T00:00:00Z",
  "endTime": "2027-12-31T00:00:00Z",
  "isPublic": true,
  "options": ["C#", "JavaScript", "Python"]
}
```

- `201` returns the created poll including option ids.
- `400` if start >= end, fewer than two unique non-blank options.
- `401` no/invalid token, `403` non-admin role.

Rule: `startTime` / `endTime` are normalized to UTC before storage.

## Votes

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| POST | `/api/polls/{pollId}/votes` | Authenticated | Cast one vote |

```json
POST /api/polls/{pollId}/votes
{
  "pollOptionId": 7
}
```

- `201` vote recorded.
- `409` if the user already voted, the poll is closed, or it has not started yet.
- `404` poll not found; `400` option does not belong to the poll.

Each user may vote **once** per poll (enforced in service + DB unique index).

## Results & Analytics

| Method | Endpoint | Access | Description |
| --- | --- | --- | --- |
| GET | `/api/polls/{pollId}/results` | Public | Votes and percentages per option |
| GET | `/api/polls/{pollId}/analytics` | Public | Per-period voting trends |

`GET /api/polls/{id}/results` →

```json
{
  "pollId": 1008,
  "title": "Favorite programming language",
  "totalVotes": 4,
  "options": [
    { "optionId": 7, "optionText": "C#", "voteCount": 3, "percentage": 75.0 },
    { "optionId": 8, "optionText": "JavaScript", "voteCount": 1, "percentage": 25.0 }
  ]
}
```

`GET /api/polls/{id}/analytics` → vote counts grouped by time window.

## Error envelope

All errors use the same shape:

```json
{
  "code": "BUSINESS_RULE_VIOLATION",
  "message": "You have already voted in this poll.",
  "traceId": "0HNNUFUMT8CS0:00000004",
  "timestamp": "2026-08-20T05:57:41.0662253Z"
}
```

## Status code summary

`200` OK · `201` Created · `204` No Content · `400` Bad Request ·
`401` Unauthenticated · `403` Forbidden (wrong role) · `404` Not Found · `409` Conflict

## Seed users (development)

| Email | Password | Role |
| --- | --- | --- |
| `admin@pollynx.com` | `Admin@123` | Admin |
| `user@pollynx.com` | `User@123` | User |