import { defineNuxtModule, addServerHandler, addImportsDir, createResolver, addTemplate } from '@nuxt/kit'

export interface ModuleOptions {
  /**
   * Backend .NET service URL
   * @default 'http://localhost:5050'
   */
  backendUrl?: string

  /**
   * API route prefix for the proxy server route
   * @default '/api/pdf'
   */
  routePrefix?: string
}

export default defineNuxtModule<ModuleOptions>({
  meta: {
    name: 'nuxt-pdf-service',
    configKey: 'pdfService',
  },
  defaults: {
    backendUrl: 'http://localhost:5050',
    routePrefix: '/api/pdf',
  },
  setup(options, nuxt) {
    const resolver = createResolver(import.meta.url)

    // 注入运行时配置
    nuxt.options.runtimeConfig.public.pdfService = {
      backendUrl: options.backendUrl!,
      routePrefix: options.routePrefix!,
    }

    // 注册 server route 代理
    addServerHandler({
      route: `${options.routePrefix}/**`,
      method: 'post',
      handler: resolver.resolve('./runtime/server/routes/pdf/proxy'),
    })

    // 注册 composables 自动导入
    addImportsDir(resolver.resolve('./runtime/composables'))

    // 生成类型定义
    addTemplate({
      filename: 'types/pdf-service.d.ts',
      getContents: () => `
declare module '#app' {
  interface RuntimeConfig {
    pdfService: {
      backendUrl: string
      routePrefix: string
    }
  }
}
export {}
`,
    })

    nuxt.hook('nitro:config', (config) => {
      config.typescript = config.typescript || {}
      config.typescript.tsConfig = config.typescript.tsConfig || {}
      config.typescript.tsConfig.include = config.typescript.tsConfig.include || []
    })
  },
})
