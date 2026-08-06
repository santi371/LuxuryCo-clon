using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Hosting;

namespace LuxuryCo.Back.Services;

public class SecureFileStorageService
{
    private readonly IMemoryCache _cache;
    private readonly IWebHostEnvironment _env;
    private readonly string _secureStoragePath;

    public SecureFileStorageService(IMemoryCache cache, IWebHostEnvironment env)
    {
        _cache = cache;
        _env = env;
        
        // Almacenar fuera del wwwroot (ContentRootPath + SecureDocs)
        _secureStoragePath = Path.Combine(_env.ContentRootPath, "SecureDocs");
        if (!Directory.Exists(_secureStoragePath))
        {
            Directory.CreateDirectory(_secureStoragePath);
        }
    }

    public async Task<string> SaveFileSecurelyAsync(string fileName, byte[] content, string contentType)
    {
        var filePath = Path.Combine(_secureStoragePath, fileName);
        await File.WriteAllBytesAsync(filePath, content);

        var token = Guid.NewGuid().ToString("N");
        
        // TTL de 24 horas para el enlace de descarga segura
        var docInfo = new SecureDocumentInfo
        {
            FilePath = filePath,
            ContentType = contentType,
            FileName = fileName,
            CreatedAt = DateTime.UtcNow
        };

        _cache.Set($"SecureDoc_{token}", docInfo, TimeSpan.FromHours(24));

        // Background worker can be used later to delete the physical file after 24h,
        // or we just delete it when downloaded or via a scheduled job.
        
        // Return relative signed URL
        return $"/api/docs/download/{token}";
    }

    public SecureDocumentInfo? GetDocumentByToken(string token)
    {
        if (_cache.TryGetValue($"SecureDoc_{token}", out SecureDocumentInfo? docInfo))
        {
            return docInfo;
        }
        return null;
    }
}

public class SecureDocumentInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
