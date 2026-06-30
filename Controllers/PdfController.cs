using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using PlayWrightPdfService.Services;

namespace PlaywrightPdfService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PdfController: ControllerBase{
    private readonly PdfServices _pdfService;
    private readonly ILogger<PdfController> _logger;
    private readonly IWebHostEnvironment _env;

    public PdfController(PdfServices pdfService, ILogger<PdfController> logger, IWebHostEnvironment env)
    {
        _pdfService = pdfService;
        _logger = logger;
        _env = env;
    }

    [HttpPost("from-url")]
    public async Task<IActionResult> GeneratePdfFromUrl([FromBody] UrlRequest request)
    {
        if ( string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest(new { error = "URL 不能为空" });
        }

        try
        {
            var pdfBytes = await _pdfService.UrlToPdfAsync(request.Url);

            var filename = string.IsNullOrWhiteSpace(request.Filename)
                ? "document"
                : request.Filename;

            // 确保文件名有 .pdf 后缀
            if (!filename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".pdf";
            }

            // 安全处理：移除路径分隔符和非法字符
            var safeFilename = Path.GetInvalidFileNameChars()
                .Aggregate(filename, (current, c) => current.Replace(c, '_'));

            var filesDir = Path.Combine(_env.WebRootPath, "files");

            if (!Directory.Exists(filesDir))
            {
                Directory.CreateDirectory(filesDir);
            }

            var filePath = Path.Combine(filesDir, safeFilename);
            await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

            // 构建 URL: /files/测试.pdf
            var fileUrl = $"{Request.Scheme}://{Request.Host}/files/{safeFilename}";

            return Ok(new { url = fileUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF 生成失败");
            return StatusCode(500, new { error = "generate PDF failed", message = ex.Message });
        }
    }
}

public class UrlRequest
{
    public string Url { get; set; } = string.Empty;
    public string? Filename { get; set; }
}