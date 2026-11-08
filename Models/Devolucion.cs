
namespace Biblioteca.Api.Models
{
    public class Devolucion
    {
        public int Devolucionid { get; set; }
        public int Prestamoid { get; set; }
        public DateTime Fechadevolucion { get; set; }
        public string? Observaciones { get; set; }
    }
}
