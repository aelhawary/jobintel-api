# Railway Deployment - Environment Variables Setup

## ⚠️ Problem We Fixed

The connection string detection was looking for `postgresql://` but wasn't finding it. This is now fixed in Program.cs.

## ✅ What to Do Now

### Step 1: Verify PostgreSQL Database is Created in Railway

1. Go to [railway.app](https://railway.app)
2. Open your project
3. You should see **2 services**:
   - ✅ **jobintel-api** (Web Service)
   - ✅ **postgresql** (Database)

If you don't see PostgreSQL, create it:
- Click **Create** → **Database** → **PostgreSQL**
- Wait for initialization

### Step 2: Get the PostgreSQL Connection String

1. Click on the **PostgreSQL** service in Railway
2. Go to the **Connect** tab
3. Look for **Railway CLI** section or **Connection String** section
4. Copy the connection string. It should look like:
   ```
   postgresql://postgres:password@containers-us-west-000.railway.app:5432/railway
   ```

### Step 3: Set Environment Variables in Web Service

1. Click on your **Web Service** (jobintel-api)
2. Go to **Variables** tab
3. **Delete any old variables** and add these NEW ones:

| Variable | Value |
|----------|-------|
| `ConnectionStrings__DefaultConnection` | `postgresql://...` (paste from Step 2) |
| `JwtSettings__SecretKey` | `YourSuperSecretKeyThatIsAtLeast32CharsLong12345` |
| `EmailSettings__SenderPassword` | `umrjurfkdeyubwyb` |
| `GoogleOAuth__ClientId` | `1094518034372-ka3p6p1dc6ur5d9os4pula12d2u9e7jl.apps.googleusercontent.com` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

⚠️ **IMPORTANT:** Make sure `ConnectionStrings__DefaultConnection` has the full PostgreSQL URL!

### Step 4: Redeploy

1. Click **Redeploy** on your latest deployment
2. OR push code to GitHub (auto-redeploys):
```powershell
git add .
git commit -m "Fix PostgreSQL connection string detection"
git push origin main
```

### Step 5: Check Logs

Railway should now:
1. Auto-deploy
2. Run database initialization
3. Show: `"PostgreSQL detected. Ensuring database is created..."`
4. Show: `"Database initialization completed successfully."`

---

## Troubleshooting

### Error: "Format of the initialization string does not conform"

**Cause:** Connection string variable not set properly

**Fix:**
1. Click on PostgreSQL service → **Connect** tab
2. Copy the full connection string exactly
3. Paste into `ConnectionStrings__DefaultConnection` variable
4. **No spaces, paste exactly as-is**

### Error: "connection refused"

**Cause:** PostgreSQL service not running

**Fix:**
1. Check PostgreSQL status in Railway
2. If crashed, click **Redeploy**
3. Wait 30 seconds, try again

### Error: "Database doesn't exist"

**Cause:** Connection string points to wrong database

**Fix:**
1. Get fresh connection string from Railway
2. Update environment variable
3. Redeploy

---

## Example Connection String Formats

**Railway PostgreSQL (CORRECT):**
```
postgresql://postgres:abc123@containers-us-west-000.railway.app:5432/railway
```

**Local SQL Server (WRONG for Railway):**
```
Server=(localdb)\mssqllocaldb;Database=RecruitmentPlatformDb;...
```

---

## Verification Checklist

After deployment:

- [ ] PostgreSQL service exists in Railway
- [ ] Connection string copied correctly
- [ ] `ConnectionStrings__DefaultConnection` variable set in Web Service
- [ ] All 5 environment variables added
- [ ] Web Service redeployed
- [ ] Logs show "PostgreSQL detected"
- [ ] Logs show "Database initialization completed successfully"
- [ ] Test: `https://YOUR_URL/api/locations/countries` returns 200

---

## Can't Find Your Connection String?

In Railway:
1. Click **PostgreSQL** service
2. Go to **Connect** tab
3. Look for:
   - **Local Connection String** (for connecting from your computer)
   - **Public Connection String** (for Railway services - use this!)
   - Or **Railway CLI** command (contains the full URL)

The one that starts with `postgresql://` is what you need.

---

**Once deployed and working:**
- Test endpoint: `https://YOUR_URL/swagger`
- Try: `GET /api/locations/countries`
- Should return countries list ✅
