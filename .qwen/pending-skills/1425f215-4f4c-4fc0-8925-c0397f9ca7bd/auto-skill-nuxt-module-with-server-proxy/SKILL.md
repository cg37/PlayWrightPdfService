---
name: nuxt-module-with-server-proxy
description: Pattern for creating a Nuxt module that registers server routes as a proxy to an external backend, with auto-imported composables
source: auto-skill
extracted_at: '2026-06-30T10:49:15.273Z'
---

# Nuxt Module with Server Route Proxy

When wrapping an external backend API as a Nuxt module, use server routes as a proxy to avoid CORS issues. Users interact with local routes only.

## Why

- External backends (e.g., .NET, Python, Go services) run on different origins
- Client-side calls would require CORS configuration on the backend
- A Nuxt server route proxy eliminates CORS entirely — the frontend talks to its own server
- Auto-imported composables give users a seamless `useXxx()` experience

## Directory Structure

```
nuxt-module/
├── package.json
├── src/
│   ├── module.ts                    # Module entry point
│   └── runtime/
│       ├── composables/
│       │   └── useXxx.ts            # Auto-imported composable
│       ├── server/
│       │   └── routes/
│       │       └── xxx/
│       │           └── proxy.ts     # Server route handler
│       └── types.ts                 # Shared TypeScript types
└── playground/                      # Local dev test app
```

## Steps

### 1. package.json

```json
{
  "name": "nuxt-my-module",
  "type": "module",
  "exports": {
    ".": {
      "types": "./dist/types.d.ts",
      "import": "./dist/module.mjs",
      "require": "./dist/module.cjs"
    }
  },
  "main": "./dist/module.cjs",
  "types": "./dist/types.d.ts",
  "scripts": {
    "build": "nuxt-module-build build",
    "dev": "nuxi dev playground",
    "dev:prepare": "nuxt-module-build --stub && nuxi prepare playground"
  },
  "dependencies": {
    "@nuxt/kit": "^3.15.0"
  },
  "devDependencies": {
    "@nuxt/module-builder": "^0.8.4",
    "nuxt": "^3.15.0"
  }
}
```

### 2. module.ts — Register server routes, composables, and runtime config

```ts
import { defineNuxtModule, addServerHandler, addImportsDir, createResolver } from '@nuxt/kit'

export interface ModuleOptions {
  backendUrl?: string
  routePrefix?: string
}

export default defineNuxtModule<ModuleOptions>({
  meta: {
    name: 'nuxt-my-module',
    configKey: 'myModule',
  },
  defaults: {
    backendUrl: 'http://localhost:5050',
    routePrefix: '/api/proxy',
  },
  setup(options, nuxt) {
    const resolver = createResolver(import.meta.url)

    // Inject runtime config for composable + server route to read
    nuxt.options.runtimeConfig.public.myModule = {
      backendUrl: options.backendUrl!,
      routePrefix: options.routePrefix!,
    }

    // Register server route as proxy
    addServerHandler({
      route: `${options.routePrefix}/**`,
      method: 'post',
      handler: resolver.resolve('./runtime/server/routes/proxy'),
    })

    // Auto-import composables
    addImportsDir(resolver.resolve('./runtime/composables'))
  },
})
```

### 3. Server route proxy handler

```ts
// runtime/server/routes/proxy.ts
import { defineEventHandler, readBody } from 'h3'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig(event).public.myModule as {
    backendUrl: string
  }

  const body = await readBody(event)

  // Forward to external backend
  const response = await $fetch(`${config.backendUrl}/api/endpoint`, {
    method: 'POST',
    body,
  })

  return response
})
```

### 4. Composable (auto-imported)

```ts
// runtime/composables/useXxx.ts
export function useXxx() {
  const config = useRuntimeConfig().public.myModule as {
    routePrefix: string
  }

  async function callBackend(options: { url: string; filename?: string }) {
    return $fetch(config.routePrefix + '/from-url', {
      method: 'POST',
      body: options,
    })
  }

  return { callBackend }
}
```

### 5. User experience

```ts
// nuxt.config.ts
export default defineNuxtConfig({
  modules: ['nuxt-my-module'],
  myModule: {
    backendUrl: 'http://localhost:5050',
  },
})
```

```vue
<script setup>
// No import needed — auto-imported by the module
const { callBackend } = useXxx()

const result = await callBackend({
  url: 'https://example.com',
  filename: 'report',
})
</script>
```

## Key points

- `addServerHandler` with `/**` wildcard captures sub-routes — the handler reads the full path if needed
- Runtime config is available in both composables (client-safe public config) and server routes (via `useRuntimeConfig(event)`)
- `addImportsDir` makes composables available globally without explicit imports
- For production npm publishing: add `files: ["dist"]` to package.json and build with `@nuxt/module-builder`
