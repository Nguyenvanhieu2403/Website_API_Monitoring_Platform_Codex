using MonitoringPlatform.API.Data;
using MonitoringPlatform.API.Middleware;
using MonitoringPlatform.API.Settings;
using MonitoringPlatform.Application;
using MonitoringPlatform.Infrastructure;
using MonitoringPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure Settings with Validation
builder.Services.AddOptions<CorsSettings>()
    .Bind(builder.Configuration.GetSection(CorsSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<SeedDataSettings>()
    .Bind(builder.Configuration.GetSection(SeedDataSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Monitoring Platform API",
        Version = "v1",
        Description = "API for SaaS Monitoring Platform"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.CustomSchemaIds(type => type.FullName);

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

    c.EnableAnnotations();
});

// CORS Policy - Using Configuration
var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ConfiguredCorsPolicy", policy =>
    {
        if (corsSettings.AllowedOrigins.Any())
        {
            policy.WithOrigins(corsSettings.AllowedOrigins.ToArray());
        }
        else
        {
            policy.AllowAnyOrigin();
        }

        if (corsSettings.AllowedMethods.Any())
        {
            policy.WithMethods(corsSettings.AllowedMethods.ToArray());
        }
        else
        {
            policy.AllowAnyMethod();
        }

        if (corsSettings.AllowedHeaders.Any() && corsSettings.AllowedHeaders.Contains("*"))
        {
            policy.AllowAnyHeader();
        }
        else if (corsSettings.AllowedHeaders.Any())
        {
            policy.WithHeaders(corsSettings.AllowedHeaders.ToArray());
        }

        if (corsSettings.AllowCredentials)
        {
            policy.AllowCredentials();
        }

        if (corsSettings.MaxAgeSeconds.HasValue)
        {
            policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsSettings.MaxAgeSeconds.Value));
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Monitoring Platform API v1");
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("ConfiguredCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Custom exception handling middleware
app.UseGlobalExceptionMiddleware();

app.MapControllers();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var seedDataSettings = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SeedDataSettings>>().Value;

        // Ensure the database is created and migrations are applied
        await context.Database.MigrateAsync();

        // Seed initial data only if enabled
        if (seedDataSettings.EnableSeeding)
        {
            await DbSeed.SeedAsync(context, seedDataSettings);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        // Depending on your application's error handling, you might not want to re-throw here
        // For now, re-throwing to ensure the application doesn't start with a broken DB
        throw;
    }
}

app.Run();

public partial class Program { }
