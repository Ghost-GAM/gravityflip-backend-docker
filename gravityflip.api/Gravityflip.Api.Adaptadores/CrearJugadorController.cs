using Microsoft.AspNetCore.Mvc;
using Gravityflip.Api.Aplicacion;

namespace Gravityflip.Api.Adaptadores
{
    // Responsabilidad única: crear jugadores
    [ApiController]
    [Route("api/jugador")]
    public class CrearJugadorController : ControllerBase
    {
        private readonly CrearJugadorUseCase _crearJugador;

        public CrearJugadorController(CrearJugadorUseCase crearJugador)
        {
            _crearJugador = crearJugador;
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] CrearJugadorRequest request)
        {
            var jugador = await _crearJugador.Ejecutar(request);
            return Ok(new { jugador.Id, jugador.Nombre, jugador.Score, jugador.CantidadMuertes });
        }
    }
}