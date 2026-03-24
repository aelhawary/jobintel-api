# Railway Deployment Guide

**Free Tier Alternative without Credit Card Requirements**

Railway is a modern cloud platform similar to Render but with a more straightforward setup. It offers free credits and doesn't require payment methods for free tier testing.

---

## Step-by-Step Railway Deployment

### Prerequisites
- GitHub account with your code pushed
- Railway account (free signup at [railway.app](https://railway.app))

---

### Step 1: Push Your Code to GitHub

```powershell
cd D:\Graduation-Project\Backend-2

git add .
git commit -m "Add PostgreSQL support and Railway deployment config"
git push origin main
```

Your GitHub repo should now have:
- ✅ `Dockerfile`
- ✅ `.dockerignore`
- ✅ Updated `Program.cs` with PostgreSQL support
- ✅ Updated `appsettings.Production.json`

---

### Step 2: Create Railway Account

1. Go to [railway.app](https://railway.app)
2. Click **"Create Account"**
3. Sign in with GitHub (easiest option)
4. Authorize Railway to access your GitHub
5. Click **"Start a New Project"**

---

### Step 3: Add PostgreSQL Database

1. In Railway Dashboard → **Create** → **Database**
2. Select **PostgreSQL**
3. Wait for it to initialize (~30 seconds)
4. The database will appear in your project
5. Click on it to view the **connection strings**
6. Copy the **Database URL** (you'll need it for environment variables)

**Connection String Format:**
```
postgresql://user:password@host:port/database
```

---

### Step 4: Deploy Your API

1. In Railway Dashboard → **Create** → **GitHub Repository**
2. Connect your GitHub repo: `YOUR_USERNAME/jobintel-api`
3. Select the `main` branch
4. Railway will auto-detect the `Dockerfile` ✅

---

### Step 5: Configure Environment Variables

Once deployment starts:

1. Click on your **Web Service** (the deployed API)
2. Go to **Variables** tab
3. Add these environment variables:

| Variable | Value |
|----------|-------|
| `ConnectionStrings__DefaultConnection` | `postgresql://...` (from Step 3) |
| `JwtSettings__SecretKey` | `YourSuperSecretKeyThatIsAtLeast32CharsLong12345` |
| `EmailSettings__SenderPassword` | `umrjurfkdeyubwyb` |
| `GoogleOAuth__ClientId` | `1094518034372-ka3p6p1dc6ur5d9os4pula12d2u9e7jl.apps.googleusercontent.com` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

4. Click **Deploy** to restart with new variables

---

### Step 6: Get Your Public URL

1. In the Web Service settings, find **Public URL**
2. It should look like: `https://jobintel-api-production.up.railway.app`
3. Test it: `https://YOUR_URL/swagger`

---

### Step 7: Test the API

**Via Browser:**
```
https://YOUR_RAILWAY_URL/swagger
```

**Via Command Line:**
```powershell
curl https://YOUR_RAILWAY_URL/api/locations/countries
```

**Expected Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Egypt",
      "isoCode": "EG"
    },
    ...
  ]
}
```

---

### Step 8: Share with Frontend Developer

Give them these credentials:

```env
REACT_APP_API_URL=https://YOUR_RAILWAY_URL/api
REACT_APP_SWAGGER=https://YOUR_RAILWAY_URL/swagger
```

Or for Vite:
```env
VITE_API_URL=https://YOUR_RAILWAY_URL/api
VITE_SWAGGER=https://YOUR_RAILWAY_URL/swagger
```

---

## Railway vs Local Deployment

| Aspect | Local | Railway |
|--------|-------|---------|
| Server uptime | Only while your PC is on | 24/7 (with free tier limits) |
| Database | Local SQL Server | Cloud PostgreSQL |
| URL | Temporary tunnels | Permanent URL |
| Cost | Free | Free tier ($5 credit) |
| Setup time | 5 minutes | 15 minutes |
| Best for | Quick testing | Longer testing periods |

---

## Railway Free Tier Limitations

| Limit | Details |
|-------|---------|
| **Monthly hours** | 500 hours (rolls over unused) |
| **Compute** | 1x shared CPU, 512MB RAM |
| **Database storage** | 100MB PostgreSQL |
| **Inactivity timeout** | Services don't auto-stop |
| **Services per project** | Unlimited |

**Note:** Railway offers $5 free credit monthly, which is usually enough for testing.

---

## Monitoring & Logs

### View Deployment Logs:

1. Click on **Web Service** in dashboard
2. Go to **Logs** tab
3. Watch real-time output as your app starts

### Common Log Messages:

```
[12:34:56] PostgreSQL detected. Ensuring database is created...
[12:35:01] Seeding reference data for PostgreSQL...
[12:35:05] Database initialization completed successfully.
[12:35:10] Application started. Listening on http://+:8080
```

If you see these = **Deployment Success! ✅**

---

## Troubleshooting

### Error: "Connection refused" for database
- **Fix:** Make sure `ConnectionStrings__DefaultConnection` env var matches your database URL
- Check PostgreSQL service is running in Railway

### Error: "ASPNETCORE_ENVIRONMENT not set"
- **Fix:** Add `ASPNETCORE_ENVIRONMENT=Production` to environment variables

### Error: "Port already in use"
- Railway auto-manages this, but check logs for real errors

### API returns 500 errors
- Check Railway logs for stack traces
- Verify all environment variables are set correctly
- Test with: `https://YOUR_URL/api/locations/countries`

---

## Updating Your Deployment

**When you make local changes:**

```powershell
git add .
git commit -m "Your message"
git push origin main
```

Railway auto-detects the push and redeploys automatically ✅

---

## Cleanup (When Done Testing)

1. Go to Railway Dashboard
2. Click your **Project**
3. Click **Project Settings** → **Delete Project**
4. Confirm deletion

This removes both your API and database.

---

## Quick Deployment Checklist

- [ ] Code pushed to GitHub
- [ ] Railway account created
- [ ] PostgreSQL database created
- [ ] GitHub repo connected to Railway
- [ ] Environment variables added
- [ ] Deployment completed (check Logs)
- [ ] API responding at public URL
- [ ] Swagger docs working
- [ ] Frontend developer has the URL

---

**Next Step:** Once deployed, share the URL with your frontend team and they can update their `.env` file to start testing integration! 🚀
