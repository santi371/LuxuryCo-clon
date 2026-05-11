using LuxuryCo.Back.DTOs;
using LuxuryCo.Database.Data;
using Microsoft.EntityFrameworkCore;

namespace LuxuryCo.Back.Services;

public class UsuarioService : IUsuarioService
{
    private readonly LuxuryCoDbContext _context;

    public UsuarioService(LuxuryCoDbContext context)
    {
        _context = context;
    }

    public async Task<UsuarioDto> GetUsuarioByIdAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.id_usuario == usuarioId);

        if (usuario == null) throw new Exception("Usuario no encontrado");

        return new UsuarioDto
        {
            IdUsuario = usuario.id_usuario,
            Nombre = usuario.nombre,
            Email = usuario.email,
            Rol = usuario.Rol?.nombre_rol ?? "CLIENTE",
            FotoPerfilUrl = usuario.foto_perfil_url
        };
    }

    public async Task<string> UploadProfilePictureAsync(int usuarioId, IFormFile file, string webRootPath)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null) throw new Exception("Usuario no encontrado");

        if (file == null || file.Length == 0)
            throw new Exception("Archivo no válido");

        // Creamos la carpeta si no existe
        var uploadsFolder = Path.Combine(webRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Generamos un nombre único para la imagen
        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // Guardamos la URL relativa en la base de datos
        var relativeUrl = $"/uploads/{uniqueFileName}";
        usuario.foto_perfil_url = relativeUrl;

        await _context.SaveChangesAsync();

        return relativeUrl;
    }
}
