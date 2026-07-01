export default defineNuxtConfig({
  modules: ['../src/module'],
  pdfService: {
    backendUrl: 'http://localhost:5050',
  },
  devtools: { enabled: true },
})
