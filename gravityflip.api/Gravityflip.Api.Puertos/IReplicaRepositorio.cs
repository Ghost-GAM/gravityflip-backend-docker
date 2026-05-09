using Gravityflip.Api.Dominio;

namespace Gravityflip.Api.Puertos
{
    // Puerto de SOLO LECTURA — conecta con PostgreSQL (replica)
    public interface IReplicaRepositorio
    {
        Task<IEnumerable<ResultadoPartida>> ObtenerTopDiez();
    }
}