using FootballAI.Application.Interfaces;
using FootballAI.Application.Services;
using FootballAI.src.FootbalaAI.API.Services;
using FootballAI.Infrastructure.Data;
using FootballAI.src.FootbalaAI.API.Hub;
using FootballAI.src.FootballAI.Infrastructure.Repositories;
using FootballAI.src.FootballAI.Infrastructure.Storage;
using FootballAI.src.FootballAI.ML.Detectors;
using FootballAI.src.FootballAI.ML.Trackers;
using FootballAI.src.FootballAI.ML.Video;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =================== LOGGING (Serilog) ===================
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("logs/footballai-.log", rollingInterval: RollingInterval.Day)
    .ReadFrom.Configuration(ctx.Configuration));

// =================== DATABASE ===================
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// =================== HANGFIRE (Background Jobs) ===================
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire")));
builder.Services.AddHangfireServer();

// =================== AUTHENTICATION ===================
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// =================== INFRASTRUCTURE SERVICES ===================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IBlobStorageService>(sp =>
    new AzureBlobStorageService(
        builder.Configuration["Storage:ConnectionString"]!,
        builder.Configuration["Storage:Container"]!,
        sp.GetRequiredService<ILogger<AzureBlobStorageService>>()));

// =================== APPLICATION SERVICES ===================
builder.Services.AddScoped<IVideoProcessingService, VideoProcessingService>();
builder.Services.AddScoped<IPlayerTrackingService, PlayerTrackingService>();
builder.Services.AddScoped<IEventDetectionService, EventDetectionService>();
builder.Services.AddScoped<IHeatmapService, HeatmapService>();
builder.Services.AddScoped<IReportService, ReportService>();

// =================== ML SERVICES ===================
builder.Services.AddSingleton<FrameExtractor>();
builder.Services.AddSingleton<IPlayerDetectionService>(sp =>
    new PlayerDetector(
        builder.Configuration["ML:YoloModelPath"]!,
        sp.GetRequiredService<ILogger<PlayerDetector>>()));
builder.Services.AddSingleton<IBallTrackingService, BallTracker>();

// =================== SIGNALR ===================
builder.Services.AddSignalR();
builder.Services.AddScoped<IAnalysisNotifier, AnalysisNotifier>();

// =================== CONTROLLERS & SWAGGER ===================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FootballAI API",
        Version = "v1",
        Description = "Intelligent football match video analysis platform"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
});

// =================== CORS ===================
builder.Services.AddCors(opts => opts.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

// =================== BUILD & PIPELINE ===================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire");

app.MapControllers();
app.MapHub<AnalysisHub>("/hubs/analysis");

// Run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
