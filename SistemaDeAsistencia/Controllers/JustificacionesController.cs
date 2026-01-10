using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // Para IWebHostEnvironment
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDeAsistencia.Data;
using SistemaDeAsistencia.Models;
using System.IO; // Necesario para Path, Directory, FileStream
using System.Threading.Tasks;
using X.PagedList; // Necesario para la clase Task

namespace SistemaDeAsistencia.Controllers
{
    [Authorize] 
    public class JustificacionesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;
        public readonly IConfiguration _configurar;

        public JustificacionesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment, IConfiguration configurar)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
            _configurar = configurar;
        }

        public IActionResult Crear()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Justificacion justificacion, IFormFile archivoPdf)
        {
            // *** CÓDIGO DE DEPURACIÓN - NO USAR EN PRODUCCIÓN ***
            // Se comenta la validación para forzar la ejecución de la lógica de guardado
            // if (ModelState.IsValid) { ... }

            // Si el archivo no es nulo y tiene contenido, intenta guardarlo.
            if (archivoPdf != null && archivoPdf.Length > 0)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    justificacion.UsuarioId = userId;

                    // Lógica para guardar el archivo...
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "justificaciones");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(archivoPdf.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await archivoPdf.CopyToAsync(fileStream);
                    }
                    justificacion.RutaArchivoPDF = "/justificaciones/" + uniqueFileName;

                    // Intenta guardar en la base de datos
                    _context.Justificaciones.Add(justificacion);
                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] = "Justificación enviada con éxito.";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Si el código llega aquí, hay un problema con permisos o la ruta del archivo.
                    ModelState.AddModelError("", "Ocurrió un error al guardar el archivo. " + ex.Message);
                    return View(justificacion);
                }
            }
            else
            {
                ModelState.AddModelError("archivoPdf", "Debe seleccionar un archivo PDF.");
            }

            // Si llegamos aquí, el archivo no se envió, y el formulario se vuelve a mostrar.
            return View(justificacion);
        }


        public IActionResult VerArchivo(string nombreArchivo)
        {
            if (string.IsNullOrEmpty(nombreArchivo))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_hostEnvironment.WebRootPath, "justificaciones", nombreArchivo);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // La clave es no proporcionar un nombre de archivo para la descarga.
            // Esto le indica al navegador que intente mostrar el archivo en lugar de descargarlo.
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var mimeType = "application/pdf";

            return File(fileStream, mimeType);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RevisarJustificaciones(int? pagina)
        {
            // Asegúrate de incluir la tabla Usuario para poder mostrar el nombre del empleado
            var justificaciones = _context.Justificaciones
                .Include(j => j.Usuario)
                .OrderByDescending(j => j.FechaInasistencia)
                .AsQueryable();

            var registrosPorPagina = _configurar.GetValue("RegistroPorPagina", 6);
            var numeroPagina = pagina ?? 1;
            var justificacionesPaginadas = await justificaciones.ToPagedListAsync(numeroPagina, registrosPorPagina);

            return View(justificacionesPaginadas);

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var justificacion = await _context.Justificaciones.FindAsync(id);
            if (justificacion == null)
            {
                return NotFound();
            }

            justificacion.Estado = "Aprobada";
            _context.Justificaciones.Update(justificacion);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Justificación aprobada con éxito.";
            return RedirectToAction(nameof(RevisarJustificaciones));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id)
        {
            var justificacion = await _context.Justificaciones.FindAsync(id);
            if (justificacion == null)
            {   
                return NotFound();
            }

            justificacion.Estado = "Rechazada";
            _context.Justificaciones.Update(justificacion);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Justificación rechazada con éxito.";
            return RedirectToAction(nameof(RevisarJustificaciones));
        }


    }
}
