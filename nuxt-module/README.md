# nuxt-pdf-service

Nuxt module for [PlaywrightPdfService](https://github.com/your-org/PlayWrightPdfService) - convert any webpage to PDF.

## Installation

```bash
npm install nuxt-pdf-service
# or
yarn add nuxt-pdf-service
# or
pnpm add nuxt-pdf-service
```

## Setup

Add to your `nuxt.config.ts`:

```ts
export default defineNuxtConfig({
  modules: ['nuxt-pdf-service'],
  pdfService: {
    backendUrl: 'http://localhost:5050', // Your .NET backend URL
  },
})
```

## Usage

```vue
<script setup>
const { generatePdf, downloadPdf } = usePdf()

// Generate PDF and get the URL
const { url } = await generatePdf({
  url: 'https://example.com',
  filename: 'my-report',
})

// Or trigger browser download directly
await downloadPdf({
  url: 'https://example.com',
  filename: 'my-report',
})
</script>
```

## Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| backendUrl | string | `http://localhost:5050` | .NET backend service URL |
| routePrefix | string | `/api/pdf` | Local server route prefix |

## How It Works

```
┌─────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Nuxt App   │────▶│ Nuxt Server Route│────▶│ .NET Backend    │
│  (Client)   │     │ (Proxy /api/pdf) │     │ (Playwright)    │
└─────────────┘     └──────────────────┘     └─────────────────┘
                           │                          │
                           │                          ▼
                           │                   Save PDF to disk
                           │                          │
                           │                          ▼
                           │                  Return file URL
                           ▼                          │
                    Return URL to client ◀────────────┘
```

1. Client calls `/api/pdf/from-url` (local Nuxt server)
2. Nuxt server proxies request to .NET backend
3. Backend generates PDF, saves to disk, returns URL
4. Nuxt server returns URL to client
5. Client accesses the URL → browser downloads with correct filename (via `Content-Disposition`)

## License

MIT
