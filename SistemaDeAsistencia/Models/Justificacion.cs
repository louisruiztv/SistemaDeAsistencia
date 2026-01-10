using SistemaDeAsistencia.Data;
using System.ComponentModel.DataAnnotations;

namespace SistemaDeAsistencia.Models
{
    public class Justificacion
    {
        public int Id { get; set; }

        // Relación con el usuario
        public string UsuarioId { get; set; }
        public ApplicationUser Usuario { get; set; }

        [Required(ErrorMessage = "La fecha de inasistencia es obligatoria.")]
        public DateTime FechaInasistencia { get; set; }
        [Required(ErrorMessage = "El motivo de la justificación es obligatorio.")]
        public string Motivo { get; set; }
        public string RutaArchivoPDF { get; set; }
        // Propiedad agregada para rastrear el estado de la justificación
        // Por defecto, lo ponemos "Pendiente" al ser creada.
        public string Estado { get; set; } = "Pendiente";
    }
}
