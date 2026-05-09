using Npgsql;
using Microsoft.Extensions.Configuration;
using Gravityflip.Api.Dominio;
using Gravityflip.Api.Puertos;

namespace Gravityflip.Api.Infraestructura
{
    // ─────────────────────────────────────────────────────────────────────────
    // Repositorio de RÉPLICA — solo lectura, conecta con PostgreSQL
    // ─────────────────────────────────────────────────────────────────────────
    public class ReplicaRepositorioPostgres : IReplicaRepositorio
    {
        private readonly string _connectionString;

        public ReplicaRepositorioPostgres(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("PostgresReplica")
                ?? throw new InvalidOperationException("Falta ConnectionString 'PostgresReplica'");
        }

        public async Task<IEnumerable<ResultadoPartida>> ObtenerTopDiez()
        {
            const string sql = @"
                SELECT id, nombre_jugador, password_hash,
                       tiempo_segundos, muertes, puntaje, fecha_partida
                FROM resultados_partida
                ORDER BY puntaje DESC
                LIMIT 10";

            var resultados = new List<ResultadoPartida>();

            await using var conn   = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd    = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                resultados.Add(new ResultadoPartida(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    (float)reader.GetDouble(3),
                    reader.GetInt32(4),
                    reader.GetInt32(5),
                    reader.GetDateTime(6)));
            }
            return resultados;
        }
    }
}