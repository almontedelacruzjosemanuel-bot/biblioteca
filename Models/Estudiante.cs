using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Api.Models
{
    public class Estudiante
    {
        public int Estudianteid { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "La matrícula es obligatoria")]
        [StringLength(20)]
        public string Matricula { get; set; } = string.Empty;

        [StringLength(15)]
        public string? Celular { get; set; }

        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        [StringLength(100)]
        public string? Email { get; set; }
    }
}