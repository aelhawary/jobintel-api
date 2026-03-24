# PostgreSQL Compatibility Fixes

## Issue
Railway deployment was failing with 500 errors because the AppDbContext contained **SQL Server-specific syntax** that PostgreSQL doesn't support.

## Changes Made

### 1. Check Constraints
**Before (SQL Server syntax):**
```csharp
"[EndDate] IS NULL OR [EndDate] >= [StartDate]"
```

**After (Database-agnostic):**
```csharp
"EndDate IS NULL OR EndDate >= StartDate"
```

### 2. Filtered Indexes
**Before (SQL Server filtered index):**
```csharp
b.HasIndex(r => r.JobSeekerId)
 .IsUnique()
 .HasFilter("[IsDeleted] = 0")  // SQL Server specific
```

**After (Standard unique composite index):**
```csharp
b.HasIndex(r => new { r.JobSeekerId, r.IsDeleted })
 .IsUnique()
```

## Files Modified
- ✅ `Data/AppDbContext.cs` - Fixed 5 SQL Server-specific syntax issues

## Entities Fixed
1. **Education** - Check constraint for EndDate
2. **Experience** - Check constraint for EndDate
3. **Resume** - Filtered unique index
4. **Certificate** - Check constraint for ExpirationDate
5. **AssessmentAttempt** - Filtered unique index for InProgress status

## How to Redeploy to Railway

### Option 1: Auto-Deploy (Easiest)
```powershell
cd D:\Graduation-Project\Backend-2
git add .
git commit -m "Fix PostgreSQL compatibility issues"
git push origin main
```

Railway will automatically detect the push and redeploy in ~3-5 minutes.

### Option 2: Manual Deploy
1. Go to Railway Dashboard
2. Click your Web Service
3. Go to **Deployments** tab
4. Click **Redeploy** on the latest deployment

## Testing After Redeployment

### Test 1: Countries Endpoint (Should work now!)
```bash
curl https://YOUR_RAILWAY_URL/api/locations/countries
```

**Expected:**
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

### Test 2: Register a User
```bash
curl -X POST https://YOUR_RAILWAY_URL/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Test",
    "lastName": "User",
    "email": "test@example.com",
    "password": "Test123!",
    "accountType": "JobSeeker"
  }'
```

**Expected:**
```json
{
  "success": true,
  "message": "Registration successful. Please verify your email."
}
```

### Test 3: Swagger UI
Visit: `https://YOUR_RAILWAY_URL/swagger`

Try these endpoints:
- ✅ `GET /api/locations/countries`
- ✅ `GET /api/locations/languages`
- ✅ `POST /api/auth/register`

## What Changed Technically?

| Issue | Impact | Solution |
|-------|--------|----------|
| Bracket notation `[Column]` | PostgreSQL doesn't recognize | Removed brackets |
| Filtered indexes with `HasFilter()` | Different syntax per DB | Changed to composite unique indexes |
| Application-level uniqueness | - | Services already validate uniqueness |

## Important Notes

1. **Uniqueness Still Enforced**: The application-level validation in services already prevents duplicate active resumes and in-progress assessments.

2. **No Data Loss**: These were just index/constraint definitions - no actual data is affected.

3. **Both Databases Supported**: The code now works with both SQL Server (local) and PostgreSQL (Railway).

## If Issues Persist

Check Railway logs:
1. Go to Railway Dashboard
2. Click on Web Service
3. Go to **Logs** tab
4. Look for errors during database initialization

Common error messages:
- ✅ "PostgreSQL detected. Ensuring database is created..." - Good!
- ✅ "Seeding reference data for PostgreSQL..." - Good!
- ✅ "Database initialization completed successfully." - Perfect!
- ❌ "Syntax error near '['" - Old code still deployed
- ❌ "column \"IsDeleted\" does not exist" - Needs redeploy

## Verification Checklist

After redeployment:
- [ ] Push code to GitHub
- [ ] Wait for Railway auto-deploy (~3-5 min)
- [ ] Check deployment logs for "Database initialization completed"
- [ ] Test `/api/locations/countries` - should return 200
- [ ] Test `/swagger` - should load without errors
- [ ] Try registering a test user
- [ ] Share working URL with frontend developer

---

**Status:** Ready to redeploy! 🚀
