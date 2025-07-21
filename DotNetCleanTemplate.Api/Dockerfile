# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DotNetCleanTemplate.Api/", "DotNetCleanTemplate.Api/"]
COPY ["DotNetCleanTemplate.Application/", "DotNetCleanTemplate.Application/"]
COPY ["DotNetCleanTemplate.Domain/", "DotNetCleanTemplate.Domain/"]
COPY ["DotNetCleanTemplate.Infrastructure/", "DotNetCleanTemplate.Infrastructure/"]
COPY ["DotNetCleanTemplate.Shared/", "DotNetCleanTemplate.Shared/"]
RUN dotnet restore "./DotNetCleanTemplate.Api/DotNetCleanTemplate.Api.csproj"
WORKDIR "/src/DotNetCleanTemplate.Api"
RUN dotnet build "./DotNetCleanTemplate.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DotNetCleanTemplate.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Явно копируем конфигурационные файлы
COPY ["DotNetCleanTemplate.Api/configs/", "/app/configs/"]
# Create a non-root user and switch to it
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser
ENTRYPOINT ["dotnet", "DotNetCleanTemplate.Api.dll"]