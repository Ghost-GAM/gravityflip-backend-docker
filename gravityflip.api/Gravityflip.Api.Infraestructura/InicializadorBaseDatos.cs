using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gravityflip.Api.Infraestructura
{
    // ─────────────────────────────────────────────────────────────────────────
    // Servicio que se ejecuta UNA VEZ al inicio.
    // Crea la base de datos y la tabla en SQL Server si no existen.
    // (La tabla en PostgreSQL ya la crea SincronizacionService)
    // ─────────────────────────────────────────────────────────────────────────
    public class InicializadorBaseDatos : IHostedService
    {
        private readonly string _connectionString;
        private readonly ILogger<InicializadorBaseDatos> _logger;

        public InicializadorBaseDatos(
            IConfiguration config,
            ILogger<InicializadorBaseDatos> logger)
        {
            _connectionString = config.GetConnectionString("SqlServer")
                ?? throw new InvalidOperationException("Falta SqlServer");
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[INIT] Verificando base de datos SQL Server...");

            // Esperar a que SQL Server este listo (pueden ser hasta 60 segundos en primer arranque)
            int intentos = 0;
            const int maxIntentos = 30;
            bool conectado = false;

            while (intentos < maxIntentos && !conectado)
            {
                try
                {
                    // Conectar a master para crear la base de datos
                    string masterConn = _connectionString.Replace(
                        "Database=GravityflipDB", "Database=master");

                    await using var conn = new SqlConnection(masterConn);
                    await conn.OpenAsync(cancellationToken);
                    conectado = true;

                    // Crear base de datos si no existe
                    const string sqlCrearDb = @"
                        IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GravityflipDB')
                        BEGIN
                            CREATE DATABASE GravityflipDB;
                        END";

                    await using var cmdDb = new SqlCommand(sqlCrearDb, conn);
                    await cmdDb.ExecuteNonQueryAsync(cancellationToken);
                    _logger.LogInformation("[INIT] Base de datos verificada/creada.");
                }
                catch (Exception ex)
                {
                    intentos++;
                    _logger.LogInformation(
                        "[INIT] Esperando SQL Server... intento {n}/{max} ({error})",
                        intentos, maxIntentos, ex.Message.Split('\n')[0]);
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                }
            }

            if (!conectado)
            {
                _logger.LogWarning("[INIT] No se pudo conectar a SQL Server despues de varios intentos.");
                return;
            }

            // Crear tabla en GravityflipDB
            try
            {
                const string sqlCrearTabla = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ResultadosPartida' AND xtype='U')
                    CREATE TABLE ResultadosPartida (
                        Id              UNIQUEIDENTIFIER PRIMARY KEY,
                        NombreJugador   NVARCHAR(100) NOT NULL UNIQUE,
                        PasswordHash    NVARCHAR(64)  NOT NULL,
                        TiempoSegundos  FLOAT         NOT NULL,
                        Muertes         INT           NOT NULL,
                        Puntaje         INT           NOT NULL,
                        FechaPartida    DATETIME2     NOT NULL
                    )";

                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken);
                await using var cmd = new SqlCommand(sqlCrearTabla, conn);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                _logger.LogInformation("[INIT] Tabla ResultadosPartida verificada/creada.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[INIT] Error al crear tabla: {error}", ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}