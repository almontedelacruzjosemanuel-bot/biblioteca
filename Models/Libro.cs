

namespace Biblioteca.Api.Models
{
    public class Libro
    {
        public int Libroid { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public string? ISBN { get; set; }
        public int Cantidadtotal { get; set; }
        public int Cantidaddisponible { get; set; }
    }
}
