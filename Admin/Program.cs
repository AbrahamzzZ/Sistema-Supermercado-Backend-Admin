using Domain.Context;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Utilities.IA;

var builder = WebApplication.CreateBuilder(args);

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

// Creacion de una nueva politica
builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica", app =>
    {
        app.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

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
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("NuevaPolitica");

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
