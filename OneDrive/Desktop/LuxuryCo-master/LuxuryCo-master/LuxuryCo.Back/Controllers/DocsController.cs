using LuxuryCo.Back.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace LuxuryCo.Back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DocsController : ControllerBase
{
    private readonly SecureFileStorageService _secureStorage;

    public DocsController(SecureFileStorageService secureStorage)
    {
        _secureStorage = secureStorage;
    }

    [HttpGet("download/{token}")]
    public IActionResult DownloadSecureDocument(string token)
    {
        var docInfo = _secureStorage.GetDocumentByToken(token);
        
        if (docInfo == null || !System.IO.File.Exists(docInfo.FilePath))
        {
            return NotFound(new { message = "Enlace de descarga inválido, expirado o archivo no encontrado." });
        }

        var stream = new FileStream(docInfo.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        
        // Return file and optionally force download
        return File(stream, docInfo.ContentType, docInfo.FileName);
    }
}
