param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0",
    [switch]$InstallLocal,
    [switch]$UninstallLocal
)

Write-Host "Building Clean Architecture API Template..." -ForegroundColor Green

# Clean
Write-Host "Cleaning..." -ForegroundColor Yellow
if (Test-Path "./bin") { Remove-Item "./bin" -Recurse -Force }
if (Test-Path "./obj") { Remove-Item "./obj" -Recurse -Force }
if (Test-Path "./nupkg") { Remove-Item "./nupkg" -Recurse -Force }

# Restore
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore "./DotNetCleanTemplate.sln"

# Build
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build "./DotNetCleanTemplate.sln" --configuration $Configuration --no-restore

# Test
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test "./DotNetCleanTemplate.sln" --configuration $Configuration --no-build --no-restore

# Pack template
Write-Host "Packing template..." -ForegroundColor Yellow
dotnet pack "./.template.config/templatepack.csproj" --configuration $Configuration --output "./nupkg" --no-build --no-restore --include-symbols

if ($InstallLocal) {
    Write-Host "Installing template locally..." -ForegroundColor Yellow
    $nupkgPath = Get-ChildItem "./nupkg/*.nupkg" | Select-Object -First 1
    if ($nupkgPath) {
        dotnet new --install $nupkgPath.FullName
        Write-Host "Template installed successfully!" -ForegroundColor Green
    }
}

if ($UninstallLocal) {
    Write-Host "Uninstalling template..." -ForegroundColor Yellow
    dotnet new --uninstall "DotNetCleanTemplate"
    Write-Host "Template uninstalled successfully!" -ForegroundColor Green
}

Write-Host "Build completed!" -ForegroundColor Green 