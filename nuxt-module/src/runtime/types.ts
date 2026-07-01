/**
 * Request body for PDF generation
 */
export interface PdfRequest {
  /** Target URL to convert */
  url: string
  /** Desired filename (without .pdf extension) */
  filename?: string
}

/**
 * Response from PDF generation API
 */
export interface PdfResponse {
  /** URL to download the generated PDF */
  url: string
}

/**
 * Return type of usePdf composable
 */
export interface UsePdfReturn {
  /**
   * Generate a PDF from a URL
   * @returns Promise with the downloadable PDF URL
   */
  generatePdf: (options: PdfRequest) => Promise<PdfResponse>
  /**
   * Generate a PDF and open in browser for download
   */
  downloadPdf: (options: PdfRequest) => Promise<void>
}
