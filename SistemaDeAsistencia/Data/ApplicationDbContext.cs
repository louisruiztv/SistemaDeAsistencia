using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaDeAsistencia.Models;

namespace SistemaDeAsistencia.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Propiedades DbSet para tus nuevas tablas
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<Justificacion> Justificaciones { get; set; }
    }
}
