 # =============================================================================
 # Stage 1: Build + download Chromium
 # =============================================================================
 FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

 # Restore with project file only for layer caching
 COPY *.csproj .
 RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish -c Release -o /app

# Install Playwright CLI and download Chromium
# 设备默认 Playwright CDN，无国内网络限制时直接下载
RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PATH="$PATH:/root/.dotnet/tools"
# playwright install 需要在有 .csproj 的目录下运行，以读取 Playwright 版本
RUN playwright install chromium

 # =============================================================================
 # Stage 2: Runtime (lean aspnet image + Chromium from build stage)
 # =============================================================================
 FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

 # Copy published app
 COPY --from=build /app .

# Copy Playwright browsers (downloaded in build stage)
COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright

 # Chromium 系统依赖（使用阿里云镜像加速国内下载）
 RUN sed -i 's|http://archive.ubuntu.com|https://mirrors.aliyun.com|g' /etc/apt/sources.list.d/ubuntu.sources && \
     sed -i 's|http://security.ubuntu.com|https://mirrors.aliyun.com|g' /etc/apt/sources.list.d/ubuntu.sources && \
     apt-get update && apt-get install -y --no-install-recommends \
     libnss3 \
     libnspr4 \
     libatk1.0-0t64 \
     libatk-bridge2.0-0t64 \
     libcups2t64 \
     libdrm2 \
     libdbus-1-3 \
     libxkbcommon0 \
     libxcomposite1 \
     libxdamage1 \
     libxrandr2 \
     libgbm1 \
     libpango-1.0-0 \
     libcairo2 \
     libasound2t64 \
     && rm -rf /var/lib/apt/lists/*

 # 确保 PDF 存储目录存在
RUN mkdir -p /app/wwwroot/files

ENV PLAYWRIGHT_BROWSERS_PATH=/root/.cache/ms-playwright
ENV ASPNETCORE_URLS=http://+:5050
ENV ASPNETCORE_ENVIRONMENT=Production

 EXPOSE 5050

 ENTRYPOINT ["dotnet", "PlayWrightPdfService.dll"]
