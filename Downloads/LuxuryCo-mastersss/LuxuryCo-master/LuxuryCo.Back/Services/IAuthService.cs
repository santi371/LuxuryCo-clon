using LuxuryCo.Back.DTOs;

namespace LuxuryCo.Back.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<string> Setup2FAAsync(int usuarioId);
    Task<AuthResponseDto> Verify2FAAsync(int usuarioId, string code);
    
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
}
