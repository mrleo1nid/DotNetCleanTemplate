#!/bin/bash

# Build script for Clean Architecture API Template

set -e

CONFIGURATION=${1:-Release}
VERSION=${2:-1.0.0}
INSTALL_LOCAL=${3:-false}

echo "Building Clean Architecture API Template..."

# Clean
echo "Cleaning..."
rm -rf ./bin ./obj ./nupkg

# Restore
echo "Restoring packages..."
dotnet restore "./DotNetCleanTemplate.sln"

# Build
echo "Building solution..."
dotnet build "./DotNetCleanTemplate.sln" --configuration $CONFIGURATION --no-restore

# Test
echo "Running tests..."
dotnet test "./DotNetCleanTemplate.sln" --configuration $CONFIGURATION --no-build --no-restore

# Pack template
echo "Packing template..."
dotnet pack "./.template.config/templatepack.csproj" --configuration $CONFIGURATION --output "./nupkg" --no-build --no-restore --include-symbols

if [ "$INSTALL_LOCAL" = "true" ]; then
    echo "Installing template locally..."
    NUPKG_PATH=$(find ./nupkg -name "*.nupkg" | head -1)
    if [ -n "$NUPKG_PATH" ]; then
        dotnet new --install "$NUPKG_PATH"
        echo "Template installed successfully!"
    fi
fi

echo "Build completed!" 