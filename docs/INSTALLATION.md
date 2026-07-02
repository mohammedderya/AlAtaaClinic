# AlAtaa Clinic — Installation Instructions

## System Requirements

| Requirement | Details |
|---|---|
| Operating System | Windows 10/11 (64-bit) |
| Database | SQL Server Express 2019 or later (local instance) |
| Runtime | .NET 8 Desktop Runtime |
| Disk Space | 500 MB for application + database growth |
| Network | Not required after installation (fully offline) |

## Step 1 — Install Prerequisites

### SQL Server Express

1. Download [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads).
2. Run the installer and choose **Basic** or **Custom** installation.
3. Install the default instance name **SQLEXPRESS**.
4. Enable **Windows Authentication** (recommended for clinic workstations).
5. Confirm the SQL Server (SQLEXPRESS) service is **Running** in `services.msc`.

### .NET 8 Desktop Runtime

1. Download [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (x64).
2. Install if not already present on the workstation.

## Step 2 — Install AlAtaa Clinic

1. Run `AlAtaaClinic-Setup-1.0.0.exe`.
2. If SQL Server Express is not detected, the installer displays guidance — install SQL Server first, then continue.
3. Accept the default installation folder: `C:\Program Files\AlAtaa Clinic`.
4. Optionally create a Desktop shortcut.
5. Finish setup and launch the application.

## Step 3 — First Launch Setup Wizard

On first launch, the setup wizard guides you through:

1. **Clinic Information** — Enter the clinic name.
2. **Database Connection** — Defaults to `.\SQLEXPRESS` with Windows Authentication.
3. **License Activation** — Enter your license key (trial mode is available if no key is provided).
4. **Complete** — Settings are saved to `appsettings.json`.

The application automatically:

- Creates the clinic database if it does not exist
- Applies Entity Framework Core migrations
- Seeds default branch, admin account, roles, and permissions

### Default Administrator Account

| Field | Value |
|---|---|
| Username | `admin` |
| Password | `admin123` |

Change this password immediately after first login. The application **requires** a password change before granting access on first sign-in.

## Configuration File

Database settings are stored in:

```
C:\Program Files\AlAtaa Clinic\appsettings.json
```

Example connection string (Windows Authentication):

```json
{
  "ConnectionStrings": {
    "ClinicDatabase": "Server=.\\SQLEXPRESS;Database=AlAtaaClinic;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=True;"
  }
}
```

## Logs

| Log | Location |
|---|---|
| Application log | `{InstallFolder}\logs\alataa-YYYYMMDD.log` |
| Startup log | `{InstallFolder}\logs\startup.log` |
| Crash log | `{InstallFolder}\logs\crash.log` |

## Troubleshooting

| Problem | Solution |
|---|---|
| "SQL Server Required" | Install/start SQL Server Express (SQLEXPRESS instance) |
| "Login failed" | Verify Windows Authentication or SQL credentials in `appsettings.json` |
| "Database could not be initialized" | Check logs folder; ensure the Windows user has permission to create databases |
| Application will not start | Confirm .NET 8 Desktop Runtime is installed |

## Uninstallation

Use **Settings → Apps → AlAtaa Clinic → Uninstall**, or run the uninstaller from the Start Menu.

User data (`appsettings.json`, `license.dat`, logs) marked as preserved during install may remain in the installation folder. Back up the database before uninstalling if you need to retain clinic data.
