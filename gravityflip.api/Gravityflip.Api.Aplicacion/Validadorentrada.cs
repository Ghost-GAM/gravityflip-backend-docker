using System.Text.RegularExpressions;

namespace Gravityflip.Api.Aplicacion
{
    // ─────────────────────────────────────────────────────────────────────────
    // Protección contra XSS, SQL Injection y entradas maliciosas
    // ─────────────────────────────────────────────────────────────────────────
    public static class ValidadorEntrada
    {
        // Solo permite letras, números, guion bajo y guion. Entre 3 y 20 caracteres
        private static readonly Regex NombreValido = new(@"^[a-zA-Z0-9_-]{3,20}$", RegexOptions.Compiled);

        // Patrones peligrosos comunes
        private static readonly string[] PatronesPeligrosos = new[]
        {
            "<script", "</script>", "javascript:", "onerror=", "onclick=",
            "onload=", "<iframe", "<embed", "<object",
            "DROP TABLE", "DELETE FROM", "INSERT INTO", "UPDATE ",
            "UNION SELECT", "--", "/*", "*/", "xp_", "sp_"
        };

        public static (bool valido, string mensaje) ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre no puede estar vacío.");

            if (nombre.Length < 3)
                return (false, "El nombre debe tener al menos 3 caracteres.");

            if (nombre.Length > 20)
                return (false, "El nombre no puede tener más de 20 caracteres.");

            if (!NombreValido.IsMatch(nombre))
                return (false, "El nombre solo puede contener letras, números, guion bajo y guion.");

            // Detectar contenido malicioso
            string nombreUpper = nombre.ToUpper();
            foreach (var patron in PatronesPeligrosos)
            {
                if (nombreUpper.Contains(patron.ToUpper()))
                    return (false, "El nombre contiene caracteres no permitidos.");
            }

            return (true, "OK");
        }

        public static (bool valido, string mensaje) ValidarPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "La contraseña no puede estar vacía.");

            if (password.Length < 4)
                return (false, "La contraseña debe tener al menos 4 caracteres.");

            if (password.Length > 50)
                return (false, "La contraseña no puede tener más de 50 caracteres.");

            return (true, "OK");
        }

        public static (bool valido, string mensaje) ValidarTiempo(float tiempoSegundos)
        {
            if (tiempoSegundos < 0)
                return (false, "El tiempo no puede ser negativo.");

            if (tiempoSegundos > 3600)
                return (false, "El tiempo no puede ser mayor a 1 hora.");

            return (true, "OK");
        }

        public static (bool valido, string mensaje) ValidarMuertes(int muertes)
        {
            if (muertes < 0)
                return (false, "Las muertes no pueden ser negativas.");

            if (muertes > 10000)
                return (false, "Cantidad de muertes inválida.");

            return (true, "OK");
        }
    }
}