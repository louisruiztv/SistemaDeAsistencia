using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using SistemaDeAsistencia.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeAsistencia.Data
{
    public class ApplicationUser: IdentityUser
    {
        public string NombreCompleto { get; set; }

        public int? DepartamentoId { get; set; }

        [ForeignKey("DepartamentoId")]
        public Departamento Departamento { get; set; }
        // 👇 Relaciones agregadas
        public ICollection<Asistencia> Asistencias { get; set; }
        public ICollection<Justificacion> Justificaciones { get; set; }
    }
}
