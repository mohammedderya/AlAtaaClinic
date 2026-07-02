# Al Ataa Clinic Management System

Offline Windows desktop clinic management system for **جمعية العطاء الخيرية الطبية** (Al Ataa Medical Charity).

## Architecture

Clean Architecture solution with four projects:

| Project | Purpose |
|---------|---------|
| `AlAtaaClinic.Domain` | Entities, enums, aggregate roots |
| `AlAtaaClinic.Application` | Business services, DTOs, validation, authorization |
| `AlAtaaClinic.Infrastructure` | EF Core persistence, repositories, security, audit logging |
| `AlAtaaClinic.Desktop` | WPF MVVM desktop UI |

Each clinic runs a local **SQL Server Express** database. The application works fully offline after installation.

## Features

- Patient registration and medical history
- Appointments and clinical visits (vitals, diagnoses, prescriptions)
- Billing, payments, and charity cases
- Medicine inventory
- Branch, department, staff, and doctor management
- Role-based access control with 18 granular permissions
- Audit trail for authentication and patient changes
- Arabic/English UI with light/dark themes
- License management with user limits

## Requirements

- Windows 10/11 (64-bit)
- .NET 8 Desktop Runtime
- SQL Server Express (local instance recommended: `.\SQLEXPRESS`)

## Build

```powershell
dotnet build AlAtaaClinic.sln -c Release
```

## Publish

```powershell
dotnet publish src/AlAtaaClinic.Desktop/AlAtaaClinic.Desktop.csproj -c Release -r win-x64 -o publish/Release
```

## Installer

Build the publish output first, then compile `installer/AlAtaaClinic.iss` with Inno Setup 6.

See [docs/INSTALLATION.md](docs/INSTALLATION.md) and [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) for clinic deployment steps.

## First Login

After a fresh install, sign in with:

- **Username:** `admin`
- **Password:** `admin123`

You will be prompted to change the password on first login.

## Development Database (Docker)

```powershell
docker compose up -d
```

Use the connection string in `publish/appsettings.Docker.json` for local development against the containerized SQL Server instance.
