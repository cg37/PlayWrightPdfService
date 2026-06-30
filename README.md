# PlayWrightPdfService

> 基于 Playwright 的 PDF 生成服务 —— 将任意网页转换为可下载的 PDF 文件。

## 功能特性

- 🌐 通过 URL 将网页转换为 PDF
- 📝 支持自定义文件名（含中文）
- 📦 生成的 PDF 可通过简洁 URL 直接访问
- 🔽 自动设置 `Content-Disposition` 响应头，浏览器下载时保留原始文件名
- 📚 内置 Swagger/OpenAPI 文档

## 快速开始

### 环境要求

- .NET 10.0+
- Playwright 浏览器（首次运行会自动安装）

### 安装与运行

```bash
# 还原依赖
dotnet restore

# 安装 Playwright 浏览器
dotnet exec $(which playwright) install chromium
# 或使用: dotnet playwright install

# 运行服务（默认端口 5050）
dotnet run
```

服务启动后访问：`http://localhost:5050`

## API 文档

### 生成 PDF

**POST** `/api/pdf/from-url`

**请求体：**

```json
{
  "url": "https://example.com",
  "filename": "测试文档"
}
```

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| url | string | ✅ | 目标网页 URL |
| filename | string | ❌ | 期望的文件名（默认 `document`） |

**响应：**

```json
{
  "url": "http://localhost:5050/files/测试文档.pdf"
}
```

**下载文件：**

直接访问返回的 URL 即可下载：

```
GET /files/测试文档.pdf
```

响应头会自动设置：
```
Content-Disposition: attachment; filename="____.pdf"; filename*=UTF-8''%E6%B5%8B%E8%AF%95%E6%96%87%E6%A1%A3.pdf
```

## 项目结构

```
PlayWrightPdfService/
├── Controllers/
│   └── PdfController.cs      # API 路由与请求处理
├── Services/
│   └── PdfServices.cs        # Playwright PDF 生成核心逻辑
├── Middleware/
│   └── PdfFileMiddleware.cs  # 文件下载中间件（Content-Disposition）
├── wwwroot/files/            # 生成的 PDF 存储目录
├── Program.cs                # 应用入口与中间件配置
└── appsettings.json          # 应用配置
```

## 配置

### 端口设置

编辑 `appsettings.json`：

```json
{
  "Urls": "http://0.0.0.0:5050"
}
```

### PDF 生成选项

在 `Services/PdfServices.cs` 中可调整：

```csharp
var pdfOptions = new PagePdfOptions
{
    Format = "A4",           // 纸张格式
    PrintBackground = false, // 是否打印背景
    Margin = new Margin
    {
        Top = "0cm",
        Bottom = "0cm",
        Left = "0cm",
        Right = "0cm"
    }
};
```

## 前端调用示例

### cURL

```bash
curl -X POST http://localhost:5050/api/pdf/from-url \
  -H "Content-Type: application/json" \
  -d '{"url": "https://example.com", "filename": "报告"}'
```

### JavaScript (Fetch)

```javascript
const response = await fetch('http://localhost:5050/api/pdf/from-url', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    url: 'https://example.com',
    filename: '测试文档'
  })
});

const { url } = await response.json();
// 直接访问 URL 下载
window.open(url, '_blank');
```

### Nuxt/Vue Composable

参见 `examples/nuxt-composable.ts`

## 技术栈

- [.NET 10.0](https://dotnet.microsoft.com/)
- [Microsoft Playwright](https://playwright.dev/dotnet)
- [Swashbuckle (Swagger)](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

## License

MIT
