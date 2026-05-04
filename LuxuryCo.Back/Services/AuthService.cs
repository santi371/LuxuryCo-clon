using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LuxuryCo.Back.DTOs;
using LuxuryCo.Database.Data;
using LuxuryCo.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LuxuryCo.Back.Services;

public class AuthService : IAuthService
{
    private readonly LuxuryCoDbContext _context;
    private readonly IConfiguration _config;
    private readonly Supabase.Client _supabase;

    public AuthService(LuxuryCoDbContext context, IConfiguration config, Supabase.Client supabase)
    {
        _context = context;
        _config = config;
        _supabase = supabase;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        try
        {
            // 1. Autenticar con Supabase
            var session = await _supabase.Auth.SignIn(loginDto.Email, loginDto.Password);
            
            if (session == null || session.User == null)
                throw new Exception("Credenciales inválidas proporcionadas por Supabase.");

            // 2. Traer el perfil público de nuestra base de datos
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.email == loginDto.Email);

            if (usuario == null)
            {
                throw new Exception("Usuario autenticado pero perfil no encontrado en el catálogo.");
            }

            if (!usuario.activo)
                throw new Exception("Esta cuenta está desactivada.");

            // Retornamos nuestro propio JWT por ahora para no romper los demás controladores
            var token = GenerateJwtToken(usuario);

            return new AuthResponseDto
            {
                Token = token,
                Mensaje = "Login exitoso vía Supabase",
                Requires2FA = false,
                Usuario = new UsuarioDto
                {
                    IdUsuario = usuario.id_usuario,
                    Nombre = usuario.nombre,
                    Email = usuario.email,
                    Rol = usuario.Rol?.nombre_rol ?? "CLIENTE",
                    FotoPerfilUrl = usuario.foto_perfil_url
                }
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Error en Login: " + ex.Message);
        }
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            if (await _context.Usuarios.AnyAsync(u => u.email == registerDto.Email))
            {
                throw new Exception("El email ya está registrado en la base de datos de LuxuryCo");
            }

            // 1. Registrar en Supabase Auth
            var session = await _supabase.Auth.SignUp(registerDto.Email, registerDto.Password);
            
            if (session == null || session.User == null)
                throw new Exception("No se pudo registrar en la autenticación segura de Supabase.");

            // 2. Si es exitoso, insertar el perfil público
            var rolCliente = await _context.Roles.FirstOrDefaultAsync(r => r.nombre_rol == "CLIENTE");

            var newUsuario = new Usuario
            {
                nombre = registerDto.Nombre,
                apellido = registerDto.Apellido,
                email = registerDto.Email,
                password_hash = "SUPABASE_MANAGED", // La contraseña la maneja Supabase 
                telefono = registerDto.Telefono,
                id_rol = rolCliente?.id_rol ?? 2,
                activo = true,
                fecha_registro = DateTime.UtcNow,
                two_factor_enabled = false
            };

            _context.Usuarios.Add(newUsuario);
            await _context.SaveChangesAsync();

            // 3. Auto Login devolviendo token personalizado
            string token = string.Empty;
            string mensaje = "Registro exitoso vía Supabase";
            
            if (string.IsNullOrEmpty(session.AccessToken))
            {
                mensaje = "REQUIRES_EMAIL_CONFIRMATION";
            }
            else
            {
                token = GenerateJwtToken(newUsuario);
            }

            return new AuthResponseDto
            {
                Token = token,
                Mensaje = mensaje,
                Requires2FA = false,
                Usuario = new UsuarioDto
                {
                    IdUsuario = newUsuario.id_usuario,
                    Nombre = newUsuario.nombre,
                    Email = newUsuario.email,
                    Rol = newUsuario.Rol?.nombre_rol ?? "CLIENTE",
                    FotoPerfilUrl = newUsuario.foto_perfil_url
                }
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Error en Registro: " + ex.Message);
        }
    }

    public async Task ForgotPasswordAsync(string email)
    {
        // Supabase se encarga de enviar el correo de recuperación
        await _supabase.Auth.ResetPasswordForEmail(email);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        if (string.IsNullOrEmpty(resetPasswordDto.AccessToken) || string.IsNullOrEmpty(resetPasswordDto.RefreshToken)) 
            throw new Exception("Los tokens de seguridad (Access o Refresh) son inválidos o no existen.");

        // Inicializar la sesión con el token recibido desde el email
        await _supabase.Auth.SetSession(resetPasswordDto.AccessToken, resetPasswordDto.RefreshToken);

        // Actualizar la contraseña del usuario actualmente autenticado por el token
        var attributes = new Supabase.Gotrue.UserAttributes 
        { 
            Password = resetPasswordDto.NewPassword 
        };
        
        var user = await _supabase.Auth.Update(attributes);
        if (user == null) 
        {
            throw new Exception("Error al restablecer la contraseña en Supabase.");
        }
    }

    public Task<string> Setup2FAAsync(int usuarioId)
    {
        throw new NotImplementedException("2FA manual desactivado. Usar Supabase MFA opcional.");
    }

    public Task<AuthResponseDto> Verify2FAAsync(int usuarioId, string code)
    {
        throw new NotImplementedException("2FA manual desactivado. Usar Supabase MFA opcional.");
    }

    private string GenerateJwtToken(Usuario usuario)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var keyParam = jwtSettings["Key"] ?? "ThisIsASecretKey1234567890OuchNeedMoreCharacters";
        var key = Encoding.ASCII.GetBytes(keyParam);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.id_usuario.ToString()),
            new Claim(ClaimTypes.Email, usuario.email ?? string.Empty),
            new Claim(ClaimTypes.Name, usuario.nombre ?? string.Empty),
            new Claim(ClaimTypes.Role, usuario.Rol?.nombre_rol ?? "CLIENTE")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["ExpireDays"] ?? "7")),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
