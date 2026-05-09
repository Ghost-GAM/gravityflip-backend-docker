using Microsoft.AspNetCore.Mvc;
using Gravityflip.Api.Aplicacion;

namespace Gravityflip.Api.Adaptadores
{
    // Responsabilidad única: gestionar el score
    [ApiController]
    [Route("api/jugador")]
    public class ScoreController : ControllerBase
    {
        private readonly GuardarScoreUseCase _guardarScore;

        public ScoreController(GuardarScoreUseCase guardarScore)
        {
            _guardarScore = guardarScore;
        }

        [HttpPost("score")]
        public async Task<IActionResult> GuardarScore([FromBody] GuardarScoreRequest request)
        {
            var ok = await _guardarScore.Ejecutar(request);
            if (!ok) return NotFound("Jugador no encontrado.");
            return Ok(new { mensaje = "Score actualizado." });
        }
    }
}