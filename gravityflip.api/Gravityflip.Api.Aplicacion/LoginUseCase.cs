using Gravityflip.Api.Puertos;

namespace Gravityflip.Api.Aplicacion
{
    public record LoginRequest(string NombreJugador, string Password);

    public record LoginRespuesta(
        bool Exito,
        string Mensaje,
        string Token = "");

    // ─────────────────────────────────────────────────────────────────────────
    // Caso de uso: Login con JWT
    // - Si el usuario existe: valida contraseña y devuelve token
    // - Si el usuario NO existe: lo registra y devuelve token
    // ─────────────────────────────────────────────────────────────────────────
    public class LoginUseCase
    {
        private readonly IResultadoRepositorio _repositorio;
        private readonly JwtService _jwt;

        public LoginUseCase(IResultadoRepositorio repositorio, JwtService jwt)
        {
            _repositorio = repositorio;
            _jwt = jwt;
        }

        public async Task<LoginRespuesta> Ejecutar(LoginRequest request)
        {
            // Validar entrada (XSS / SQL Injection)
            var (nombreValido, msgNombre) = ValidadorEntrada.ValidarNombre(request.NombreJugador);
            if (!nombreValido)
                return new LoginRespuesta(false, msgNombre);

            var (passValida, msgPass) = ValidadorEntrada.ValidarPassword(request.Password);
            if (!passValida)
                return new LoginRespuesta(false, msgPass);

            string hashPassword = JwtService.HashPassword(request.Password);
            var existente = await _repositorio.ObtenerPorNombre(request.NombreJugador);

            // Usuario nuevo — registrar
            if (existente == null)
            {
                string tokenNuevo = _jwt.GenerarToken(request.NombreJugador);
                return new LoginRespuesta(true,
                    "Usuario nuevo registrado. Guarda este token para tus partidas.",
                    tokenNuevo);
            }

            // Usuario existente — verificar contraseña
            if (existente.PasswordHash != hashPassword)
                return new LoginRespuesta(false,
                    "Contraseña incorrecta. No puedes suplantar este usuario.");

            // Login exitoso
            string token = _jwt.GenerarToken(request.NombreJugador);
            return new LoginRespuesta(true, "Login exitoso.", token);
        }
    }
}