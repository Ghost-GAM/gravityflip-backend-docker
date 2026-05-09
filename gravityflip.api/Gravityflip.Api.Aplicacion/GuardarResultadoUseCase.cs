using System.Security.Cryptography;
using System.Text;
using Gravityflip.Api.Dominio;
using Gravityflip.Api.Puertos;

namespace Gravityflip.Api.Aplicacion
{
    public record GuardarResultadoRequest(
        string NombreJugador,
        string Password,
        float TiempoSegundos,
        int Muertes);

    public record ResultadoRespuesta(
        bool Exito,
        string Mensaje,
        int Puntaje = 0);

    public class GuardarResultadoUseCase
    {
        // Escribe en el PRIMARIO (SQL Server)
        private readonly IResultadoRepositorio _repositorio;

        public GuardarResultadoUseCase(IResultadoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<ResultadoRespuesta> Ejecutar(GuardarResultadoRequest request)
        {
            // ── PROTECCIÓN XSS / SQL Injection ────────────────────────────────
            var (nombreValido, msgNombre) = ValidadorEntrada.ValidarNombre(request.NombreJugador);
            if (!nombreValido)
                return new ResultadoRespuesta(false, msgNombre);

            var (passValida, msgPass) = ValidadorEntrada.ValidarPassword(request.Password);
            if (!passValida)
                return new ResultadoRespuesta(false, msgPass);

            var (tiempoValido, msgTiempo) = ValidadorEntrada.ValidarTiempo(request.TiempoSegundos);
            if (!tiempoValido)
                return new ResultadoRespuesta(false, msgTiempo);

            var (muertesValidas, msgMuertes) = ValidadorEntrada.ValidarMuertes(request.Muertes);
            if (!muertesValidas)
                return new ResultadoRespuesta(false, msgMuertes);

            // ── Lógica de negocio ─────────────────────────────────────────────
            string hashPassword = HashPassword(request.Password);
            int nuevoPuntaje = ResultadoPartida.CalcularPuntaje(
                request.TiempoSegundos, request.Muertes);

            var existente = await _repositorio.ObtenerPorNombre(request.NombreJugador);

            if (existente == null)
            {
                var nuevo = new ResultadoPartida(
                    request.NombreJugador,
                    hashPassword,
                    request.TiempoSegundos,
                    request.Muertes);
                await _repositorio.Guardar(nuevo);
                return new ResultadoRespuesta(true, "Puntaje registrado.", nuevo.Puntaje);
            }

            if (existente.PasswordHash != hashPassword)
                return new ResultadoRespuesta(false, "Contraseña incorrecta para este nombre.");

            if (nuevoPuntaje <= existente.Puntaje)
                return new ResultadoRespuesta(false,
                    $"Tu puntaje anterior ({existente.Puntaje:N0}) es mayor. No se actualizó.",
                    existente.Puntaje);

            existente.ActualizarPuntaje(request.TiempoSegundos, request.Muertes);
            await _repositorio.Actualizar(existente);
            return new ResultadoRespuesta(true, "Nuevo récord personal guardado.", nuevoPuntaje);
        }

        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Lee de la RÉPLICA (PostgreSQL) — solo SELECT
    // ─────────────────────────────────────────────────────────────────────────
    public class ObtenerTablaUseCase
    {
        private readonly IReplicaRepositorio _replica;

        public ObtenerTablaUseCase(IReplicaRepositorio replica)
        {
            _replica = replica;
        }

        public async Task<IEnumerable<ResultadoPartida>> Ejecutar()
        {
            return await _replica.ObtenerTopDiez();
        }
    }
}