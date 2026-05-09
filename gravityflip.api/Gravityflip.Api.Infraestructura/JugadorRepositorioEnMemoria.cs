using Gravityflip.Api.Dominio;
using Gravityflip.Api.Puertos;

namespace Gravityflip.Api.Infraestructura
{
    public class JugadorRepositorioEnMemoria : IJugadorRepositorio
    {
        private readonly List<Jugador> _jugadores = new();

        public Task<Jugador?> ObtenerPorId(Guid id)
        {
            var jugador = _jugadores.FirstOrDefault(j => j.Id == id);
            return Task.FromResult(jugador);
        }

        public Task<Jugador?> ObtenerPorNombre(string nombre)
        {
            var jugador = _jugadores.FirstOrDefault(j =>
                j.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(jugador);
        }

        public Task<IEnumerable<Jugador>> ObtenerTodos()
        {
            return Task.FromResult<IEnumerable<Jugador>>(_jugadores);
        }

        public Task Guardar(Jugador jugador)
        {
            _jugadores.Add(jugador);
            return Task.CompletedTask;
        }

        public Task Actualizar(Jugador jugador)
        {
            return Task.CompletedTask;
        }
    }
}