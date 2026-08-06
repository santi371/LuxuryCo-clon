using LuxuryCo.Back.Services;
using LuxuryCo.Database.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LuxuryCo.Back.DTOs;
using LuxuryCo.Database.Models;

namespace LuxuryCo.Back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly IWebHostEnvironment _env;
    private readonly LuxuryCoDbContext _context;
    private readonly Supabase.Client _supabase;

    public UsuarioController(IUsuarioService usuarioService, IWebHostEnvironment env, LuxuryCoDbContext context, Supabase.Client supabase)
    {
        _usuarioService = usuarioService;
        _env = env;
        _context = context;
        _supabase = supabase;
    }

    [Authorize]
    [HttpGet("perfil")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var usuarioIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdStr)) return Unauthorized();

            var usuarioId = int.Parse(usuarioIdStr);
            var usuario = await _usuarioService.GetUsuarioByIdAsync(usuarioId);
            return Ok(usuario);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET api/usuario - Listar todos (para el panel admin)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Sede)
                .Select(u => new
                {
                    idUsuario = u.id_usuario,
                    nombre = u.nombre,
                    apellido = u.apellido,
                    email = u.email,
                    telefono = u.telefono,
                    rol = u.Rol != null ? u.Rol.nombre_rol : "CLIENTE",
                    idSede = u.id_sede,
                    sedeNombre = u.Sede != null ? u.Sede.nombre : "Global",
                    activo = u.activo,
                    fechaRegistro = u.fecha_registro,
                    fotoPerfilUrl = u.foto_perfil_url
                })
                .OrderByDescending(u => u.fechaRegistro)
                .ToListAsync();

            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST api/usuario - Para que los admins creen cuentas con roles diferentes
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UsuarioCreateDto dto)
    {
        try
        {
            if (await _context.Usuarios.AnyAsync(u => u.email == dto.Email))
            {
                return BadRequest(new { message = "El email ya está registrado en la base de datos de LuxuryCo" });
            }

            // 1. Crear en Supabase Auth
            var session = await _supabase.Auth.SignUp(dto.Email, dto.Password);
            if (session == null || session.User == null)
            {
                return BadRequest(new { message = "No se pudo registrar la cuenta en el sistema de seguridad de Supabase." });
            }

            // 2. Insertar en nuestra Base de Datos
            var newUsuario = new Usuario
            {
                nombre = dto.Nombre,
                apellido = dto.Apellido,
                email = dto.Email,
                password_hash = "SUPABASE_MANAGED",
                telefono = dto.Telefono,
                id_rol = dto.IdRol,
                id_sede = dto.IdSede,
                activo = true,
                fecha_registro = DateTime.UtcNow,
                two_factor_enabled = false
            };

            _context.Usuarios.Add(newUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario creado exitosamente mediante panel administrativo." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno: " + ex.Message });
        }
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Sede)
                .FirstOrDefaultAsync(u => u.id_usuario == id);

            if (usuario == null) return NotFound(new { message = "Usuario no encontrado." });

            return Ok(new
            {
                idUsuario = usuario.id_usuario,
                nombre = usuario.nombre,
                apellido = usuario.apellido,
                email = usuario.email,
                telefono = usuario.telefono,
                idRol = usuario.id_rol,
                rol = usuario.Rol?.nombre_rol,
                idSede = usuario.id_sede,
                sedeNombre = usuario.Sede?.nombre,
                activo = usuario.activo
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno: " + ex.Message });
        }
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UsuarioUpdateDto dto)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { message = "Usuario no encontrado." });

            usuario.nombre = dto.Nombre;
            usuario.apellido = dto.Apellido;
            usuario.telefono = dto.Telefono;
            usuario.id_rol = dto.IdRol;
            usuario.id_sede = dto.IdSede;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario actualizado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno: " + ex.Message });
        }
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { message = "Usuario no encontrado." });

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario eliminado permanentemente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno al eliminar. Asegúrate de que el usuario no tiene registros dependientes o transacciones. Detalles: " + ex.Message });
        }
    }

    // PATCH api/usuario/{id}/estado - Activar o desactivar usuario
    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> ToggleEstado(int id, [FromBody] EstadoDto dto)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { message = "Usuario no encontrado." });

            usuario.activo = dto.Activo;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Usuario {(dto.Activo ? "activado" : "desactivado")} exitosamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("upload-foto")]
    public async Task<IActionResult> UploadFoto(IFormFile file)
    {
        try
        {
            var usuarioIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdStr)) return Unauthorized();

            var usuarioId = int.Parse(usuarioIdStr);
            var url = await _usuarioService.UploadProfilePictureAsync(usuarioId, file, _env.WebRootPath);
            
            return Ok(new { FotoUrl = url, Mensaje = "Foto subida exitosamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            var roles = await _context.Roles
                .Select(r => new { r.id_rol, r.nombre_rol })
                .ToListAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class EstadoDto
{
    public bool Activo { get; set; }
}
