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
 # 参照 Playwright 官方依赖清单 + Ubuntu Noble (24.04) t64 适配
 RUN sed -i 's|http://archive.ubuntu.com|https://mirrors.aliyun.com|g' /etc/apt/sources.list.d/ubuntu.sources && \
     sed -i 's|http://security.ubuntu.com|https://mirrors.aliyun.com|g' /etc/apt/sources.list.d/ubuntu.sources && \
     apt-get update && apt-get install -y --no-install-recommends \
     ca-certificates \
     fonts-liberation \
     libasound2t64 \
     libatk-bridge2.0-0t64 \
     libatk1.0-0t64 \
     libatspi2.0-0t64 \
     libc6 \
     libcairo2 \
     libcups2t64 \
     libcurl4 \
     libdbus-1-3 \
     libdrm2 \
     libexpat1 \
     libgbm1 \
     libglib2.0-0t64 \
     libgtk-3-0t64 \
     libnspr4 \
     libnss3 \
     libpango-1.0-0 \
     libudev1 \
     libvulkan1 \
     libx11-6 \
     libxcb1 \
     libxcomposite1 \
     libxdamage1 \
     libxext6 \
     libxfixes3 \
     libxkbcommon0 \
     libxrandr2 \
     wget \
     xdg-utils \
     && rm -rf /var/lib/apt/lists/*

 # 确保 PDF 存储目录存在
RUN mkdir -p /app/wwwroot/files

ENV PLAYWRIGHT_BROWSERS_PATH=/root/.cache/ms-playwright
ENV ASPNETCORE_URLS=http://+:5050
ENV ASPNETCORE_ENVIRONMENT=Production

 EXPOSE 5050

 ENTRYPOINT ["dotnet", "PlayWrightPdfService.dll"]
