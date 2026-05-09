namespace Gravityflip.Api.Dominio
{
    // Es la "regla principal": qué datos tiene un jugador.
    public class Jugador
    {
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public int Score { get; private set; }
        public int CantidadMuertes { get; private set; }
        public DateTime FechaCreacion { get; private set; }

        // Constructor: cuando creas un jugador nuevo
        public Jugador(string nombre)
        {
            Id = Guid.NewGuid();
            Nombre = nombre;
            Score = 0;
            CantidadMuertes = 0;
            FechaCreacion = DateTime.UtcNow;
        }

        // Constructor para cuando lo cargas desde la base de datos
        public Jugador(Guid id, string nombre, int score, int cantidadMuertes, DateTime fechaCreacion)
        {
            Id = id;
            Nombre = nombre;
            Score = score;
            CantidadMuertes = cantidadMuertes;
            FechaCreacion = fechaCreacion;
        }

        // Métodos del dominio: las "reglas" del juego
        public void AgregarScore(int puntos)
        {
            if (puntos < 0) throw new ArgumentException("Los puntos no pueden ser negativos.");
            Score += puntos;
        }

        public void RegistrarMuerte()
        {
            CantidadMuertes++;
        }

        public void ReiniciarScore()
        {
            Score = 0;
        }
    }
}