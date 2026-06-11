using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.Data.Seed;
using RecruitmentPlatformAPI.Services;
using RecruitmentPlatformAPI.Services.Auth;
using RecruitmentPlatformAPI.Services.JobSeeker;
using RecruitmentPlatformAPI.Services.Recruiter;
using RecruitmentPlatformAPI.Services.Assessment;
using RecruitmentPlatformAPI.Services.Assessment.V2;
using RecruitmentPlatformAPI.Services.Assessment.LlmGeneration;

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

            // Allow local network IP addresses (for mobile testing)
            if (uri.Host.StartsWith("192.168.") || uri.Host.StartsWith("10.") || uri.Host.StartsWith("172.") || uri.Host.StartsWith("2.")) return true;

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

            // Allow MonsterASP hosting
            if (uri.Host.EndsWith(".runasp.net")) return true;

            return false;
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=RecruitmentPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("RecruitmentPlatformAPI")));

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Configure Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure File Storage Settings
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorage"));

// Configure Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (string.IsNullOrEmpty(jwtSettings?.SecretKey))
{
    throw new InvalidOperationException("JWT SecretKey is missing from configuration. Set JwtSettings:SecretKey in appsettings.");
}
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SecretKey))
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

// Register HttpClient for Brevo HTTP API email sending
builder.Services.AddHttpClient();

// Register in-memory cache for AI matching results
builder.Services.AddMemoryCache();

// Register services
builder.Services.AddScoped<SkillMatcher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IJobSeekerService, JobSeekerService>();
builder.Services.AddScoped<IRecruiterService, RecruiterService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ISocialAccountService, SocialAccountService>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddHttpClient<ICvParserService, GeminiCvParserService>(client =>
{
    var timeout = builder.Configuration.GetValue<int>("LlmSettings:TimeoutSeconds", 45);
    client.Timeout = TimeSpan.FromSeconds(timeout);
});
builder.Services.AddScoped<IProfilePictureService, ProfilePictureService>();
builder.Services.AddScoped<IExperienceService, ExperienceService>();
builder.Services.AddScoped<IEducationService, EducationService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IJobSeekerSkillService, JobSeekerSkillService>();
builder.Services.AddScoped<IEngagementService, EngagementService>();
builder.Services.AddScoped<IAIMatchingService, AIMatchingService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();

// Background Services
builder.Services.AddHostedService<WeeklyEngagementDigestService>();

// Assessment V2 — Groq LLM pipeline
builder.Services.Configure<LlmSettings>(
    builder.Configuration.GetSection("LlmSettings"));

builder.Services.AddHttpClient<ILlmQuestionGenerator, GroqQuestionGenerator>(client =>
{
    var timeout = builder.Configuration.GetValue<int>("LlmSettings:TimeoutSeconds", 45);
    client.Timeout = TimeSpan.FromSeconds(timeout);
});

builder.Services.AddScoped<QuestionGenerationOrchestrator>();
builder.Services.AddScoped<IAssessmentServiceV2, AssessmentServiceV2>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "JobIntel API", Version = "v1" });

    // Use full type names as schema IDs to avoid collisions between
    // identically-named DTOs in different namespaces (e.g. V1 vs V2).
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

    // Include XML comments for better Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

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
        logger.LogInformation("Applying SQL Server migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migration completed successfully.");

        // Seed geographic data
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        await GeographicSeeder.SeedAsync(db, env, logger);
        await GeographicSeeder.SeedCitiesAsync(db, env, logger);
        
        // Seed language data
        await LanguageSeeder.SeedAsync(db, logger, env.ContentRootPath);
        await FieldOfStudySeeder.SeedAsync(db, logger, env.ContentRootPath);

        // Seed skills data
        await SkillSeeder.SeedAsync(db, logger, env.ContentRootPath);

    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed.");
        throw;
    }
}

// Configure the HTTP request pipeline.
// Enable Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowFrontend");

// Serve uploaded files (Resumes, Profile Pictures)
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/Uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

