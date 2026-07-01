import { defineEventHandler, readBody, getRouterParams } from 'h3'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig(event).public.pdfService as {
    backendUrl: string
  }

  const body = await readBody(event)
  const params = getRouterParams(event)

  // 转发到 .NET 后端
  const backendUrl = `${config.backendUrl}/api/pdf/from-url`

  const response = await $fetch(backendUrl, {
    method: 'POST',
    body,
  })

  return response
})
