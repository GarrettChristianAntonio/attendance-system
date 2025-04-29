# Attendance System — Facial Recognition

A real-time attendance system that uses facial recognition to automatically identify and check-in employees. The system detects faces through the browser's webcam, extracts biometric descriptors client-side, and matches them against registered employees on the server.

## How It Works

```
Browser (Next.js)                         Server (.NET 9)
┌──────────────────────┐                 ┌──────────────────────┐
│  Camera Feed (WebRTC)│                 │  REST API            │
│  ↓                   │   128-d vector  │  ┌─────────────────┐ │
│  face-api.js         │ ──────────────→ │  │ FaceMatchService │ │
│  - detect face       │   POST /match   │  │ Euclidean dist.  │ │
│  - extract embedding │                 │  └─────────────────┘ │
│  ↓                   │ ←────────────── │         ↓            │
│  Show result         │   { match,      │  PostgreSQL + Files   │
│  "Presente! Juan"    │     name,       │  - employees table   │
└──────────────────────┘     confidence } │  - attendance table  │
                                         │  - photos on disk    │
                                         └──────────────────────┘
```

**Why hybrid?** The browser handles the heavy ML work (face detection + 128-d embedding extraction via face-api.js). The server only performs fast math (Euclidean distance comparison on float arrays). No ML libraries or GPU needed server-side.

## Architecture

The system supports **multi-organization tenancy** through API key authentication. Each organization has isolated data — employees, attendance records, and configurations are scoped per organization.

```
┌─────────────┐     X-Api-Key header     ┌──────────────────┐
│  Frontend    │ ──────────────────────→  │  API Gateway     │
│  (Next.js)   │                          │  ↓               │
└─────────────┘                          │  ApiKeyAuth MW   │
                                         │  ↓               │
┌─────────────┐     X-Api-Key header     │  OrgContext      │
│  3rd Party   │ ──────────────────────→  │  ↓               │
│  Integration │                          │  Controllers     │
└─────────────┘                          │  (org-scoped)    │
                                         └──────────────────┘
```

## Features

- **Real-time face detection** — Continuous webcam scanning (~500ms intervals)
- **Automatic check-in** — Recognized employees are marked present instantly
- **Multi-face handling** — Warns when multiple faces are detected
- **5-minute cooldown** — Prevents duplicate attendance records (client + server)
- **Employee registration** — Camera capture with live face validation
- **Attendance dashboard** — Daily stats, confidence scores, date filtering
- **Responsive UI** — Works on desktop and mobile browsers
- **Multi-organization support** — Isolated data per organization via API keys
- **Rate limiting** — 100 requests/minute per API key
- **Shift scheduling** — Define shifts with grace periods, assign to employees weekly
- **Punctuality tracking** — Automatic on-time/late/absent status detection
- **Analytics dashboard** — Weekly trends, top performers, per-employee details
- **CSV/PDF exports** — Download attendance reports with date range filtering
- **Embeddable widgets** — iframe-ready check-in camera and status board
- **Webhooks** — HMAC-signed event notifications for check-in, check-out, absence
- **Real-time feed** — Server-Sent Events for live check-in streaming
- **Swagger/OpenAPI** — Interactive API documentation at /swagger
- **Docker support** — Dockerfile and docker-compose for containerized deployment
- **Health check** — GET /healthz endpoint for monitoring

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | Next.js 15, React 19, TypeScript, Tailwind CSS v4 |
| **Backend** | .NET 9, C# 13, Entity Framework Core 9 |
| **Database** | PostgreSQL 16 |
| **Face Detection** | [@vladmandic/face-api](https://github.com/vladmandic/face-api) (runs in browser) |
| **ML Models** | SSD MobileNet v1 + FaceLandmark68 + FaceRecognition (~12MB total) |
| **Auth** | API Key authentication (SHA-256 hashed, per-organization) |

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18+)
- [PostgreSQL 16](https://www.postgresql.org/download/) (or use Docker)
- A browser with webcam access (Chrome or Edge recommended)

### Quick Start with Docker

```bash
docker-compose up -d
```

This starts PostgreSQL, the API, and the frontend. Open **http://localhost:3000**.

### Manual Setup

#### 1. Start PostgreSQL

Ensure PostgreSQL is running on `localhost:5432`. Create a database named `attendance`:

```bash
createdb attendance
```

Or use Docker for just the database:

```bash
docker-compose up -d postgres
```

#### 2. Start the API

```bash
cd api/src/AttendanceSystem.API
dotnet run
```

The API starts at **http://localhost:5000**. Database migrations run automatically on startup.

### 3. Create an Organization

```bash
curl -X POST http://localhost:5000/api/organizations/create \
  -H "Content-Type: application/json" \
  -d '{"name": "My Company", "slug": "my-company"}'
```

This returns an API key (shown only once). Save it — you'll need it for all subsequent requests.

### 4. Start the Frontend

```bash
cd web
npm install
npm run dev
```

Set the API key in the frontend by adding it to localStorage:
```javascript
localStorage.setItem('api-key', 'ak_your_key_here');
```

Or set the environment variable `NEXT_PUBLIC_API_KEY` before starting the dev server.

Open **http://localhost:3000** in your browser.

### 5. Register an Employee

1. Navigate to **Employees → + New Employee**
2. Enter name and (optionally) email
3. Click **Start Camera** and face the webcam
4. Click **Capture Photo** — the system validates exactly one face is detected
5. Click **Register Employee**

### 6. Test Check-in

1. Go to the **Camera** page (home)
2. Face the webcam — you should see **"Presente! [Name]"** in green
3. Check the **Attendance** page to see the recorded entry

## API Authentication

All API endpoints (except organization creation) require the `X-Api-Key` header:

```bash
curl -H "X-Api-Key: ak_your_key_here" http://localhost:5000/api/employees
```

Rate limiting is enforced at **100 requests per minute** per API key. Exceeding the limit returns `429 Too Many Requests`.

## API Endpoints

### Organizations & API Keys

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/organizations/create` | Create organization (returns first API key) |
| `GET` | `/api/organizations/current` | Get current organization details |
| `POST` | `/api/apikeys` | Generate new API key for current org |
| `GET` | `/api/apikeys` | List API keys (prefix only, never full key) |
| `DELETE` | `/api/apikeys/{id}` | Revoke an API key |

### Employees

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/employees` | Create employee (multipart: name, email, photo, descriptor) |
| `GET` | `/api/employees` | List all active employees |
| `GET` | `/api/employees/{id}` | Get single employee |
| `PUT` | `/api/employees/{id}` | Update employee |
| `DELETE` | `/api/employees/{id}` | Soft delete (deactivate) |

### Face Matching

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/face/match` | Submit 128-float descriptor → returns best match or "unknown" |

### Attendance

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/attendance` | List attendance records (query: `date`, `employeeId`) |
| `GET` | `/api/attendance/today` | Today's check-ins |

## Face Matching Algorithm

```
Input: float[128] descriptor from browser

1. Load all active employees' stored descriptors from DB (scoped to org)
2. Calculate Euclidean distance to each:
   distance = √(Σ(a[i] - b[i])²) for i = 0..127
3. Find minimum distance
4. If minDistance < 0.55 → MATCH (return employee + confidence)
5. If minDistance ≥ 0.55 → UNKNOWN (no match)

Cooldown: Skip recording if same employee checked in < 5 minutes ago
```

## Project Structure

```
attendance-system/
├── api/                                # .NET 9 Backend
│   ├── AttendanceSystem.sln
│   └── src/AttendanceSystem.API/
│       ├── Controllers/
│       │   ├── EmployeesController.cs  # CRUD + photo upload (org-scoped)
│       │   ├── AttendanceController.cs # Check-in records (org-scoped)
│       │   ├── FaceController.cs       # Face matching endpoint
│       │   ├── OrganizationsController.cs # Org registration
│       │   └── ApiKeysController.cs    # API key management
│       ├── Models/
│       │   ├── Employee.cs             # Name, photo, face descriptor, orgId
│       │   ├── AttendanceRecord.cs     # Check-in timestamp + confidence
│       │   ├── Organization.cs         # Multi-tenant organization
│       │   └── ApiKey.cs               # Hashed API keys
│       ├── Services/
│       │   ├── FaceMatchingService.cs  # Euclidean distance comparison
│       │   ├── ApiKeyService.cs        # Key generation + validation
│       │   └── IOrganizationContext.cs # Scoped org context
│       ├── Middleware/
│       │   └── ApiKeyAuthMiddleware.cs # X-Api-Key validation
│       ├── DTOs/
│       │   ├── OrganizationDtos.cs
│       │   └── ApiKeyDtos.cs
│       ├── Data/
│       │   └── AppDbContext.cs         # EF Core + PostgreSQL
│       └── Program.cs                  # CORS, rate limiting, DI
│
├── web/                                # Next.js 15 Frontend
│   └── src/
│       ├── app/
│       │   ├── page.tsx                # Camera check-in (main page)
│       │   ├── employees/
│       │   │   ├── page.tsx            # Employee list
│       │   │   └── new/page.tsx        # Registration form
│       │   └── attendance/
│       │       └── page.tsx            # Attendance log + stats
│       ├── components/
│       │   ├── CameraFeed.tsx          # Webcam + detection loop
│       │   ├── CheckInResult.tsx       # Match result overlay
│       │   ├── EmployeeForm.tsx        # Registration + camera capture
│       │   ├── EmployeeList.tsx        # Employee table
│       │   └── Navbar.tsx              # Shared navigation
│       └── lib/
│           ├── face-api-setup.ts       # Model loading + face detection
│           └── api.ts                  # API client (includes X-Api-Key)
│
└── README.md
```

## Embedding

The system supports embeddable widgets via JWT tokens. Parent applications can embed check-in cameras and status boards in iframes.

### 1. Create an Embed Token

```bash
curl -X POST http://localhost:5000/api/embed/tokens \
  -H "X-Api-Key: ak_your_key_here" \
  -H "Content-Type: application/json" \
  -d '{"name": "Lobby Kiosk", "scopes": ["embed:checkin", "embed:status"], "expiresInDays": 90}'
```

### 2. Embed in an iframe

```html
<iframe src="http://localhost:3000/embed/check-in?token=YOUR_JWT_TOKEN"
        width="640" height="520" frameborder="0"></iframe>

<iframe src="http://localhost:3000/embed/status?token=YOUR_JWT_TOKEN"
        width="400" height="600" frameborder="0"></iframe>
```

**Available scopes**: `embed:checkin`, `embed:status`, `embed:feed`

Embed tokens are scoped — they can only access widget endpoints, not the full API. Tokens can be revoked at any time via `DELETE /api/embed/tokens/{id}`.

## Key Design Decisions

- **Client-side ML**: Face detection runs in the browser using WebGL — the server never processes images, only compares number arrays. This makes the backend extremely lightweight.
- **PostgreSQL**: Production-grade relational database with full ACID compliance, JSON support, and excellent concurrency handling.
- **128-d descriptors**: Industry-standard face embedding size. Stored as JSON-serialized float arrays in the database.
- **Threshold 0.55**: Tuned for a balance between false positives and false negatives. Lower = stricter matching.
- **Dual cooldown**: Client-side Map (instant feedback) + server-side DB check (prevents duplicates even across page refreshes).
- **API Key Auth**: SHA-256 hashed keys with `ak_` prefix for easy identification. Keys are shown once at creation and never stored in plain text.
- **Multi-org isolation**: All queries are automatically scoped to the authenticated organization via middleware + scoped service pattern.

## License

MIT
