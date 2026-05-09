using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Gravityflip.Api.Dominio;
using Gravityflip.Api.Puertos;

namespace Gravityflip.Api.Infraestructura
{
    public class ResultadoRepositorioSql : IResultadoRepositorio
    {
        private readonly string _connectionString;

        public ResultadoRepositorioSql(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SqlServer")
                ?? throw new InvalidOperationException("Falta ConnectionString 'SqlServer'");
        }

        public async Task Guardar(ResultadoPartida r)
        {
            const string sql = @"
                INSERT INTO ResultadosPartida
                    (Id, NombreJugador, PasswordHash, TiempoSegundos, Muertes, Puntaje, FechaPartida)
                VALUES
                    (@Id, @NombreJugador, @PasswordHash, @TiempoSegundos, @Muertes, @Puntaje, @FechaPartida)";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id",             r.Id);
            cmd.Parameters.AddWithValue("@NombreJugador",  r.NombreJugador);
            cmd.Parameters.AddWithValue("@PasswordHash",   r.PasswordHash);
            cmd.Parameters.AddWithValue("@TiempoSegundos", r.TiempoSegundos);
            cmd.Parameters.AddWithValue("@Muertes",        r.Muertes);
            cmd.Parameters.AddWithValue("@Puntaje",        r.Puntaje);
            cmd.Parameters.AddWithValue("@FechaPartida",   r.FechaPartida);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Actualizar(ResultadoPartida r)
        {
            const string sql = @"
                UPDATE ResultadosPartida
                SET TiempoSegundos = @TiempoSegundos,
                    Muertes        = @Muertes,
                    Puntaje        = @Puntaje,
                    FechaPartida   = @FechaPartida
                WHERE NombreJugador = @NombreJugador";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NombreJugador",  r.NombreJugador);
            cmd.Parameters.AddWithValue("@TiempoSegundos", r.TiempoSegundos);
            cmd.Parameters.AddWithValue("@Muertes",        r.Muertes);
            cmd.Parameters.AddWithValue("@Puntaje",        ResultadoPartida.CalcularPuntaje(r.TiempoSegundos, r.Muertes));
            cmd.Parameters.AddWithValue("@FechaPartida",   r.FechaPartida);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<ResultadoPartida?> ObtenerPorNombre(string nombreJugador)
        {
            const string sql = @"
                SELECT Id, NombreJugador, PasswordHash, TiempoSegundos, Muertes, Puntaje, FechaPartida
                FROM ResultadosPartida
                WHERE NombreJugador = @NombreJugador";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NombreJugador", nombreJugador);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return null;

            return new ResultadoPartida(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                (float)reader.GetDouble(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetDateTime(6));
        }

        public async Task<IEnumerable<ResultadoPartida>> ObtenerTopDiez()
        {
            const string sql = @"
                SELECT TOP 10 Id, NombreJugador, PasswordHash, TiempoSegundos, Muertes, Puntaje, FechaPartida
                FROM ResultadosPartida
                ORDER BY Puntaje DESC";

            var resultados = new List<ResultadoPartida>();
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
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