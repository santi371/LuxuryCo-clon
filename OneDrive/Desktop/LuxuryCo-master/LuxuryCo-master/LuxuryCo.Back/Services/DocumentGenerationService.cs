using System;
using System.IO;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace LuxuryCo.Back.Services;

public class DocumentGenerationService
{
    private readonly SecureFileStorageService _secureStorage;

    public DocumentGenerationService(SecureFileStorageService secureStorage)
    {
        _secureStorage = secureStorage;
    }

    public async Task<string> GenerateWordDocumentAsync(string title, string content)
    {
        // 1. Crear documento en memoria
        using var mem = new MemoryStream();
        using (var wordDocument = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
        {
            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());

            // Título
            Paragraph paraTitle = body.AppendChild(new Paragraph());
            Run runTitle = paraTitle.AppendChild(new Run());
            RunProperties titleRunProps = new RunProperties(new Bold(), new FontSize { Val = "48" }); // 24pt
            runTitle.AppendChild(titleRunProps);
            runTitle.AppendChild(new Text(title));

            // Espacio
            body.AppendChild(new Paragraph(new Run(new Text(""))));

            // Contenido (sanitizado básico por ahora)
            string sanitizedContent = content.Replace("<", "&lt;").Replace(">", "&gt;");
            string[] lines = sanitizedContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            
            foreach (var line in lines)
            {
                Paragraph paraLine = body.AppendChild(new Paragraph());
                Run runLine = paraLine.AppendChild(new Run());
                runLine.AppendChild(new Text(line));
            }
        }

        // 2. Guardar en almacenamiento seguro
        mem.Position = 0;
        var fileBytes = mem.ToArray();
        string fileName = $"LuxuryCo_Doc_{Guid.NewGuid():N}.docx";
        
        string downloadUrl = await _secureStorage.SaveFileSecurelyAsync(fileName, fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        return downloadUrl;
    }
}
