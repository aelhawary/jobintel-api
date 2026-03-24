# Railway Environment Variables - Detailed Setup

## 🔍 The Issue

The connection string wasn't being passed to the application correctly. I've added:
- ✅ Fallback to Railway's `DATABASE_URL` environment variable
- ✅ Debug logging to show what connection string is detected
- ✅ Better error messages

## ✅ How to Set Up Railway Variables Correctly

### Step 1: In Railway Dashboard - Open Your Project

Click on your project name to open it

### Step 2: Click on PostgreSQL Service

You should see both services:
- PostgreSQL (database)
- jobintel-api (web service)

Click on **PostgreSQL**

### Step 3: Get Connection String

Go to **Connect** tab

Look for one of these:
- **Connection string section** - Shows `postgres://...` or `postgresql://...`
- **Railway CLI** - Shows a `railway connect` command with the full URL

**Copy the connection string** - it should look like:
```
postgresql://postgres:PASSWORD@containers-us-west.railway.app:5432/railway
```

### Step 4: Click on jobintel-api Web Service

Now click on the **Web Service** (jobintel-api)

### Step 5: Go to Variables Tab

In the left menu, click **Variables** (or **Config** → **Environment**)

### Step 6: Set These Variables (EXACTLY as shown)

| Variable Name | Variable Value |
|---------------|----------------|
| `DATABASE_URL` | `postgresql://postgres:PASSWORD@containers-us-west.railway.app:5432/railway` |
| `JwtSettings__SecretKey` | `YourSuperSecretKeyThatIsAtLeast32CharsLong12345` |
| `EmailSettings__SenderPassword` | `umrjurfkdeyubwyb` |
| `GoogleOAuth__ClientId` | `1094518034372-ka3p6p1dc6ur5d9os4pula12d2u9e7jl.apps.googleusercontent.com` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

**IMPORTANT:**
- Use `DATABASE_URL` (not `ConnectionStrings__DefaultConnection`)
- Paste the FULL PostgreSQL connection string exactly as-is
- No quotes around the value
- No extra spaces

### Step 7: Deploy

```powershell
cd D:\Graduation-Project\Backend-2
git add .
git commit -m "Add fallback DATABASE_URL support and debug logging"
git push origin main
```

Railway auto-deploys in ~3 minutes.

### Step 8: Check Logs

Go back to Railway dashboard → **jobintel-api** → **Logs** tab

Look for these messages:

**✅ SUCCESS:**
```
Connection string detected: postgresql://postgres:...
Database type detected: PostgreSQL
PostgreSQL detected. Ensuring database is created...
Seeding reference data for PostgreSQL...
Database initialization completed successfully.
Now listening on http://+:8080
```

**❌ FAILURE (Bad connection string):**
```
Connection string detected: Server=(localdb)...
Database type detected: SQL Server
[ERROR] Format of the initialization string does not conform
```

## Troubleshooting

### Error: "Format of the initialization string does not conform"

**Cause:** Connection string is invalid or not set

**Fixes:**
1. **Delete ALL old variables** in Railway (if you have any from previous attempts)
2. **Add ONLY these 5 variables** exactly as shown above
3. **For `DATABASE_URL`:**
   - Copy fresh from Railway PostgreSQL → Connect tab
   - Make sure it starts with `postgresql://`
   - Include the password
4. **Save/Deploy**

### Error: "Could not translate LINQ query to SQL"

**Cause:** Using SQL Server connection string with PostgreSQL

**Fix:**
- Delete `ConnectionStrings__DefaultConnection` variable if it exists
- Only use `DATABASE_URL` variable
- Make sure `DATABASE_URL` starts with `postgresql://`

### How to Find PostgreSQL Connection String in Railway

1. Railway Dashboard → PostgreSQL service
2. Click **Connect** button
3. You'll see different options:
   - **Postgres CLI** - `psql postgres://...`
   - **Connection string** - `postgresql://...` (use this!)
   - **Node.js** - Shows connection method for Node apps
   - **Python** - Shows connection method for Python apps

The **connection string** option is what you need - copy everything after `postgresql://` prefix too.

### Still Getting Errors After Setting Variables?

1. **Wait 2-3 minutes** after setting variables
2. **Click "Redeploy"** on latest deployment
3. **Check logs again** - wait for "Database initialization completed"
4. **Verify variables** - Go back to Variables tab and confirm they're still set
5. **Check for typos** - Variable names are case-sensitive!

---

## Expected Flow After Deployment

```
1. App starts
2. Reads DATABASE_URL variable
3. Converts to PostgreSQL connection
4. Logs: "PostgreSQL detected..."
5. Creates database schema
6. Seeds reference data (countries, languages, etc.)
7. Logs: "Database initialization completed successfully"
8. API ready to accept requests ✅
```

## Testing After Success

Once deployment succeeds:

```
1. Visit: https://YOUR_RAILWAY_URL/swagger
2. Try: GET /api/locations/countries
3. Should return: 200 with list of countries
4. Try registration: POST /api/auth/register
```

---

## Quick Checklist

- [ ] PostgreSQL service exists in Railway
- [ ] Got connection string from Railway PostgreSQL → Connect
- [ ] Opened jobintel-api Web Service
- [ ] Deleted any old variables
- [ ] Added `DATABASE_URL = postgresql://...`
- [ ] Added other 4 required variables
- [ ] Code pushed to GitHub
- [ ] Deployment completed in Railway
- [ ] Logs show "PostgreSQL detected"
- [ ] Logs show "Database initialization completed"
- [ ] `/swagger` loads without errors
- [ ] `/api/locations/countries` returns 200

---

**Need help?** Follow the "Check Logs" section to diagnose what's happening.
