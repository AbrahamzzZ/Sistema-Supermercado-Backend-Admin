using Admin.Middleware;
using Domain.Context;
using Infrastructure.Extensions;
using Infrastructure.Handler;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using Utilities.IA;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, sp, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Admin")
    .WriteTo.Console()
    .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddDbContext<AdminSistemaSupermercadoContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CadenaSQL"),
        x =>
        {
            x.UseNetTopologySuite();
            x.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
        }
    )
);

builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddValidators();
builder.Services.AddHttpClient<OllamaClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434");
    client.Timeout = Timeout.InfiniteTimeSpan;
});

// Reportes: Admin consulta a Order, Inventory y Auth a través del Gateway, reenviando el token del usuario
builder.Services.AddTransient<TokenDelegatingHandler>();
builder.Services.AddHttpClient(MicroserviciosApiClient.NombreHttpClient, client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:GatewayUrl"] ?? "https://localhost:7134");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<TokenDelegatingHandler>();

// JWT Auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Descarga la clave pública desde {Authority}/.well-known/openid-configuration -> jwks.json
    options.Authority = jwtSettings["Authority"];
    options.RequireHttpsMetadata = jwtSettings.GetValue("RequireHttpsMetadata", true);
    options.RefreshInterval = TimeSpan.FromSeconds(30);
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});


builder.Services.AddControllers();


// Agregar Swagger para documentación API  https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "API-Admin", Version = "v1" });

    // Configuración de seguridad JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese el token JWT en el campo de texto: **Bearer {su token}**"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AdminSistemaSupermercadoContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();

// Un log por petición: Information = OK, Warning = 4xx (reglas de negocio / validación), Error = 5xx o excepción
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (ctx, _, ex) =>
        ex != null || ctx.Response.StatusCode >= 500 ? LogEventLevel.Error
        : ctx.Response.StatusCode >= 400 ? LogEventLevel.Warning
        : LogEventLevel.Information;

    options.EnrichDiagnosticContext = (diagnostico, ctx) =>
    {
        diagnostico.Set("IdUsuario", ctx.User.FindFirstValue(ClaimTypes.NameIdentifier));
        diagnostico.Set("CorrelationId", ctx.Items[CorrelationIdMiddleware.ItemKey]);
    };
});

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
