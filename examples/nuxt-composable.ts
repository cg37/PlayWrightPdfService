// composables/usePdfService.ts

/**
 * Composable for calling the PlaywrightPdfService backend
 */
export function usePdfService() {
  // Change this to your actual backend URL
  const baseUrl = useRuntimeConfig().public.pdfServiceUrl || 'http://localhost:5050'

  /**
   * Generate a PDF from a URL and return it as a Blob
   */
  async function generatePdfFromUrl(url: string): Promise<Blob> {
    const response = await $fetch(`${baseUrl}/api/pdf/from-url`, {
      method: 'POST',
      body: { url },
      responseType: 'blob' as const,
    })
    return response as Blob
  }

  /**
   * Generate a PDF from a URL and trigger download in the browser
   */
  async function downloadPdf(url: string, filename = 'document.pdf') {
    const blob = await generatePdfFromUrl(url)
    const blobUrl = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = blobUrl
    a.download = filename
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(blobUrl)
  }

  return {
    generatePdfFromUrl,
    downloadPdf,
  }
}
