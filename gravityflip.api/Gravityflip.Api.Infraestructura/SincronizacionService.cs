using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Gravityflip.Api.Infraestructura
{
    // ─────────────────────────────────────────────────────────────────────────
    // CRON / BackgroundService: copia datos de SQL Server → PostgreSQL
    // Se ejecuta cada 6 segundos automaticamente
    // ─────────────────────────────────────────────────────────────────────────
    public class SincronizacionService : BackgroundService
    {
        private readonly string _connSqlServer;
        private readonly string _connPostgres;
        private readonly ILogger<SincronizacionService> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(6);

        public SincronizacionService(IConfiguration config, ILogger<SincronizacionService> logger)
        {
            _connSqlServer = config.GetConnectionString("SqlServer")
                ?? throw new InvalidOperationException("Falta SqlServer");
            _connPostgres = config.GetConnectionString("PostgresReplica")
                ?? throw new InvalidOperationException("Falta PostgresReplica");
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[CRON] Servicio de sincronizacion iniciado. Intervalo: 6 segundos.");

            // Esperar a que PostgreSQL este listo
            await Task.Delay(TimeSpan.FromSeconds(8), stoppingToken);

            // Crear tabla en PostgreSQL si no existe
            await CrearTablaEnPostgres();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    int registrosSincronizados = await Sincronizar();
                    _logger.LogInformation(
                        "[CRON] Sincronizacion completada: {hora} — {count} registros copiados a PostgreSQL",
                        DateTime.Now.ToString("HH:mm:ss"),
                        registrosSincronizados);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[CRON] Error al sincronizar: {error}", ex.Message);
                }

                await Task.Delay(_intervalo, stoppingToken);
            }
        }

        private async Task CrearTablaEnPostgres()
        {
            try
            {
                const string sql = @"
                    CREATE TABLE IF NOT EXISTS resultados_partida (
                        id              UUID PRIMARY KEY,
                        nombre_jugador  VARCHAR(100) NOT NULL UNIQUE,
                        password_hash   VARCHAR(64)  NOT NULL,
                        tiempo_segundos FLOAT        NOT NULL,
                        muertes         INTEGER      NOT NULL,
                        puntaje         INTEGER      NOT NULL,
                        fecha_partida   TIMESTAMP    NOT NULL
                    )";

                await using var conn = new NpgsqlConnection(_connPostgres);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("[CRON] Tabla PostgreSQL verificada/creada.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[CRON] No se pudo crear tabla en PostgreSQL: {e}", ex.Message);
            }
        }

        private async Task<int> Sincronizar()
        {
            // 1. Leer todos los datos del primario (SQL Server)
            var datos = new List<(Guid id, string nombre, string hash,
                double tiempo, int muertes, int puntaje, DateTime fecha)>();

            await using (var connSql = new SqlConnection(_connSqlServer))
            {
                await connSql.OpenAsync();
                const string sqlLeer = @"
                    SELECT Id, NombreJugador, PasswordHash,
                           TiempoSegundos, Muertes, Puntaje, FechaPartida
                    FROM ResultadosPartida";

                await using var cmd    = new SqlCommand(sqlLeer, connSql);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    datos.Add((
                        reader.GetGuid(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetDouble(3),
                        reader.GetInt32(4),
                        reader.GetInt32(5),
                        reader.GetDateTime(6)));
                }
            }

            if (datos.Count == 0) return 0;

            // 2. Copiar a PostgreSQL usando UPSERT (insertar o actualizar)
            await using var connPg = new NpgsqlConnection(_connPostgres);
            await connPg.OpenAsync();

            foreach (var d in datos)
            {
                const string upsert = @"
                    INSERT INTO resultados_partida
                        (id, nombre_jugador, password_hash, tiempo_segundos, muertes, puntaje, fecha_partida)
                    VALUES
                        (@id, @nombre, @hash, @tiempo, @muertes, @puntaje, @fecha)
                    ON CONFLICT (id) DO UPDATE SET
                        nombre_jugador  = EXCLUDED.nombre_jugador,
                        password_hash   = EXCLUDED.password_hash,
                        tiempo_segundos = EXCLUDED.tiempo_segundos,
                        muertes         = EXCLUDED.muertes,
                        puntaje         = EXCLUDED.puntaje,
                        fecha_partida   = EXCLUDED.fecha_partida";

                await using var cmd = new NpgsqlCommand(upsert, connPg);
                cmd.Parameters.AddWithValue("@id",      d.id);
                cmd.Parameters.AddWithValue("@nombre",  d.nombre);
                cmd.Parameters.AddWithValue("@hash",    d.hash);
                cmd.Parameters.AddWithValue("@tiempo",  d.tiempo);
                cmd.Parameters.AddWithValue("@muertes", d.muertes);
                cmd.Parameters.AddWithValue("@puntaje", d.puntaje);
                cmd.Parameters.AddWithValue("@fecha",   d.fecha);
                await cmd.ExecuteNonQueryAsync();
            }

            return datos.Count;
        }
    }
}