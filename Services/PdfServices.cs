using Microsoft.Playwright;

namespace PlayWrightPdfService.Services;

public class PdfServices : IDisposable
{
    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private readonly ILogger<PdfServices> _logger;

    public PdfServices(ILogger<PdfServices> logger)
    {
        _logger = logger;
        try
        {
            _playwright = Playwright.CreateAsync().GetAwaiter().GetResult();

            _browser = _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {
                Headless = true,
            }).GetAwaiter().GetResult();

            _logger.LogInformation("✅ Playwright 浏览器启动成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Playwright 启动失败");
            throw;
        }
    }

    public async Task<byte[]> UrlToPdfAsync(string url, PagePdfOptions? options = null)
    {
        await using var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Height = 1080, Width = 1920 }
        });

        var page = await context.NewPageAsync();

        try
        {
            _logger.LogInformation("正在访问 URL： {Url}", url);

            await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            await page.WaitForTimeoutAsync(1000);

            var pdfOptions = options ?? new PagePdfOptions
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin
                {
                    Top = "1cm",
                    Bottom = "1cm",
                    Left = "1cm",
                    Right = "1cm",

                }
            };

            var pdfBytes = await page.PdfAsync(pdfOptions);

            _logger.LogInformation("✅ PDF 生成成功，大小: {Size} bytes", pdfBytes.Length);

            return pdfBytes;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "❌ PDF 生成失败\"");
            throw;
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public void Dispose()
    {
        try
        {
            _browser?.CloseAsync().GetAwaiter().GetResult();
            _playwright?.Dispose();
            _logger.LogInformation("🛑 Playwright 资源已释放");
        }
        catch (Exception ex)
        { 
            _logger.LogWarning(ex, "释放资源时出现警告");
        }
    }

}

