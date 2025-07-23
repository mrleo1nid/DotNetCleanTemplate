# build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish source/Clients/DotNetCleanTemplate.WebClient/DotNetCleanTemplate.WebClient.csproj -c Release -o out

# runtime stage
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/out/wwwroot .
COPY source/Clients/DotNetCleanTemplate.WebClient/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"] 