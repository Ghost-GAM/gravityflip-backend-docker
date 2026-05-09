namespace Gravityflip.Api.Puertos
{
    // ─────────────────────────────────────────────────────────────────────────
    // PUERTO DE SALIDA: IJuegoCliente
    //
    // Define el "contrato" de comunicación entre cualquier cliente del juego
    // (Unity, web, móvil, consola) y el servidor.
    //
    // Cualquier plataforma que quiera conectarse a la API solo necesita
    // implementar esta interfaz — sin tocar nada del servidor.
    //
    // Ejemplo:
    //   Unity implementa → UnityJuegoCliente : IJuegoCliente
    //   Web implementa   → WebJuegoCliente   : IJuegoCliente
    //   Móvil implementa → MovilJuegoCliente : IJuegoCliente
    // ─────────────────────────────────────────────────────────────────────────
    public interface IJuegoCliente
    {
        // ── Gestión del jugador ───────────────────────────────────────────────

        // Registrar un jugador nuevo al iniciar el juego
        Task<string> CrearJugadorAsync(string nombre);

        // Obtener los datos actuales del jugador (score, muertes, etc.)
        Task<DatosJugador?> ObtenerJugadorAsync(string jugadorId);

        // ── Eventos del juego ─────────────────────────────────────────────────

        // Registrar una muerte en el servidor
        Task RegistrarMuerteAsync(string jugadorId);

        // Guardar puntos ganados
        Task GuardarScoreAsync(string jugadorId, int puntos);

        // Reiniciar el progreso del jugador (score y muertes a cero)
        Task ReiniciarProgresoAsync(string jugadorId);

        // ── Movimiento en tiempo real (WebSocket) ─────────────────────────────

        // Enviar la posición del personaje al servidor
        Task EnviarPosicionAsync(string jugadorId, float x, float y);

        // Conectar el canal de tiempo real
        Task ConectarAsync();

        // Desconectar el canal de tiempo real
        Task DesconectarAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DTO: datos que el servidor devuelve sobre un jugador
    // ─────────────────────────────────────────────────────────────────────────
    public record DatosJugador(
        string Id,
        string Nombre,
        int Score,
        int CantidadMuertes
    );
}