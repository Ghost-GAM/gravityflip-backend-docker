using Microsoft.AspNetCore.Mvc;
using Gravityflip.Api.Aplicacion;
using Microsoft.AspNetCore.Authorization;

namespace Gravityflip.Api.Adaptadores
{
    [ApiController]
    [Route("api/resultado")]
    public class ResultadoController : ControllerBase
    {
        private readonly GuardarResultadoUseCase _guardar;
        private readonly ObtenerTablaUseCase _tabla;

        public ResultadoController(GuardarResultadoUseCase guardar, ObtenerTablaUseCase tabla)
        {
            _guardar = guardar;
            _tabla   = tabla;
        }

        // POST /api/resultado/guardar
        [Authorize]
        [HttpPost("guardar")]
        public async Task<IActionResult> Guardar([FromBody] GuardarResultadoRequest request)
        {
            var respuesta = await _guardar.Ejecutar(request);
            if (!respuesta.Exito)
                return BadRequest(new { mensaje = respuesta.Mensaje, puntaje = respuesta.Puntaje });
            return Ok(new { mensaje = respuesta.Mensaje, puntaje = respuesta.Puntaje });
        }

        // GET /api/resultado/tabla
        [HttpGet("tabla")]
        public async Task<IActionResult> Tabla()
        {
            var resultados = await _tabla.Ejecutar();
            return Ok(resultados.Select((r, i) => new
            {
                posicion      = i + 1,
                nombre        = r.NombreJugador,
                puntaje       = r.Puntaje,
                tiempo        = TimeSpan.FromSeconds(r.TiempoSegundos).ToString(@"mm\:ss"),
                muertes       = r.Muertes,
                fecha         = r.FechaPartida.ToString("dd/MM/yyyy")
            }));
        }
    }
}