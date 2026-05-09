using Gravityflip.Api.Dominio;

namespace Gravityflip.Api.Puertos
{
    public interface IResultadoRepositorio
    {
        Task Guardar(ResultadoPartida resultado);
        Task Actualizar(ResultadoPartida resultado);
        Task<ResultadoPartida?> ObtenerPorNombre(string nombreJugador);
        Task<IEnumerable<ResultadoPartida>> ObtenerTopDiez();
    }
}