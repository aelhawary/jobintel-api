using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
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

var connectionString = ResolvePostgresConnectionString(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("RecruitmentPlatformAPI")));

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

// Apply migrations on startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying PostgreSQL migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed.");
        throw;
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

static string ResolvePostgresConnectionString(IConfiguration configuration)
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        return ConvertDatabaseUrlToNpgsqlConnectionString(databaseUrl);
    }

    var configuredConnection = configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(configuredConnection))
    {
        return ConvertDatabaseUrlToNpgsqlConnectionString(configuredConnection);
    }

    throw new InvalidOperationException(
        "PostgreSQL connection is not configured. Set DATABASE_URL or ConnectionStrings:DefaultConnection.");
}

static string ConvertDatabaseUrlToNpgsqlConnectionString(string value)
{
    if (value.StartsWith("Host=", StringComparison.OrdinalIgnoreCase))
    {
        return value;
    }

    if (value.StartsWith("Server=", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "SQL Server-style connection string detected. Configure a PostgreSQL connection string or DATABASE_URL.");
    }

    if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Unsupported database URL format. Expected postgres:// or postgresql://");
    }

    var uri = new Uri(value);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
    var database = uri.AbsolutePath.Trim('/');

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = database,
        Username = username,
        Password = password,
        SslMode = SslMode.Require
    };

    return builder.ConnectionString;
}

