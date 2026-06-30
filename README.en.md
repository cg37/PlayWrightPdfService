# PlayWrightPdfService

> A PDF generation service powered by Playwright — convert any webpage to a downloadable PDF.

## Features

- 🌐 Convert any URL to PDF
- 📝 Custom filename support (including Chinese characters)
- 📦 Generated PDFs accessible via clean, direct URLs
- 🔽 Automatic `Content-Disposition` header for proper browser downloads
- 📚 Built-in Swagger/OpenAPI documentation

## Quick Start

### Prerequisites

- .NET 10.0+
- Playwright browser (installed automatically on first run)

### Installation & Running

```bash
# Restore dependencies
dotnet restore

# Install Playwright browsers
dotnet exec $(which playwright) install chromium
# Or: dotnet playwright install

# Run the service (default port: 5050)
dotnet run
```

Once started, visit: `http://localhost:5050`

## API Documentation

### Generate PDF

**POST** `/api/pdf/from-url`

**Request Body:**

```json
{
  "url": "https://example.com",
  "filename": "My Report"
}
```

| Parameter | Type   | Required | Description                    |
|-----------|--------|----------|--------------------------------|
| url       | string | ✅       | Target webpage URL             |
| filename  | string | ❌       | Desired filename (default: `document`) |

**Response:**

```json
{
  "url": "http://localhost:5050/files/My Report.pdf"
}
```

**Download the File:**

Simply access the returned URL:

```
GET /files/My Report.pdf
```

The response automatically includes:
```
Content-Disposition: attachment; filename="My Report.pdf"
```

## Project Structure

```
PlayWrightPdfService/
├── Controllers/
│   └── PdfController.cs      # API routes & request handling
├── Services/
│   └── PdfServices.cs        # Playwright PDF generation core logic
├── Middleware/
│   └── PdfFileMiddleware.cs  # File download middleware (Content-Disposition)
├── wwwroot/files/            # Generated PDF storage directory
├── Program.cs                # Application entry point & middleware config
└── appsettings.json          # Application configuration
```

## Configuration

### Port Configuration

Edit `appsettings.json`:

```json
{
  "Urls": "http://0.0.0.0:5050"
}
```

### PDF Generation Options

Adjust in `Services/PdfServices.cs`:

```csharp
var pdfOptions = new PagePdfOptions
{
    Format = "A4",           // Paper format
    PrintBackground = false, // Whether to print background graphics
    Margin = new Margin
    {
        Top = "0cm",
        Bottom = "0cm",
        Left = "0cm",
        Right = "0cm"
    }
};
```

## Client Examples

### cURL

```bash
curl -X POST http://localhost:5050/api/pdf/from-url \
  -H "Content-Type: application/json" \
  -d '{"url": "https://example.com", "filename": "My Report"}'
```

### JavaScript (Fetch)

```javascript
const response = await fetch('http://localhost:5050/api/pdf/from-url', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    url: 'https://example.com',
    filename: 'My Report'
  })
});

const { url } = await response.json();
// Access the URL directly to download
window.open(url, '_blank');
```

### Nuxt/Vue Composable

See `examples/nuxt-composable.ts`

## Tech Stack

- [.NET 10.0](https://dotnet.microsoft.com/)
- [Microsoft Playwright](https://playwright.dev/dotnet)
- [Swashbuckle (Swagger)](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

## License

MIT
