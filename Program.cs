using PlayWrightPdfService.Middleware;
using PlayWrightPdfService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<PdfServices>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 启用静态文件（wwwroot）
app.UseStaticFiles();

// 注册 PDF 文件中间件，处理 /files/ 路径的 Content-Disposition
app.UseMiddleware<PdfFileMiddleware>();

app.Run();