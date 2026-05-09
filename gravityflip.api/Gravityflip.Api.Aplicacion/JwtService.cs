using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Gravityflip.Api.Aplicacion
{
    // ─────────────────────────────────────────────────────────────────────────
    // Servicio JWT — genera y valida tokens para autenticar peticiones
    // Protege contra: Suplantación de credenciales y Consultas sin autorización
    // ─────────────────────────────────────────────────────────────────────────
    public class JwtService
    {
        // Secret Key — en producción esto debe estar en variables de entorno
        private const string SecretKey = "GravityFlip-SuperSecretKey-2024-MinLength32Chars!";
        private const string Issuer    = "GravityflipApi";
        private const string Audience  = "GravityflipClient";
        private const int    DuracionMinutos = 60;

        public string GenerarToken(string nombreJugador)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, nombreJugador),
                new Claim(JwtRegisteredClaimNames.Sub, nombreJugador),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:             Issuer,
                audience:           Audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(DuracionMinutos),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        // Configuración pública para que Program.cs sepa cómo validar tokens
        public static (string secretKey, string issuer, string audience) ObtenerConfiguracion()
        {
            return (SecretKey, Issuer, Audience);
        }
    }
}