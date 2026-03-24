# JobIntel API - Free Deployment Guide

**Purpose:** Temporarily deploy the API so frontend developers can test integration.

---

## Quick Comparison

| Option | Setup Time | SQL Server | Public URL | Best For |
|--------|------------|------------|------------|----------|
| **A: ngrok** | 5 min | Use local DB | `*.ngrok-free.app` | Quick testing |
| **B: Azure** | 30 min | Azure SQL Free | `*.azurewebsites.net` | Production-like |

---

## Option A: ngrok Tunnel (Recommended for Quick Testing)

Exposes your local API to the internet. Keep your local SQL Server.

### Prerequisites
- Your API runs locally on `http://localhost:5217`
- SQL Server running locally
- ngrok account (free)

### Step 1: Install ngrok

```powershell
# Option 1: Using winget (Windows 10/11)
winget install ngrok.ngrok

# Option 2: Using Chocolatey
choco install ngrok

# Option 3: Manual download
# Go to https://ngrok.com/download and extract to a folder in PATH
```

### Step 2: Authenticate ngrok

1. Create free account at [ngrok.com](https://ngrok.com)
2. Copy your auth token from the [dashboard](https://dashboard.ngrok.com/get-started/your-authtoken)
3. Run:

```powershell
ngrok config add-authtoken YOUR_AUTH_TOKEN_HERE
```

### Step 3: Start Your API Locally

```powershell
cd D:\Graduation-Project\Backend-2\RecruitmentPlatformAPI
dotnet run
```

Verify it's running at `http://localhost:5217/swagger`

### Step 4: Start ngrok Tunnel

Open a **new terminal** (keep API running):

```powershell
ngrok http 5217
```

You'll see output like:

```
Session Status    online
Account           your-email (Plan: Free)
Forwarding        https://abc123.ngrok-free.app -> http://localhost:5217
```

### Step 5: Share the URL

Give your frontend developer:
- **Base URL:** `https://abc123.ngrok-free.app`
- **Swagger:** `https://abc123.ngrok-free.app/swagger`

### Step 6: Frontend Configuration

Frontend developer updates their `.env`:

```env
REACT_APP_API_URL=https://abc123.ngrok-free.app/api
# or
VITE_API_URL=https://abc123.ngrok-free.app/api
```

### ngrok Free Tier Limitations

| Limitation | Impact |
|------------|--------|
| URL changes each restart | Share new URL each time |
| Interstitial page | First request shows ngrok warning (click "Visit Site") |
| 1 tunnel at a time | Fine for single API |
| Sessions expire after 2 hours | Restart ngrok if disconnected |

### Stopping the Tunnel

```powershell
# Press Ctrl+C in the ngrok terminal
# Also stop your API with Ctrl+C
```

---

## Option B: Azure Free Tier (Production-Like)

Full cloud deployment with Azure App Service + Azure SQL Database.

### Prerequisites
- Azure account (free: [azure.microsoft.com/free](https://azure.microsoft.com/free))
- Azure CLI installed
- Git installed

### Step 1: Install Azure CLI

```powershell
winget install Microsoft.AzureCLI
```

Close and reopen terminal, then login:

```powershell
az login
```

### Step 2: Create Azure SQL Database (Free Tier)

```powershell
# Set variables (customize these)
$resourceGroup = "jobintel-rg"
$location = "eastus"
$sqlServer = "jobintel-sql-$(Get-Random -Maximum 9999)"
$database = "JobIntelDb"
$adminUser = "jobinteladmin"
$adminPassword = "YourStr0ngP@ssw0rd!"  # Change this!

# Create resource group
az group create --name $resourceGroup --location $location

# Create SQL Server
az sql server create `
    --name $sqlServer `
    --resource-group $resourceGroup `
    --location $location `
    --admin-user $adminUser `
    --admin-password $adminPassword

# Create FREE tier database (32GB limit)
az sql db create `
    --resource-group $resourceGroup `
    --server $sqlServer `
    --name $database `
    --edition GeneralPurpose `
    --compute-model Serverless `
    --family Gen5 `
    --capacity 1 `
    --auto-pause-delay 60 `
    --min-capacity 0.5 `
    --max-size 32GB

# Allow Azure services to access
az sql server firewall-rule create `
    --resource-group $resourceGroup `
    --server $sqlServer `
    --name AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0

# Get connection string
$connStr = "Server=tcp:$sqlServer.database.windows.net,1433;Initial Catalog=$database;Persist Security Info=False;User ID=$adminUser;Password=$adminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
Write-Host "Connection String: $connStr"
```

### Step 3: Create Azure App Service (Free Tier)

```powershell
$appName = "jobintel-api-$(Get-Random -Maximum 9999)"
$planName = "jobintel-plan"

# Create FREE App Service Plan
az appservice plan create `
    --name $planName `
    --resource-group $resourceGroup `
    --sku F1 `
    --is-linux false

# Create Web App
az webapp create `
    --name $appName `
    --resource-group $resourceGroup `
    --plan $planName `
    --runtime "dotnet:9"

# Configure connection string
az webapp config connection-string set `
    --name $appName `
    --resource-group $resourceGroup `
    --settings DefaultConnection="$connStr" `
    --connection-string-type SQLAzure

Write-Host "Your API URL: https://$appName.azurewebsites.net"
```

### Step 4: Configure App Settings

```powershell
az webapp config appsettings set `
    --name $appName `
    --resource-group $resourceGroup `
    --settings `
        ASPNETCORE_ENVIRONMENT="Production" `
        JwtSettings__SecretKey="YourSuperSecretKeyThatIsAtLeast32CharsLong12345" `
        EmailSettings__SenderPassword="umrjurfkdeyubwyb" `
        GoogleOAuth__ClientId="1094518034372-ka3p6p1dc6ur5d9os4pula12d2u9e7jl.apps.googleusercontent.com"
```

### Step 5: Publish Your API

```powershell
cd D:\Graduation-Project\Backend-2\RecruitmentPlatformAPI

# Build for release
dotnet publish -c Release -o ./publish

# Deploy using ZIP deploy
Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip -Force

az webapp deployment source config-zip `
    --name $appName `
    --resource-group $resourceGroup `
    --src ./deploy.zip

# Clean up
Remove-Item ./deploy.zip
Remove-Item ./publish -Recurse
```

### Step 6: Run Database Migrations

**Option A: Via Azure Cloud Shell**

```powershell
# In Azure Portal, open Cloud Shell and run:
az sql db execute `
    --name $database `
    --resource-group $resourceGroup `
    --server $sqlServer `
    --user $adminUser `
    --password $adminPassword `
    --file @migrations.sql
```

**Option B: Connect locally and run migrations**

Update your local `appsettings.Development.json` temporarily with Azure connection string, then:

```powershell
dotnet ef database update
```

### Step 7: Test the Deployment

```powershell
# Test health
curl https://$appName.azurewebsites.net/api/locations/countries

# Open Swagger
Start-Process "https://$appName.azurewebsites.net/swagger"
```

### Step 8: Share with Frontend Developer

```
Base URL: https://jobintel-api-XXXX.azurewebsites.net
Swagger:  https://jobintel-api-XXXX.azurewebsites.net/swagger
```

### Cleanup (When Done Testing)

**Delete everything (recommended):**

```powershell
az group delete --name $resourceGroup --yes --no-wait
```

This deletes the resource group and ALL resources inside it.

---

## Required Code Changes (Already Applied)

### CORS Configuration (Program.cs)

The CORS policy now allows these frontend hosts:
- `localhost` (any port)
- `*.vercel.app`
- `*.netlify.app`
- `*.pages.dev` (Cloudflare)
- `*.github.io`
- `*.onrender.com`
- `*.railway.app`
- `*.ngrok-free.app`

If your frontend is on a different domain, add it to `Program.cs`.

---

## Testing the Deployed API

### With Postman

1. Create a new collection
2. Set base URL as variable: `{{baseUrl}}` = `https://your-app.azurewebsites.net/api`
3. Test endpoints:

```
GET  {{baseUrl}}/locations/countries
POST {{baseUrl}}/auth/register
POST {{baseUrl}}/auth/login
GET  {{baseUrl}}/auth/me (with Bearer token)
```

### With Browser

Visit:
- `https://your-app.azurewebsites.net/swagger` - Interactive API docs
- `https://your-app.azurewebsites.net/api/locations/countries` - Quick test

### With curl

```bash
# Test countries endpoint
curl https://your-app.azurewebsites.net/api/locations/countries

# Test login
curl -X POST https://your-app.azurewebsites.net/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'
```

---

## Troubleshooting

### ngrok: "Too many connections"
- Free tier allows 1 tunnel. Close other ngrok sessions.

### ngrok: URL changed
- Free tier generates new URL each restart. Share the new URL.

### Azure: 503 Service Unavailable
- Free tier has cold start delay (~30 seconds). Wait and retry.

### Azure: Database connection failed
- Check firewall rules allow Azure services
- Verify connection string in App Settings

### CORS errors
- Check if frontend domain is in the allowed list in `Program.cs`
- Check browser DevTools Network tab for actual origin

### File uploads (Profile pictures, Resumes)
- Azure Free tier has limited storage
- Files stored in `wwwroot/Uploads/` will work but are ephemeral
- For production, use Azure Blob Storage

---

## Summary

| Task | ngrok | Azure |
|------|-------|-------|
| Setup time | 5 min | 30 min |
| Cost | Free | Free (with limits) |
| URL stability | Changes each restart | Permanent |
| Database | Use local | Azure SQL |
| Best for | Quick demos | Longer testing |
| Cleanup | Close terminal | `az group delete` |

**Recommendation:** Start with **ngrok** for quick testing. Switch to **Azure** if you need a stable URL for more than a few days.
