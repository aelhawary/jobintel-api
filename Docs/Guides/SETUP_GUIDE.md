# Setup & Configuration Guide

**Last Updated:** December 28, 2025  
**Project:** JobIntel Recruitment Platform - Backend API

---

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- [SQL Server LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- Email account with SMTP access (Gmail recommended)

---

## Quick Start

### 1. Clone and Navigate

```bash
cd RecruitmentPlatformAPI
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure Database Connection

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RecruitmentPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Note:** If using a different SQL Server instance, update the connection string accordingly.

### 4. Configure Email Settings

Edit `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password",
    "SenderName": "JobIntel",
    "EnableSsl": true,
    "ApplicationUrl": "http://localhost:5217"
  }
}
```

**Gmail Setup:**
1. Enable 2-Step Verification on your Google account
2. Generate an App Password: [Google App Passwords](https://myaccount.google.com/apppasswords)
3. Use the generated app password (16 characters) in `SenderPassword`

See [EMAIL_SETUP_GUIDE.md](EMAIL_SETUP_GUIDE.md) for detailed email configuration.

### 5. Apply Database Migrations

```bash
dotnet ef database update
```

This creates the `RecruitmentPlatformDb` database with all 15 tables.

### 6. Run the Application

```bash
dotnet run
```

**Access Points:**
- HTTP: `http://localhost:5217`
- HTTPS: `https://localhost:7001`
- Swagger UI: `http://localhost:5217/swagger` (Development only)

---

## Project Structure

```
RecruitmentPlatformAPI/
├── Controllers/
│   └── AuthController.cs          # Authentication endpoints
├── Services/
│   ├── AuthService.cs             # Authentication business logic
│   ├── EmailService.cs            # Email sending service
│   └── TokenService.cs            # JWT token management
├── DTOs/
│   ├── RegisterDto.cs             # Registration request
│   ├── LoginDto.cs                # Login request
│   ├── EmailVerificationDto.cs    # Email verification request
│   ├── PasswordResetDtos.cs       # Password reset requests
│   └── AuthResponseDto.cs         # Authentication responses
├── Models/
│   ├── User.cs                    # User entity
│   ├── JobSeeker.cs               # Job seeker profile
│   ├── Recruiter.cs               # Recruiter profile
│   └── ... (other entities)
├── Data/
│   ├── AppDbContext.cs            # Database context
│   └── Migrations/                # EF Core migrations (squashed to InitialCreate)
├── Configuration/
│   ├── JwtSettings.cs             # JWT configuration
│   └── EmailSettings.cs           # Email configuration
├── Enums/
│   ├── AccountType.cs             # JobSeeker/Recruiter
│   └── LanguageProficiency.cs    # Language levels
├── Program.cs                     # Application entry point
└── appsettings.json               # Configuration file
```

---

## Configuration Details

### JWT Settings

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharsLong12345",
    "Issuer": "RecruitmentPlatformAPI",
    "Audience": "RecruitmentPlatformClient",
    "ExpirationMinutes": 1440
  }
}
```

**Important:**
- `SecretKey`: Must be at least 32 characters (change in production!)
- `ExpirationMinutes`: 1440 = 24 hours

### Email Settings

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password",
    "SenderName": "JobIntel",
    "EnableSsl": true,
    "ApplicationUrl": "http://localhost:5217"
  }
}
```

### Database Connection

The default connection uses SQL Server LocalDB:

```
Server=(localdb)\mssqllocaldb;Database=RecruitmentPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

**For Production:**
- Use a dedicated SQL Server instance
- Update connection string with proper credentials
- Use connection pooling
- Enable SSL/TLS

---

## Database Management

### Create New Migration

```bash
dotnet ef migrations add MigrationName --project RecruitmentPlatformAPI
```

### Apply Migrations

```bash
dotnet ef database update --project RecruitmentPlatformAPI
```

### Drop Database (Development Only)

```bash
dotnet ef database drop --force --project RecruitmentPlatformAPI
```

### Check Migration Status

```bash
dotnet ef migrations list --project RecruitmentPlatformAPI
```

---

## Running the Application

### Development Mode

```bash
dotnet run
```

Uses `appsettings.json` and `appsettings.Development.json`.

### Production Mode

```bash
dotnet run --environment Production
```

Uses `appsettings.json` only (ensure all settings are configured).

---

## Testing

### Automated Tests

Run the comprehensive test suite:

```powershell
cd Scripts
.\test-comprehensive-enhanced.ps1
```

### Manual Testing

Use Swagger UI at `http://localhost:5217/swagger`:
1. Open Swagger in browser
2. Click "Authorize" button
3. Enter JWT token: `Bearer {token}`
4. Test endpoints with "Try it out"

---

## Troubleshooting

### Database Connection Issues

**Error:** "Cannot open database"
- **Solution:** Ensure SQL Server LocalDB is installed and running
- **Check:** Run `sqllocaldb info mssqllocaldb` in PowerShell

**Error:** "Login failed for user"
- **Solution:** Use Windows Authentication (Trusted_Connection=True)
- **Alternative:** Update connection string with SQL Server credentials

### Email Sending Issues

**Error:** "SMTP Authentication failed"
- **Solution:** Verify Gmail App Password is correct
- **Check:** Ensure 2-Step Verification is enabled

**Error:** "Connection timeout"
- **Solution:** Check firewall settings
- **Check:** Verify SMTP server and port (Gmail: smtp.gmail.com:587)

### CORS Issues

**Error:** "CORS policy blocked"
- **Solution:** Ensure frontend URL is in allowed origins (port 3000 for React)
- **Check:** Verify CORS middleware is configured in Program.cs

### Token Issues

**Error:** "401 Unauthorized"
- **Solution:** Check if token is expired (24 hours)
- **Solution:** Verify Authorization header format: `Bearer {token}`
- **Solution:** Ensure token is included in request headers

---

## Production Deployment Checklist

### Security
- [ ] Change JWT SecretKey to strong random value
- [ ] Move email password to environment variables or secure storage
- [ ] Update ApplicationUrl to production domain
- [ ] Enable HTTPS redirect
- [ ] Configure CORS for specific production frontend URLs
- [ ] Review and update connection strings

### Password Policy
The platform enforces a balanced password policy:
- **Length:** 8-100 characters
- **Uppercase:** At least one letter (A-Z)
- **Lowercase:** At least one letter (a-z)  
- **Digit:** At least one number (0-9)
- **Examples:** `SecurePass123`, `MyPassword1`, `Welcome2024`

This policy balances security with usability, avoiding overwhelming complexity while ensuring basic password strength.

### Database
- [ ] Set up production SQL Server instance
- [ ] Run migrations on production database
- [ ] Configure database backups
- [ ] Set up connection pooling
- [ ] Test database performance

### Monitoring
- [ ] Set up application logging (Serilog, Application Insights)
- [ ] Configure error tracking
- [ ] Set up health checks
- [ ] Monitor API performance

### Configuration
- [ ] Remove or secure Swagger UI in production
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Review all appsettings.json values
- [ ] Configure proper SSL/TLS certificates

---

## Environment Variables

For production, consider using environment variables:

```bash
# Linux/Mac
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Server=..."

# Windows PowerShell
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:ConnectionStrings__DefaultConnection="Server=..."
```

---

## Support

For issues:
1. Check server logs (console output)
2. Review Swagger documentation
3. Test endpoints with Postman or Swagger UI
4. Check [EMAIL_SETUP_GUIDE.md](EMAIL_SETUP_GUIDE.md) for email issues
5. Review [API_REFERENCE.md](API_REFERENCE.md) for endpoint details

---

**Ready to develop?** Start the API and begin integrating with the frontend!

