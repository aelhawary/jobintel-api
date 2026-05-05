# AppSettings Configuration Strategy

## How ASP.NET Config Loading Works

ASP.NET loads config files in this order (each layer **overrides** the previous):

```
1. appsettings.json          ← base defaults (TRACKED in Git)
2. appsettings.{ENV}.json    ← environment overrides (GITIGNORED)
```

The `{ENV}` is determined by the `ASPNETCORE_ENVIRONMENT` variable:
- **Local development** → `Development` → loads `appsettings.Development.json`
- **MonsterASP hosting** → `Production` → loads `appsettings.Production.json`

## File Roles

| File | Git Status | Purpose |
|------|-----------|---------|
| `appsettings.json` | ✅ **Tracked** | Safe defaults, placeholders, non-secret settings |
| `appsettings.Development.json` | 🚫 **Gitignored** | Local dev secrets (JWT key, Brevo key, localhost URLs) |
| `appsettings.Production.json` | 🚫 **Gitignored** | Production secrets (DB password, Brevo key, real URLs) |

## What Goes Where

### `appsettings.json` (pushed to GitHub)
- ✅ Logging config
- ✅ FileStorage rules (extensions, max size)
- ✅ JWT issuer/audience (not the secret key!)
- ✅ Placeholder values like `"SET_IN_DEVELOPMENT_CONFIG"`
- ✅ Default localhost URLs
- ❌ Never put real passwords, API keys, or connection strings here

### `appsettings.Development.json` (stays local, gitignored)
- Real JWT secret key
- Brevo API key + SMTP key
- Localhost URLs (`http://localhost:5217`, `http://localhost:5173`)
- Local or remote DB connection string

### `appsettings.Production.json` (stays local, gitignored)
- Real JWT secret key
- Brevo API key + SMTP key
- Production URLs (`http://jobintel.runasp.net`, Vercel frontend)
- MonsterASP DB connection string
- `UseHttpApi: true` (for MonsterASP SMTP restrictions)

## Key Rules

> [!IMPORTANT]
> - **Never commit secrets to Git.** Even if you delete them later, they remain in Git history.
> - **Both Development and Production files are gitignored** — they only exist on your machine.
> - **When you Publish to MonsterASP**, Visual Studio includes `appsettings.Production.json` in the deploy package automatically.
> - **If you share the project** with a teammate, they need to create their own `appsettings.Development.json` with their own keys.

## Re-Publishing Safely

When you publish to MonsterASP from Visual Studio:
1. Make sure `appsettings.Production.json` exists locally with all production secrets
2. Visual Studio Publish will bundle both `appsettings.json` + `appsettings.Production.json`
3. MonsterASP runs in `Production` environment, so it loads the Production overrides automatically
