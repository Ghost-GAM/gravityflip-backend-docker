using Microsoft.AspNetCore.Mvc;
using Gravityflip.Api.Aplicacion;

namespace Gravityflip.Api.Adaptadores
{
    [ApiController]
    [Route("api/jugador")]
    public class ResetearJugadorController : ControllerBase
    {
        private readonly ResetearJugadorUseCase _resetearJugador;

        public ResetearJugadorController(ResetearJugadorUseCase resetearJugador)
        {
            _resetearJugador = resetearJugador;
        }

        [HttpPost("resetear")]
        public async Task<IActionResult> Resetear([FromBody] ResetearJugadorRequest request)
        {
            var ok = await _resetearJugador.Ejecutar(request);
            if (!ok) return NotFound("Jugador no encontrado.");
            return Ok(new { mensaje = "Progreso reiniciado." });
        }
    }
}