 namespace PlayWrightPdfService.Middleware;

 /// <summary>
 /// 防御性中间件：验证请求是否通过了 Cloudflare Access 认证。
 /// Cloudflare Access 在边缘验证用户身份后，会注入
 /// Cf-Access-Authenticated-User-Email 请求头。
 /// 如果请求缺少该头（且不是 API Key 调试请求），直接拒绝。
 ///
 /// 双重安全：
 ///   1. Cloudflare Access 在边缘做身份认证
 ///   2. 本中间件确保只有经由 Cloudflare 的请求才能到达业务代码
 /// </summary>
 public class CfAccessValidationMiddleware
 {
     private readonly RequestDelegate _next;
     private readonly string? _apiKey;
     private readonly ILogger<CfAccessValidationMiddleware> _logger;

     // Cloudflare Access 认证通过后注入的请求头名
     private const string CfUserEmailHeader = "Cf-Access-Authenticated-User-Email";

     public CfAccessValidationMiddleware(
         RequestDelegate next,
         IConfiguration configuration,
         ILogger<CfAccessValidationMiddleware> logger)
     {
         _next = next;
         _apiKey = configuration["PDF_SERVICE_API_KEY"];
         _logger = logger;
     }

     public async Task InvokeAsync(HttpContext context)
     {
         // ── 放行路径 ──────────────────────────────────
         // 健康检查端点不需要认证
         var path = context.Request.Path.Value ?? "";
         if (path.Equals("/health", StringComparison.OrdinalIgnoreCase) ||
             path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
         {
             await _next(context);
             return;
         }

         // ── 检查 Cloudflare Access 认证头 ─────────────
         var cfEmail = context.Request.Headers[CfUserEmailHeader].FirstOrDefault();
         if (!string.IsNullOrWhiteSpace(cfEmail))
         {
             _logger.LogInformation("Cloudflare Access 认证通过: {Email}", cfEmail);
             await _next(context);
             return;
         }

         // ── 可选：API Key 调试模式 ────────────────────
         if (!string.IsNullOrWhiteSpace(_apiKey))
         {
             var requestKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
             if (requestKey == _apiKey)
             {
                 await _next(context);
                 return;
             }
         }

         // ── 拒绝 ──────────────────────────────────────
         _logger.LogWarning("未经认证的请求被拒绝: {Method} {Path} from {RemoteIp}",
             context.Request.Method, path, context.Connection.RemoteIpAddress);

         context.Response.StatusCode = 401;
         context.Response.ContentType = "application/json";
         await context.Response.WriteAsJsonAsync(new
         {
             error = "unauthorized",
             message = "This service is private. Access via Cloudflare Zero Trust or provide a valid API key."
         });
     }
 }
