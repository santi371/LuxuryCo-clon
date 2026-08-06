namespace LuxuryCo.Back.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; }
    public string Mensaje { get; set; }
    public bool Requires2FA { get; set; }
    public UsuarioDto Usuario { get; set; }
}

public class UsuarioDto
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; }
    public string Email { get; set; }
    public string Rol { get; set; }
    public string FotoPerfilUrl { get; set; }
    
    public int? IdSede { get; set; }
    public string? SedeNombre { get; set; }
}
