namespace Gravityflip.Api.Dominio
{
    public class ResultadoPartida
    {
        public Guid Id { get; private set; }
        public string NombreJugador { get; private set; }
        public string PasswordHash { get; private set; }
        public float TiempoSegundos { get; private set; }
        public int Muertes { get; private set; }
        public int Puntaje { get; private set; }
        public DateTime FechaPartida { get; private set; }

        private const float TiempoBase         = 600f;
        private const int   PenalizacionMuerte  = 500;
        private const int   MultiplicadorTiempo = 100;

        public ResultadoPartida(string nombreJugador, string passwordHash, float tiempoSegundos, int muertes)
        {
            Id            = Guid.NewGuid();
            NombreJugador = nombreJugador;
            PasswordHash  = passwordHash;
            TiempoSegundos = tiempoSegundos;
            Muertes       = muertes;
            Puntaje       = CalcularPuntaje(tiempoSegundos, muertes);
            FechaPartida  = DateTime.UtcNow;
        }

        public ResultadoPartida(Guid id, string nombreJugador, string passwordHash, float tiempoSegundos, int muertes, int puntaje, DateTime fechaPartida)
        {
            Id             = id;
            NombreJugador  = nombreJugador;
            PasswordHash   = passwordHash;
            TiempoSegundos = tiempoSegundos;
            Muertes        = muertes;
            Puntaje        = puntaje;
            FechaPartida   = fechaPartida;
        }

        public static int CalcularPuntaje(float tiempoSegundos, int muertes)
        {
            int puntajeTiempo = (int)((TiempoBase - tiempoSegundos) * MultiplicadorTiempo);
            int penalizacion  = muertes * PenalizacionMuerte;
            return Math.Max(0, puntajeTiempo - penalizacion);
        }

        public void ActualizarPuntaje(float tiempoSegundos, int muertes)
        {
            TiempoSegundos = tiempoSegundos;
            Muertes        = muertes;
            FechaPartida   = DateTime.UtcNow;
        }
    }
}