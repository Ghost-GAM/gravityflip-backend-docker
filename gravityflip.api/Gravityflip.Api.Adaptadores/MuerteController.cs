using Microsoft.AspNetCore.Mvc;
using Gravityflip.Api.Aplicacion;

namespace Gravityflip.Api.Adaptadores
{
    // Responsabilidad única: registrar muertes
    [ApiController]
    [Route("api/jugador")]
    public class MuerteController : ControllerBase
    {
        private readonly RegistrarMuerteUseCase _registrarMuerte;

        public MuerteController(RegistrarMuerteUseCase registrarMuerte)
        {
            _registrarMuerte = registrarMuerte;
        }

        [HttpPost("muerte")]
        public async Task<IActionResult> RegistrarMuerte([FromBody] RegistrarMuerteRequest request)
        {
            var ok = await _registrarMuerte.Ejecutar(request);
            if (!ok) return NotFound("Jugador no encontrado.");
            return Ok(new { mensaje = "Muerte registrada." });
        }
    }
}