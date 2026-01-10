using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDeAsistencia.Data;
using X.PagedList;

namespace SistemaDeAsistencia.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configurar;

        public EmpleadosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configurar)
        {
            _context = context;
            _userManager = userManager;
            _configurar = configurar;
        }

        // Acción para mostrar las asistencias del usuario logueado
        public async Task<IActionResult> MisAsistencias(int? pagina)
        {
            // Obtener el usuario actual
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Cargar las asistencias del usuario actual
            var asistencias = await _context.Asistencias
                .Where(a => a.UsuarioId == user.Id)
                .OrderByDescending(a => a.Entrada)
                .ToListAsync();
            
            var registrosPorPagina = _configurar.GetValue("RegistroPorPagina", 6); 
            var numeroPagina = pagina ?? 1;
            var asistenciasPaginadas = await asistencias.ToPagedListAsync(numeroPagina, registrosPorPagina); // Convertir la lista a una lista paginada

            return View(asistenciasPaginadas);
        }

        // Acción para mostrar las justificaciones del usuario logueado
        public async Task<IActionResult> MisJustificaciones()
        {
            // Obtener el usuario actual
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Cargar las justificaciones del usuario actual
            var justificaciones = await _context.Justificaciones
                .Where(j => j.UsuarioId == user.Id)
                .OrderByDescending(j => j.FechaInasistencia)
                .ToListAsync();

            return View(justificaciones);
        }
    }
}
