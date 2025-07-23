# build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy only the necessary project files first for better layer caching
COPY source/Clients/DotNetCleanTemplate.WebClient/DotNetCleanTemplate.WebClient.csproj ./
COPY source/DotNetCleanTemplate.Shared/DotNetCleanTemplate.Shared.csproj ../DotNetCleanTemplate.Shared/
COPY source/DotNetCleanTemplate.Application/DotNetCleanTemplate.Application.csproj ../DotNetCleanTemplate.Application/
COPY source/DotNetCleanTemplate.Domain/DotNetCleanTemplate.Domain.csproj ../DotNetCleanTemplate.Domain/

# Restore dependencies
RUN dotnet restore DotNetCleanTemplate.WebClient.csproj

# Copy source code
COPY source/Clients/DotNetCleanTemplate.WebClient/ ./
COPY source/DotNetCleanTemplate.Shared/ ../DotNetCleanTemplate.Shared/
COPY source/DotNetCleanTemplate.Application/ ../DotNetCleanTemplate.Application/
COPY source/DotNetCleanTemplate.Domain/ ../DotNetCleanTemplate.Domain/

# Build and publish
RUN dotnet publish DotNetCleanTemplate.WebClient.csproj -c Release -o out

# runtime stage
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/out/wwwroot .
COPY source/Clients/DotNetCleanTemplate.WebClient/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"] 