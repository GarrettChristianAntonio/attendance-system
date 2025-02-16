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
│  Show result         │   { match,      │  SQLite + Files      │
│  "Presente! Juan"    │     name,       │  - employees table   │
└──────────────────────┘     confidence } │  - attendance table  │
                                         │  - photos on disk    │
                                         └──────────────────────┘
```

**Why hybrid?** The browser handles the heavy ML work (face detection + 128-d embedding extraction via face-api.js). The server only performs fast math (Euclidean distance comparison on float arrays). No ML libraries or GPU needed server-side.

## Features

- **Real-time face detection** — Continuous webcam scanning (~500ms intervals)
- **Automatic check-in** — Recognized employees are marked present instantly
- **Multi-face handling** — Warns when multiple faces are detected
- **5-minute cooldown** — Prevents duplicate attendance records (client + server)
- **Employee registration** — Camera capture with live face validation
- **Attendance dashboard** — Daily stats, confidence scores, date filtering
- **Responsive UI** — Works on desktop and mobile browsers

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | Next.js 15, React 19, TypeScript, Tailwind CSS v4 |
| **Backend** | .NET 9, C# 13, Entity Framework Core 9 |
| **Database** | SQLite (zero-config, file-based) |
| **Face Detection** | [@vladmandic/face-api](https://github.com/vladmandic/face-api) (runs in browser) |
| **ML Models** | SSD MobileNet v1 + FaceLandmark68 + FaceRecognition (~12MB total) |

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18+)
- A browser with webcam access (Chrome or Edge recommended)

### 1. Start the API

```bash
cd api/src/AttendanceSystem.API
dotnet run
```

The API starts at **http://localhost:5000**. SQLite database is created automatically on first run.

### 2. Start the Frontend

```bash
cd web
npm install
npm run dev
```

Open **http://localhost:3000** in your browser.

### 3. Register an Employee

1. Navigate to **Employees → + New Employee**
2. Enter name and (optionally) email
3. Click **Start Camera** and face the webcam
4. Click **Capture Photo** — the system validates exactly one face is detected
5. Click **Register Employee**

### 4. Test Check-in

1. Go to the **Camera** page (home)
2. Face the webcam — you should see **"Presente! [Name]"** in green
3. Check the **Attendance** page to see the recorded entry

## Project Structure

```
attendance-system/
├── api/                                # .NET 9 Backend
│   ├── AttendanceSystem.sln
│   └── src/AttendanceSystem.API/
│       ├── Controllers/
│       │   ├── EmployeesController.cs  # CRUD + photo upload
│       │   ├── AttendanceController.cs # Check-in records
│       │   └── FaceController.cs       # Face matching endpoint
│       ├── Models/
│       │   ├── Employee.cs             # Name, photo, face descriptor
│       │   └── AttendanceRecord.cs     # Check-in timestamp + confidence
│       ├── Services/
│       │   └── FaceMatchingService.cs  # Euclidean distance comparison
│       ├── Data/
│       │   └── AppDbContext.cs         # EF Core + SQLite
│       └── Program.cs                  # CORS, static files, DI
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
│           └── api.ts                  # API client
│
└── README.md
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/employees` | Create employee (multipart: name, email, photo, descriptor) |
| `GET` | `/api/employees` | List all active employees |
| `GET` | `/api/employees/{id}` | Get single employee |
| `PUT` | `/api/employees/{id}` | Update employee |
| `DELETE` | `/api/employees/{id}` | Soft delete (deactivate) |
| `POST` | `/api/face/match` | Submit 128-float descriptor → returns best match or "unknown" |
| `GET` | `/api/attendance` | List attendance records (query: `date`, `employeeId`) |
| `GET` | `/api/attendance/today` | Today's check-ins |

## Face Matching Algorithm

```
Input: float[128] descriptor from browser

1. Load all active employees' stored descriptors from DB
2. Calculate Euclidean distance to each:
   distance = √(Σ(a[i] - b[i])²) for i = 0..127
3. Find minimum distance
4. If minDistance < 0.55 → MATCH (return employee + confidence)
5. If minDistance ≥ 0.55 → UNKNOWN (no match)

Cooldown: Skip recording if same employee checked in < 5 minutes ago
```

## Key Design Decisions

- **Client-side ML**: Face detection runs in the browser using WebGL — the server never processes images, only compares number arrays. This makes the backend extremely lightweight.
- **SQLite**: Zero-config database perfect for single-server deployments. No PostgreSQL/MySQL setup needed.
- **128-d descriptors**: Industry-standard face embedding size. Stored as JSON-serialized float arrays in the database.
- **Threshold 0.55**: Tuned for a balance between false positives and false negatives. Lower = stricter matching.
- **Dual cooldown**: Client-side Map (instant feedback) + server-side DB check (prevents duplicates even across page refreshes).

## License

MIT
