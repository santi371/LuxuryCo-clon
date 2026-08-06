using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Supabase;

namespace LuxuryCo.Back.Services;

public class ImageStorageService
{
    private readonly Client _supabaseClient;
    private readonly IWebHostEnvironment _env;
    private readonly HttpClient _httpClient;

    public ImageStorageService(Client supabaseClient, IWebHostEnvironment env)
    {
        _supabaseClient = supabaseClient;
        _env = env;
        _httpClient = new HttpClient();
    }

    public async Task<string> StoreImageAsync(string sourceUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sourceUrl)) return string.Empty;

            // Si ya es una URL relativa local, retornar con el dominio absoluto del backend
            if (sourceUrl.StartsWith("/") || sourceUrl.StartsWith("~") || !sourceUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var cleanPath = sourceUrl.Replace("~", "");
                if (!cleanPath.StartsWith("/")) cleanPath = "/" + cleanPath;
                return $"https://luxuryco.onrender.com{cleanPath}";
            }

            // ── BYPASS para proveedores de CDN de imágenes ──────────────────────
            // Pollinations y otros CDN generan la imagen cuando el NAVEGADOR abre la URL.
            // No tiene sentido descargar y re-almacenar: solo devolver la URL directamente.
            var cdnProviders = new[] {
                "pollinations.ai",
                "stability.ai",
                "cdn.stability.ai",
                "oaidalleapiprodscus.blob.core.windows.net"
            };
            foreach (var cdn in cdnProviders)
            {
                if (sourceUrl.Contains(cdn))
                {
                    return sourceUrl; // Retornar directamente — el navegador la carga sin problemas
                }
            }

            // Para otros proveedores (base64, blobs, etc.), guardar localmente
            var bytes = await _httpClient.GetByteArrayAsync(sourceUrl);
            var filename = $"gen_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}.png";

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadDir = Path.Combine(webRoot, "uploads", "generated");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }
            var localPath = Path.Combine(uploadDir, filename);
            await File.WriteAllBytesAsync(localPath, bytes);

            // Intentar subir a Supabase Bucket si está disponible
            try
            {
                var bucket = _supabaseClient.Storage.From("luxuryco-images");
                if (bucket != null)
                {
                    await bucket.Upload(bytes, filename, new Supabase.Storage.FileOptions { ContentType = "image/png" });
                    var publicUrl = bucket.GetPublicUrl(filename);
                    if (!string.IsNullOrEmpty(publicUrl))
                    {
                        return publicUrl;
                    }
                }
            }
            catch
            {
                // Supabase inactivo o pausado
            }

            return $"https://luxuryco.onrender.com/uploads/generated/{filename}";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al guardar la imagen generada: {ex.Message}", ex);
        }
    }
}
