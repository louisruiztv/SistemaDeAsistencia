using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDeAsistencia.Data;
using SistemaDeAsistencia.Models;

namespace SistemaDeAsistencia.Controllers
{
    [Authorize] // Este atributo asegura que solo los usuarios logueados puedan acceder a este controlador
    public class AsistenciasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AsistenciasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Acción para la página de marcar asistencia
        public IActionResult Marcar()
        {
            return View();
        }

        [HttpPost]
        [Route("Asistencias/Marcar")]
        public async Task<IActionResult> MarcarAsync([FromBody] AsistenciaRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Tipo) || request.Latitud == null || request.Longitud == null)
            {
                return BadRequest("Faltan datos de tipo, latitud o longitud.");
            }

            var userId = _userManager.GetUserId(User);
            var hoy = DateTime.Today;

            var asistencia = await _context.Asistencias
                .FirstOrDefaultAsync(a => a.UsuarioId == userId && a.Fecha.Date == hoy);

            if (request.Tipo == "entrada")
            {
                if (asistencia == null)
                {
                    asistencia = new Asistencia
                    {
                        UsuarioId = userId,
                        Fecha = hoy,
                        Entrada = DateTime.Now,
                        LatitudEntrada = request.Latitud,
                        LongitudEntrada = request.Longitud,
                        DireccionEntrada = request.Direccion // Se guarda la dirección de entrada
                    };
                    _context.Asistencias.Add(asistencia);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "Entrada marcada con éxito." });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Ya has marcado la entrada hoy." });
                }
            }
            else if (request.Tipo == "salida")
            {
                if (asistencia != null && asistencia.Salida == null)
                {
                    asistencia.Salida = DateTime.Now;
                    asistencia.LatitudSalida = request.Latitud;
                    asistencia.LongitudSalida = request.Longitud;
                    asistencia.DireccionSalida = request.Direccion; // Se guarda la dirección de salida
                    _context.Asistencias.Update(asistencia);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "Salida marcada con éxito." });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Ya has marcado la salida o aún no has marcado la entrada." });
                }
            }

            return BadRequest(new { success = false, message = " Acción no válida." });
        }

        // definimos el DTO que es Data Transfer Object para recibir los datos de la solicitud
        public class AsistenciaRequestDto
        {
            public string Tipo { get; set; }
            public double? Latitud { get; set; }
            public double? Longitud { get; set; }
            public string? Direccion { get; set; }
        }
    }
}
