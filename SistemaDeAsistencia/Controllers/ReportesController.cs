using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using SistemaDeAsistencia.Data;
using X.PagedList;
using X.PagedList.Mvc.Core;
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
namespace SistemaDeAsistencia.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;  // para obtener datos del usuario actual
        private readonly IConfiguration _configurar;
        public ReportesController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, UserManager<ApplicationUser> userManager, IConfiguration configurar)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
            _configurar = configurar;
        }

        public async Task<IActionResult> Index(int? pagina, DateTime? fechaInicio, DateTime? fechaFin, string userId)
        {
            var asistencias = _context.Asistencias
                .Include(a => a.Usuario) //Esto incluye los datos del usuario relacionado
                .AsQueryable(); // Esto permite seguir filtrando la consulta

            if (fechaInicio.HasValue)
            {
                asistencias = asistencias.Where(a => a.Entrada.Value.Date >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                asistencias = asistencias.Where(a => a.Entrada.Value.Date <= fechaFin.Value.Date);
            }

            //filtro por usuario
            if (!string.IsNullOrEmpty(userId))
            {
                asistencias = asistencias.Where(a => a.UsuarioId == userId);
            }

            // Paginación:
            var registrosPorPagina = _configurar.GetValue("RegistroPorPagina", 5);
            var numeroPagina = pagina ?? 1; 

            
            var asistenciasPaginadas = await asistencias.OrderByDescending(a => a.Entrada).ToPagedListAsync(numeroPagina, registrosPorPagina);

            // Pasar la lista de usuarios y los filtros a la vista
            ViewData["Users"] = await _context.Users.ToListAsync();
            ViewData["SelectedUserId"] = userId;
            ViewData["FechaInicio"] = fechaInicio?.ToString("yyyy-MM-dd");
            ViewData["FechaFin"] = fechaFin?.ToString("yyyy-MM-dd");

            // Pasar el total de registros a la vista
            ViewData["TotalRegistros"] = asistenciasPaginadas.TotalItemCount;

            // Retornamos la lista paginada en lugar de la lista completa
            return View(asistenciasPaginadas);
        }



        [HttpPost]
        public async Task<IActionResult> ExportarPDF(DateTime? fechaInicio, DateTime? fechaFin, string userId)
        {

            //Usamos esta libreria para generar el pdf: iTextSharp-LPGL
            try
            {
                var asistenciasQuery = _context.Asistencias // 1. Construcción de la consulta filtrada
                    .Include(a => a.Usuario)
                    .AsQueryable();

                if (fechaInicio.HasValue)
                {
                    asistenciasQuery = asistenciasQuery.Where(a => a.Entrada.Value.Date >= fechaInicio.Value.Date);
                }

                if (fechaFin.HasValue)
                {
                    asistenciasQuery = asistenciasQuery.Where(a => a.Entrada.Value.Date <= fechaFin.Value.Date);
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    asistenciasQuery = asistenciasQuery.Where(a => a.UsuarioId == userId);
                }

                var asistencias = await asistenciasQuery.OrderByDescending(a => a.Entrada).ToListAsync();
                var currentUser = await _userManager.GetUserAsync(User); // Obtenemos el usuario actual para mostrar quién generó el reporte

                using (var memoryStream = new MemoryStream()) // Generamos el PDF en memoria
                {
                    var document = new Document(PageSize.A4, 25, 25, 30, 30);
                    var writer = PdfWriter.GetInstance(document, memoryStream);
                    document.Open();

                    var tableHeader = new PdfPTable(2);
                    tableHeader.WidthPercentage = 100;
                    tableHeader.SetWidths(new float[] { 1f, 4f });

                    var logoFileName = "1LOGOPEQUENO.png";
                    var logoPath = Path.Combine(_hostingEnvironment.WebRootPath, "imagenes", logoFileName);

                    try
                    {
   
                        var logoBytes = System.IO.File.ReadAllBytes(logoPath);
                        var logo = Image.GetInstance(logoBytes);
                        logo.ScaleAbsolute(120f, 50f);
                        var cellLogo = new PdfPCell(logo)
                        {
                            Border = Rectangle.NO_BORDER,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };
                        tableHeader.AddCell(cellLogo);
                    }
                    catch
                    {
                        var cellLogoPlaceholder = new PdfPCell(new Phrase("LOGO EMPRESA", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)))
                        {
                            Border = Rectangle.NO_BORDER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_CENTER
                        };
                        tableHeader.AddCell(cellLogoPlaceholder);
                    }

                    var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20);
                    var title = new Paragraph("Reporte de Asistencia", fontTitle)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    var cellTitle = new PdfPCell(title) //titulo de la celda
                    {
                        Border = Rectangle.NO_BORDER,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    };
                    tableHeader.AddCell(cellTitle);
                    document.Add(tableHeader);
                    document.Add(new Paragraph(" "));

                    var fontSummary = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    var summaryText = $"Reporte generado para el periodo: {fechaInicio?.ToShortDateString() ?? "Inicio"} - {fechaFin?.ToShortDateString() ?? "Fin"}. Total de registros: {asistencias.Count}.";
                    var summary = new Paragraph(summaryText, fontSummary);
                    summary.Alignment = Element.ALIGN_LEFT;
                    document.Add(summary);
                    document.Add(new Paragraph(" "));

                    var table = new PdfPTable(8);
                    table.WidthPercentage = 100;

                   
                    var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, Color.WHITE);  // Estilo para los encabezados de la tabla

                    var headerStyle = new PdfPCell(new Phrase("dummy"))
                    {

                        Border = Rectangle.NO_BORDER,
                        BorderWidthBottom = 1f,
                        Padding = 5,
                        BackgroundColor = new Color(40, 90,160),
                        BorderColorBottom = new Color(150, 150, 150)
                    };
                    headerStyle.Phrase = new Phrase("Usuario", fontHeader);
                    table.AddCell(headerStyle);
                    headerStyle.Phrase = new Phrase("Fecha", fontHeader);
                    table.AddCell(headerStyle);
                    headerStyle.Phrase = new Phrase("Hora Entrada", fontHeader);
                    table.AddCell(headerStyle);
                    headerStyle.Phrase = new Phrase("Hora Salida", fontHeader);
                    table.AddCell(headerStyle);
                    headerStyle.Phrase = new Phrase("Latitud Entrada", fontHeader);
                    table.AddCell(headerStyle);
                    headerStyle.Phrase = new Phrase("Longitud Entrada", fontHeader);
                    table.AddCell(headerStyle);
                    headerStyle.Phrase = new Phrase("Latitud Salida", fontHeader);
                    table.AddCell(headerStyle);
                    headerStyle.Phrase = new Phrase("Longitud Salida", fontHeader);
                    table.AddCell(headerStyle);

                    var fontData = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                    var dataStyle = new PdfPCell(new Phrase("dummy"))
                    {
                        Border = Rectangle.NO_BORDER,
                        BorderWidthBottom = 0.5f,
                        Padding = 5
                    };
                    foreach (var asistencia in asistencias)
                    {
                        dataStyle.Phrase = new Phrase(asistencia.Usuario?.NombreCompleto ?? "N/A", fontData);
                        table.AddCell(dataStyle);
                        dataStyle.Phrase = new Phrase(asistencia.Entrada.Value.ToShortDateString(), fontData);
                        table.AddCell(dataStyle);
                        dataStyle.Phrase = new Phrase(asistencia.Entrada?.ToString("HH:mm:ss") ?? "N/A", fontData);
                        table.AddCell(dataStyle);
                        dataStyle.Phrase = new Phrase(asistencia.Salida?.ToString("HH:mm:ss") ?? "N/A", fontData);
                        table.AddCell(dataStyle);
                        dataStyle.Phrase = new Phrase(asistencia.LatitudEntrada?.ToString() ?? "N/A", fontData);
                        table.AddCell(dataStyle);
                        dataStyle.Phrase = new Phrase(asistencia.LongitudEntrada?.ToString() ?? "N/A", fontData);
                        table.AddCell(dataStyle);
                        dataStyle.Phrase = new Phrase(asistencia.LatitudSalida?.ToString() ?? "N/A", fontData);
                        table.AddCell(dataStyle);
                        dataStyle.Phrase = new Phrase(asistencia.LongitudSalida?.ToString() ?? "N/A", fontData);
                        table.AddCell(dataStyle);
                    }

                    document.Add(table);


                    if (currentUser != null) //para escribir los datos del usuario que ha generado el reporte
                    {
                        document.Add(new Paragraph(" "));
                        document.Add(new Paragraph(" "));

                        var fontGeneratedBy = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                        var fontInfo = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                        var generatedBy = new Paragraph("Reporte generado por:", fontGeneratedBy)
                        {
                            Alignment = Element.ALIGN_RIGHT
                        };
                        document.Add(generatedBy);

                        var userName = new Paragraph(currentUser.NombreCompleto, fontInfo)
                        {
                            Alignment = Element.ALIGN_RIGHT
                        };
                        document.Add(userName);

                        var userEmail = new Paragraph(currentUser.Email, fontInfo)
                        {
                            Alignment = Element.ALIGN_RIGHT
                        };
                        document.Add(userEmail);
                        var generatedDate = new Paragraph($"Fecha de generación: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}", fontInfo)
                        {
                            Alignment = Element.ALIGN_RIGHT
                        };
                        document.Add(generatedDate);
                    }
                    document.Close();
                    writer.Close();

                    var fileName = $"ReporteAsistencia_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                    return File(memoryStream.ToArray(), "application/pdf", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Ocurrió un error al generar el PDF: {ex.Message}";
                return RedirectToAction(nameof(Index), new { fechaInicio, fechaFin, userId });
            }

        }



    }

}
