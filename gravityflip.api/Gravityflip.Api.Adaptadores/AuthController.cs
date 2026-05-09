using Microsoft.AspNetCore.Mvc;
using Gravityflip.Api.Aplicacion;

namespace Gravityflip.Api.Adaptadores
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly LoginUseCase _login;

        public AuthController(LoginUseCase login)
        {
            _login = login;
        }

        // POST /api/auth/login
        // Devuelve un JWT que se debe usar para guardar puntajes
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var respuesta = await _login.Ejecutar(request);
            if (!respuesta.Exito)
                return Unauthorized(new { mensaje = respuesta.Mensaje });

            return Ok(new
            {
                mensaje = respuesta.Mensaje,
                token   = respuesta.Token,
                tipo    = "Bearer"
            });
        }
    }
}