using System.Net.Http.Headers;

namespace LuxuryCo.Back.Services;

public interface IUsuarioService
{
    Task<string> UploadProfilePictureAsync(int usuarioId, IFormFile file, string webRootPath);
    Task<DTOs.UsuarioDto> GetUsuarioByIdAsync(int usuarioId);
}
