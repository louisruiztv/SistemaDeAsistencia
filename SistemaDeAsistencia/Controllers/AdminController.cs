using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaDeAsistencia.Data;
using SistemaDeAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace SistemaDeAsistencia.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        
        public async Task<IActionResult> Empleados(string buscarNombre) //Listado y busqueda de empkeados
        {
            var usuarios = _userManager.Users.Include(u => u.Departamento).AsQueryable(); //muestra todos los empleados junto con sus departamentos

            if (!string.IsNullOrEmpty(buscarNombre)) //filtro de busqueda por nombre
            {
                ViewBag.CurrentFilter = buscarNombre;
                usuarios = usuarios.Where(u => u.NombreCompleto.ToLower().Contains(buscarNombre.ToLower()));
            }

            return View(await usuarios.ToListAsync());
        }


        public async Task<IActionResult> DetallesEmpleado(string id) 
        {
            if (id == null) //validamos que el ID exista antes de ver el historial de un trabajador
            {
                return NotFound();
            }

            var usuario = await _context.Users
                .Include(u => u.Asistencias) 
                .Include(u => u.Justificaciones)
                .Include(u=>u.Departamento)
                .FirstOrDefaultAsync(u => u.Id == id);
                

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

       
        public IActionResult CrearUsuario()
        {
            ViewBag.DepartamentoId = new SelectList(_context.Departamentos, "Id", "Nombre"); //cargamos la lista de departamentos
            return View();
        }

        [HttpPost] //al enviar el formulario --->
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(CrearUsuarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser  //creamos un nuevo objeto ApplicationUser
                { UserName = model.Email, 
                    Email = model.Email, 
                    NombreCompleto = model.NombreCompleto,
                    DepartamentoId = model.DepartamentoId
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    
                    if (await _roleManager.RoleExistsAsync("Empleado")) // Asignamos el rol "Empleado" por defecto al nuevo usuario
                    {
                        await _userManager.AddToRoleAsync(user, "Empleado");
                    }
                    else
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Empleado"));  // Si el rol "Empleado" no existe, lo creamos y asignamos
                        await _userManager.AddToRoleAsync(user, "Empleado");
                    }

                    TempData["Mensaje"] = $"Usuario {model.NombreCompleto} creado con éxito.";
                    return RedirectToAction("Empleados");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }
            ViewBag.DepartamentoId = new SelectList(_context.Departamentos, "Id", "Nombre", model.DepartamentoId);
            return View(model);
        }

        
        public async Task<IActionResult> EditarUsuario(string id) // Monstramos la vista previa para editar un usuario existente
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users.Include(u => u.Departamento).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditarUsuarioViewModel
            {
                Id = user.Id,
                Email = user.Email,
                NombreCompleto = user.NombreCompleto,
                DepartamentoId = user.DepartamentoId
            };
            ViewBag.DepartamentoId = new SelectList(_context.Departamentos, "Id", "Nombre", user.DepartamentoId);
            return View(model);
        }

        [HttpPost] //al guiardar 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(EditarUsuarioViewModel model)
        {
            if (ModelState.IsValid) //validamos antes con model state
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email; // se actualiza =n email, nombre, correo, y departamento
                user.UserName = model.Email; //El username se toma del mismo correo
                user.NombreCompleto = model.NombreCompleto;
                user.DepartamentoId = model.DepartamentoId;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["Mensaje"] = $"Usuario {model.NombreCompleto} actualizado con éxito.";
                    return RedirectToAction("Empleados");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewBag.DepartamentoId = new SelectList(_context.Departamentos, "Id", "Nombre", model.DepartamentoId);
            return View(model);
        }

        
        public async Task<IActionResult> EliminarUsuario(string id) // Mostramos la vista de confirmación para eliminar un usuario
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
           
            var roles = await _userManager.GetRolesAsync(user); //hacemos la validación para no eliminar al usuario administrador
            if (roles.Contains("Admin"))
            {
                TempData["Error"] = "No se puede eliminar un usuario con rol de Administrador.";
                return RedirectToAction(nameof(Empleados));
            }

            return View(user);
        }

        [HttpPost, ActionName("EliminarUsuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuarioConfirmado(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["Mensaje"] = $"Usuario {user.NombreCompleto} eliminado con éxito.";
                }
                else
                {
                    TempData["Error"] = "Ocurrió un error al intentar eliminar el usuario.";
                }
            }
            return RedirectToAction("Empleados");
        }
    }

   
    public class CrearUsuarioViewModel  // ViewModel para la creación de usuarios, aqui aislamos la logica de entrada de datos de entidades reales.
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un departamento.")]
        [Display(Name = "Departamento")]
        public int DepartamentoId { get; set; }
    }

  
    public class EditarUsuarioViewModel   // ViewModel para la edición de usuarios
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; }

        [Required]
        [Display(Name = "Departamento")]
        public int? DepartamentoId { get; set; }
    }
}

