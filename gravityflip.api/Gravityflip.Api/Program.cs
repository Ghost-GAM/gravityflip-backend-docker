using System.Text;
using Gravityflip.Api.Adaptadores;
using Gravityflip.Api.Aplicacion;
using Gravityflip.Api.Infraestructura;
using Gravityflip.Api.Puertos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// ─────────────────────────────────────────────────────────────────────────────
// NOTA: En esta version dockerizada NO levantamos Docker manualmente.
// Docker Compose ya se encarga de levantar todos los contenedores juntos.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ── Repositorios ──────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IJugadorRepositorio, JugadorRepositorioEnMemoria>();
builder.Services.AddScoped<IResultadoRepositorio, ResultadoRepositorioSql>();      // PRIMARIO: SQL Server (escrituras)
builder.Services.AddScoped<IReplicaRepositorio, ReplicaRepositorioPostgres>();     // RÉPLICA: PostgreSQL (solo lectura)

// ── Inicializador de base de datos: crea DB y tabla si no existen ────────────
builder.Services.AddHostedService<InicializadorBaseDatos>();

// ── CRON: Sincronización automática cada 6 segundos ──────────────────────────
builder.Services.AddHostedService<SincronizacionService>();

// ── JWT ───────────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<JwtService>();

var (secretKey, issuer, audience) = JwtService.ObtenerConfiguracion();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = issuer,
            ValidAudience            = audience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// ── Casos de uso ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<CrearJugadorUseCase>();
builder.Services.AddScoped<GuardarScoreUseCase>();
builder.Services.AddScoped<RegistrarMuerteUseCase>();
builder.Services.AddScoped<ResetearJugadorUseCase>();
builder.Services.AddScoped<GuardarResultadoUseCase>();
builder.Services.AddScoped<ObtenerTablaUseCase>();
builder.Services.AddScoped<LoginUseCase>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

app.Map("/ws", async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        await GameWebSocketHandler.ManejarConexion(socket);
    }
    else context.Response.StatusCode = 400;
});

app.MapControllers();
await app.RunAsync();