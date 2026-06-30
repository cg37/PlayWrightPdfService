---
name: aspnet-file-generation-with-download
description: Pattern for generating files server-side, saving to wwwroot with direct readable filenames, and serving with proper Content-Disposition for non-ASCII filenames
source: auto-skill
extracted_at: '2026-06-30T10:04:04.569Z'
---

# ASP.NET Core: File Generation with Download URL

When an API generates files (PDF, images, etc.) that need to be downloadable with custom filenames (especially non-ASCII/Chinese), use this pattern instead of returning binary directly.

## Why

- Returning binary directly means the frontend can't control the download filename (Blob URLs use random strings)
- HTTP headers don't handle raw Chinese characters well — must use RFC 5987 encoding via `filename*`
- Two URL design options exist (see **URL Design Choices** below)

## URL Design Choices

### Option A: Direct readable filenames (cleaner, preferred)

URL: `/files/测试.pdf`
- Pro: URL is human-readable, no query params
- Con: URL contains URL-encoded characters when shared (`%E6%B5%8B%E8%AF%95.pdf`)
- Middleware reads filename directly from the path

### Option B: GUID filename + query param (opaque but shareable)

URL: `/files/abc12345.pdf?fn=%E6%B5%8B%E8%AF%95.pdf`
- Pro: URL path is clean ASCII, only query param carries the encoded name
- Con: Ugly URL, requires passing `fn` query param
- Middleware reads filename from `fn` query parameter

Both options use the same `Content-Disposition` header format. Option A is used in the examples below.

## Steps

### 1. Create storage directory

```
wwwroot/files/
```

### 2. Controller: Save file, return URL

```csharp
[HttpPost("generate")]
public async Task<IActionResult> Generate([FromBody] MyRequest request)
{
    var fileBytes = await _service.GenerateAsync(...);

    var filename = request.Filename ?? "document";
    if (!filename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        filename += ".pdf";

    // Sanitize: remove path separators and invalid filename characters
    var safeFilename = Path.GetInvalidFileNameChars()
        .Aggregate(filename, (current, c) => current.Replace(c, '_'));

    var filesDir = Path.Combine(_env.WebRootPath, "files");
    Directory.CreateDirectory(filesDir);

    var filePath = Path.Combine(filesDir, safeFilename);
    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

    var fileUrl = $"{Request.Scheme}://{Request.Host}/files/{safeFilename}";
    return Ok(new { url = fileUrl });
}
```

Key points:
- Sanitize the filename to prevent path traversal (`Path.GetInvalidFileNameChars()`)
- Store directly under `wwwroot/files/` — no unique ID prefix needed if user provides unique filenames

### 3. Create middleware for Content-Disposition

```csharp
// Middleware/MyFileMiddleware.cs
public class MyFileMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public MyFileMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
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
            await context.Response.WriteAsJsonAsync(new { error = "File not found" });
            return;
        }

        context.Response.ContentType = "application/pdf";

        var encodedFilename = Uri.EscapeDataString(fileName);
        var asciiFilename = string.Concat(
            Path.GetFileNameWithoutExtension(fileName).Select(c => c > 127 ? '_' : c)
        ) + ".pdf";

        context.Response.Headers.Append("Content-Disposition",
            $"attachment; filename=\"{asciiFilename}\"; filename*=UTF-8''{encodedFilename}");

        context.Response.Headers.Append("Content-Length", new System.IO.FileInfo(filePath).Length.ToString());
        await context.Response.SendFileAsync(filePath);
    }
}
```

### 4. Register in Program.cs

```csharp
app.UseStaticFiles();
app.UseMiddleware<MyFileMiddleware>();
```

Order matters: `UseStaticFiles()` first for general assets, then the custom middleware to intercept `/files/` paths.

## Content-Disposition format

```
Content-Disposition: attachment; filename="____.pdf"; filename*=UTF-8''%E6%B5%8B%E8%AF%95.pdf
```

- `filename` — ASCII-safe fallback (non-ASCII chars replaced with `_`)
- `filename*` — UTF-8 URL-encoded original, per RFC 5987

## Client behavior

The client does NOT need to set any special headers. It simply:
1. POSTs to the generation endpoint → gets back `{"url": "..."}`
2. GETs the URL (or opens it in a new tab) → browser downloads with correct filename
