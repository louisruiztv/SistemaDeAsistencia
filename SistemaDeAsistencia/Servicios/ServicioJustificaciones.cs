using SistemaDeAsistencia.Data;

namespace SistemaDeAsistencia.Servicios;

public class ServicioJustificaciones
{
    private readonly ApplicationDbContext _context;

    public ServicioJustificaciones(ApplicationDbContext contexto)
    {
        _context = contexto;
    }

    public int contarPendientes()
    {
        return _context.Justificaciones.Count(j=>j.Estado=="Pendiente");
    }
}
