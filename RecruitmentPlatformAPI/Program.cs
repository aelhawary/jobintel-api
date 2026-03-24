using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.Data.Seed;
using RecruitmentPlatformAPI.Services.Auth;
using RecruitmentPlatformAPI.Services.JobSeeker;
using RecruitmentPlatformAPI.Services.Recruiter;
using RecruitmentPlatformAPI.Services.Assessment;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use camelCase for JSON property names (standard for REST APIs)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // Serialize enums as strings instead of integers
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Configure CORS for frontend integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin)) return true;
            var uri = new Uri(origin);

            // Allow localhost (development)
            if (uri.Host == "localhost" || uri.Host == "127.0.0.1") return true;

            // Allow common frontend hosting platforms
            if (uri.Host.EndsWith(".vercel.app")) return true;
            if (uri.Host.EndsWith(".netlify.app")) return true;
            if (uri.Host.EndsWith(".pages.dev")) return true;  // Cloudflare Pages
            if (uri.Host.EndsWith(".github.io")) return true;
            if (uri.Host.EndsWith(".onrender.com")) return true;
            if (uri.Host.EndsWith(".railway.app")) return true;

            // Allow ngrok tunnels (for temporary testing)
            if (uri.Host.EndsWith(".ngrok-free.app")) return true;
            if (uri.Host.EndsWith(".ngrok.io")) return true;

            return false;
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Configure EF Core (SQL Server or PostgreSQL based on connection string)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// If not set, try Railway's standard DATABASE_URL variable
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
}

// If still not set, use local default
connectionString ??= "Server=(localdb)\\mssqllocaldb;Database=RecruitmentPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Detect PostgreSQL by connection string format
    // Railway format: postgresql://user:password@host:port/database
    // Local SQL Server format: Server=...
    if (connectionString.Contains("postgresql://") || connectionString.Contains("postgres") || connectionString.Contains("Host="))
    {
        options.UseNpgsql(connectionString, b => b.MigrationsAssembly("RecruitmentPlatformAPI"));
    }
    else
    {
        options.UseSqlServer(connectionString, b => b.MigrationsAssembly("RecruitmentPlatformAPI"));
    }
});

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Configure Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure File Storage Settings
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorage"));

// Configure Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? ""))
    };
    
    // Add JWT debugging events
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("JWT Token validated successfully for user: {User}", context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Challenge triggered. Error: {Error}, ErrorDescription: {ErrorDescription}", 
                context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IJobSeekerService, JobSeekerService>();
builder.Services.AddScoped<IRecruiterService, RecruiterService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ISocialAccountService, SocialAccountService>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IProfilePictureService, ProfilePictureService>();
builder.Services.AddScoped<IExperienceService, ExperienceService>();
builder.Services.AddScoped<IEducationService, EducationService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IJobSeekerSkillService, JobSeekerSkillService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "JobIntel API", Version = "v1" });

    // Include XML comments for better Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Configure Swagger to use JWT Bearer token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Auto-migrate/create database on startup (for cloud deployment)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Log connection string info for debugging (mask password)
        var redactedConnStr = connectionString.Length > 50
            ? connectionString.Substring(0, 50) + "..."
            : connectionString;
        logger.LogInformation("Connection string detected: {ConnectionString}", redactedConnStr);

        // Check if this is PostgreSQL
        // Railway format: postgresql://user:password@host:port/database
        var isPostgres = connectionString.Contains("postgresql://") || connectionString.Contains("postgres") || connectionString.Contains("Host=");
        logger.LogInformation("Database type detected: {DatabaseType}", isPostgres ? "PostgreSQL" : "SQL Server");

        if (isPostgres)
        {
            // For PostgreSQL: Create database schema (migrations are SQL Server specific)
            logger.LogInformation("PostgreSQL detected. Ensuring database is created...");
            db.Database.EnsureCreated();

            // Seed data if tables are empty (EnsureCreated doesn't run HasData)
            if (!db.Countries.Any())
            {
                logger.LogInformation("Seeding reference data for PostgreSQL...");
                db.Countries.AddRange(CountrySeed.GetCountries());
                db.Languages.AddRange(LanguageSeed.GetLanguages());
                db.JobTitles.AddRange(JobTitleSeed.GetJobTitles());
                db.Skills.AddRange(SkillSeed.GetSkills());
                db.AssessmentQuestions.AddRange(AssessmentQuestionSeed.GetQuestions());
                db.SaveChanges();
                logger.LogInformation("Reference data seeded successfully.");
            }
        }
        else
        {
            // For SQL Server: Apply migrations
            logger.LogInformation("SQL Server detected. Applying migrations...");
            db.Database.Migrate();
        }

        logger.LogInformation("Database initialization completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed. Attempting fallback...");
        try
        {
            db.Database.EnsureCreated();
            logger.LogInformation("Fallback database creation succeeded.");
        }
        catch (Exception fallbackEx)
        {
            logger.LogError(fallbackEx, "Fallback database creation also failed.");
        }
    }
}

// Configure the HTTP request pipeline.
// Enable Swagger in all environments for API testing
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

