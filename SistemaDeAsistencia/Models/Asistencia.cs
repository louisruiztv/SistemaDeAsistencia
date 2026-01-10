using SistemaDeAsistencia.Data;
using System.ComponentModel.DataAnnotations;

namespace SistemaDeAsistencia.Models
{
    public class Asistencia
    {
        [Key]
        public int Id { get; set; }

        public string UsuarioId { get; set; }
        public ApplicationUser Usuario { get; set; }

        public DateTime Fecha { get; set; }

        public DateTime? Entrada { get; set; }
        public DateTime? Salida { get; set; }


        // Definimos nuestras propiedades para la geolocalización
        public double? LatitudEntrada { get; set; }
        public double? LongitudEntrada { get; set; }
        public string? DireccionEntrada { get; set; }

        public double? LatitudSalida { get; set; }
        public double? LongitudSalida { get; set; }
        public string? DireccionSalida { get; set; }
    }
}
