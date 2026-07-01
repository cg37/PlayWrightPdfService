/**
 * Composable for generating PDFs via the nuxt-pdf-service module.
 * Automatically uses the local server proxy to avoid CORS issues.
 */
export function usePdf() {
  const config = useRuntimeConfig().public.pdfService as {
    routePrefix: string
  }

  /**
   * Generate a PDF from a URL and return the downloadable URL.
   */
  async function generatePdf(options: { url: string; filename?: string }) {
    const response = await $fetch<{ url: string }>(config.routePrefix + '/from-url', {
      method: 'POST',
      body: options,
    })
    return response
  }

  /**
   * Generate a PDF and trigger browser download.
   */
  async function downloadPdf(options: { url: string; filename?: string }) {
    const { url } = await generatePdf(options)

    // 直接打开 URL 让浏览器处理下载（服务端已设置 Content-Disposition）
    window.open(url, '_blank')
  }

  return {
    generatePdf,
    downloadPdf,
  }
}
