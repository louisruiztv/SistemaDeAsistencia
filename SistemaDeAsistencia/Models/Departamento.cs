using SistemaDeAsistencia.Data;
using System.ComponentModel.DataAnnotations;

namespace SistemaDeAsistencia.Models
{
    public class Departamento
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "el nombre es obligatorio")]
        [Display(Name = "Nombre del Departamento")]
        public string Nombre { get; set; }

        // Relación con los usuarios
        public ICollection<ApplicationUser> Usuarios { get; set; }
        public Departamento()
        {
            Usuarios = new List<ApplicationUser>();
        }
    }


    
    }