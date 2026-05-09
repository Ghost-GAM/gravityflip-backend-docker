using Gravityflip.Api.Dominio;
using Gravityflip.Api.Puertos;

namespace Gravityflip.Api.Aplicacion
{
    public record GuardarScoreRequest(Guid JugadorId, int Puntos);
    public record RegistrarMuerteRequest(Guid JugadorId);
    public record CrearJugadorRequest(string Nombre);
    public record ResetearJugadorRequest(Guid JugadorId);

    public class GuardarScoreUseCase
    {
        private readonly IJugadorRepositorio _repositorio;
        public GuardarScoreUseCase(IJugadorRepositorio repositorio) => _repositorio = repositorio;

        public async Task<bool> Ejecutar(GuardarScoreRequest request)
        {
            var jugador = await _repositorio.ObtenerPorId(request.JugadorId);
            if (jugador == null) return false;
            jugador.AgregarScore(request.Puntos);
            await _repositorio.Actualizar(jugador);
            return true;
        }
    }

    public class RegistrarMuerteUseCase
    {
        private readonly IJugadorRepositorio _repositorio;
        public RegistrarMuerteUseCase(IJugadorRepositorio repositorio) => _repositorio = repositorio;

        public async Task<bool> Ejecutar(RegistrarMuerteRequest request)
        {
            var jugador = await _repositorio.ObtenerPorId(request.JugadorId);
            if (jugador == null) return false;
            jugador.RegistrarMuerte();
            await _repositorio.Actualizar(jugador);
            return true;
        }
    }

    public class CrearJugadorUseCase
    {
        private readonly IJugadorRepositorio _repositorio;
        public CrearJugadorUseCase(IJugadorRepositorio repositorio) => _repositorio = repositorio;

        public async Task<Jugador> Ejecutar(CrearJugadorRequest request)
        {
            var jugador = new Jugador(request.Nombre);
            await _repositorio.Guardar(jugador);
            return jugador;
        }
    }

    public class ResetearJugadorUseCase
    {
        private readonly IJugadorRepositorio _repositorio;
        public ResetearJugadorUseCase(IJugadorRepositorio repositorio) => _repositorio = repositorio;

        public async Task<bool> Ejecutar(ResetearJugadorRequest request)
        {
            var jugador = await _repositorio.ObtenerPorId(request.JugadorId);
            if (jugador == null) return false;
            jugador.ReiniciarScore();
            await _repositorio.Actualizar(jugador);
            return true;
        }
    }
}