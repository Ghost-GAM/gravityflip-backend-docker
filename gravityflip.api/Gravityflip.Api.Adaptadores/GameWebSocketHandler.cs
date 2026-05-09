using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Gravityflip.Api.Adaptadores
{
    // Clase static: todos sus métodos son static, no necesita instanciarse
    public static class GameWebSocketHandler
    {
        private static readonly List<WebSocket> _clientes = new();

        public static async Task ManejarConexion(WebSocket socket)
        {
            _clientes.Add(socket);
            Console.WriteLine($"Cliente conectado. Total: {_clientes.Count}");

            var buffer = new byte[1024 * 4];

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var resultado = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (resultado.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Cierre normal", CancellationToken.None);
                        break;
                    }

                    var mensajeJson = Encoding.UTF8.GetString(buffer, 0, resultado.Count);
                    var mensaje = JsonSerializer.Deserialize<MensajeJuego>(mensajeJson);

                    if (mensaje != null)
                        await ProcesarMensaje(socket, mensaje);
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
            finally
            {
                _clientes.Remove(socket);
                Console.WriteLine($"Cliente desconectado. Total: {_clientes.Count}");
            }
        }

        private static async Task ProcesarMensaje(WebSocket remitente, MensajeJuego mensaje)
        {
            switch (mensaje.Tipo)
            {
                case "mover":
                    Console.WriteLine($"Jugador {mensaje.JugadorId} se movio a X:{mensaje.PosX} Y:{mensaje.PosY}");
                    await EnviarMensaje(remitente, new MensajeJuego
                    {
                        Tipo = "confirmacion",
                        JugadorId = mensaje.JugadorId,
                        PosX = mensaje.PosX,
                        PosY = mensaje.PosY
                    });
                    break;

                case "ping":
                    await EnviarMensaje(remitente, new MensajeJuego { Tipo = "pong" });
                    break;
            }
        }

        private static async Task EnviarMensaje(WebSocket socket, MensajeJuego mensaje)
        {
            if (socket.State != WebSocketState.Open) return;

            var json = JsonSerializer.Serialize(mensaje);
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
    }

    public class MensajeJuego
    {
        public string Tipo { get; set; } = "";
        public Guid JugadorId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
    }
}