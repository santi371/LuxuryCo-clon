namespace LuxuryCo.Back.DTOs;

public class ForgotPasswordDto
{
    public string Email { get; set; }
}

public class ResetPasswordDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string NewPassword { get; set; }
}
