using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using PlayWrightPdfService.Services;

namespace PlaywrightPdfService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PdfController: ControllerBase{
    private readonly PdfServices _pdfService;
    private readonly ILogger<PdfController> _logger;

    public PdfController(PdfServices pdfService, ILogger<PdfController> logger)
    {
        _pdfService = pdfService;
        _logger = logger;
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
                ? "document.pdf" 
                : request.Filename;
            
            Response.Headers.Append("filename", filename);
            
            return File(pdfBytes, "application/pdf");
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