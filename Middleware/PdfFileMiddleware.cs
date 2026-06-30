namespace PlayWrightPdfService.Middleware;

/// <summary>
/// 中间件：拦截 /files/ 请求，设置 Content-Disposition 响应头以便浏览器下载时使用正确的文件名。
/// </summary>
public class PdfFileMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public PdfFileMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // 只处理 /files/ 下的 PDF 请求
        if (!path.StartsWith("/files/", StringComparison.OrdinalIgnoreCase) || !path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var filesDir = Path.Combine(_env.WebRootPath, "files");
        var fileName = Path.GetFileName(path.TrimStart('/'));
        var filePath = Path.Combine(filesDir, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = "文件不存在" });
            return;
        }

        // 设置响应头
        context.Response.ContentType = "application/pdf";

        var encodedFilename = Uri.EscapeDataString(fileName);
        var asciiFilename = Path.GetFileNameWithoutExtension(fileName);
        // 将非 ASCII 字符替换为下划线，用于 filename 字段
        asciiFilename = string.Concat(asciiFilename.Select(c => c > 127 ? '_' : c)) + ".pdf";

        context.Response.Headers.Append("Content-Disposition",
            $"attachment; filename=\"{asciiFilename}\"; filename*=UTF-8''{encodedFilename}");

        context.Response.Headers.Append("Content-Length", new System.IO.FileInfo(filePath).Length.ToString());

        await context.Response.SendFileAsync(filePath);
    }
}
