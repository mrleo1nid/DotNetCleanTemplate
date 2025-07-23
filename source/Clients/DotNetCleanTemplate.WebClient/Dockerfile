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

# Set up nginx directories with proper permissions
RUN mkdir -p /var/cache/nginx /var/run /var/log/nginx /tmp && \
    chown -R nginx:nginx /var/cache/nginx /var/run /var/log/nginx /tmp && \
    chown -R nginx:nginx /etc/nginx/conf.d

WORKDIR /usr/share/nginx/html

# Copy files and set proper ownership
COPY --from=build /app/out/wwwroot .
COPY source/Clients/DotNetCleanTemplate.WebClient/nginx.conf /etc/nginx/nginx.conf

# Set proper permissions for web content
RUN chown -R nginx:nginx /usr/share/nginx/html && \
    chmod -R 755 /usr/share/nginx/html

# Switch to nginx user (already exists in nginx:alpine)
USER nginx

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"] 