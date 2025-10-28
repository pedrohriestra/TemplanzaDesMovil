using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Templanza.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// CORS amplio (luego restringí al dominio del front)
builder.Services.AddCors(o => o.AddPolicy("Any", p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Controllers + OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// DB (toma ConnectionStrings:Default desde appsettings o variables de entorno)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ===== JWT Auth =====
var jwtKey = builder.Configuration["Jwt:Key"]!;
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.RequireHttpsMetadata = false; // en Render podés dejar false porque está detrás de proxy TLS
    opt.SaveToken = true;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

// Swagger con botón Authorize (Bearer)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Templanza.Api", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Bearer. Ej: **Bearer eyJhbGciOi...**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { scheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// Auto-migrar DB al arrancar (útil en Somee)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Middleware
app.UseCors("Any");
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

// **Importante**: Autenticación antes de Autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// No hardcodear UseUrls: Render inyecta ASPNETCORE_URLS/PORT
app.Run();
