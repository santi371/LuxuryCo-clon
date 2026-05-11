using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace LuxuryCo.Back.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var emailSettings = _config.GetSection("EmailSettings");
        var mail = new MailMessage()
        {
            From = new MailAddress(emailSettings["SenderEmail"], emailSettings["SenderName"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mail.To.Add(new MailAddress(toEmail));

        var portStr = emailSettings["Port"] ?? "587";
        using var smtpClient = new SmtpClient(emailSettings["SmtpServer"], int.Parse(portStr));
        smtpClient.EnableSsl = true;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Credentials = new NetworkCredential(emailSettings["SenderEmail"], emailSettings["Password"]);

        await smtpClient.SendMailAsync(mail);
    }
}
