

namespace Biblioteca.Api.Models
{
    public class Prestamo
    {
        public int Prestamoid { get; set; }
        public int Estudianteid { get; set; }
        public int Libroid { get; set; }
        public DateTime Fechaprestamo { get; set; }
        public DateTime Fechavencimiento { get; set; }
    }
}
