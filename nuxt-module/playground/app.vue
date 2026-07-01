<template>
  <div class="container">
    <h1>Nuxt PDF Service - Playground</h1>

    <div class="form">
      <input v-model="url" type="text" placeholder="输入网页 URL" />
      <input v-model="filename" type="text" placeholder="文件名（可选）" />
      <button @click="handleGenerate" :disabled="loading">
        {{ loading ? '生成中...' : '生成 PDF' }}
      </button>
    </div>

    <div v-if="error" class="error">{{ error }}</div>

    <div v-if="pdfUrl" class="result">
      <p>PDF 已生成！</p>
      <a :href="pdfUrl" target="_blank">点击下载 PDF</a>
    </div>
  </div>
</template>

<script setup lang="ts">
const url = ref('https://example.com')
const filename = ref('测试文档')
const loading = ref(false)
const error = ref('')
const pdfUrl = ref('')

const { generatePdf } = usePdf()

async function handleGenerate() {
  if (!url.value) {
    error.value = '请输入 URL'
    return
  }

  loading.value = true
  error.value = ''
  pdfUrl.value = ''

  try {
    const result = await generatePdf({
      url: url.value,
      filename: filename.value || undefined,
    })
    pdfUrl.value = result.url
  } catch (e: any) {
    error.value = e.message || '生成失败'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.container {
  max-width: 600px;
  margin: 40px auto;
  padding: 20px;
  font-family: system-ui, sans-serif;
}
.form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
input {
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 14px;
}
button {
  padding: 12px;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
}
button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.error {
  color: #ef4444;
  margin-top: 12px;
}
.result {
  margin-top: 20px;
  padding: 16px;
  background: #f0fdf4;
  border-radius: 6px;
}
.result a {
  color: #16a34a;
  font-weight: 500;
}
</style>
