# NuGet Package Publishing Guide

This document describes how to publish the DotNetCleanTemplate.Shared package to NuGet.

## Prerequisites

1. **NuGet API Key**: You need a NuGet API key to publish packages
   - Go to [NuGet.org](https://www.nuget.org)
   - Sign in to your account
   - Go to "API Keys" section
   - Create a new API key

2. **GitHub Secrets**: Add your NuGet API key to GitHub repository secrets
   - Go to your GitHub repository
   - Navigate to Settings → Secrets and variables → Actions
   - Create a new repository secret named `NUGET_API_KEY`
   - Paste your NuGet API key as the value

## Publishing Process

### 1. Update Version

Before creating a new release, update the version in the project file:

```xml
<PropertyGroup>
    <Version>1.0.1</Version>
</PropertyGroup>
```

### 2. Create and Push Tag

Create a new Git tag with the version number (prefixed with 'v'):

```bash
git tag v1.0.1
git push origin v1.0.1
```

### 3. Automatic Publishing

The GitHub Actions workflow will automatically:

1. Trigger when a tag starting with 'v' is pushed
2. Extract the version number from the tag
3. Build the DotNetCleanTemplate.Shared project
4. Create a NuGet package with the extracted version
5. Publish the package to NuGet.org

### 4. Verify Publication

After the workflow completes successfully:

1. Check the [NuGet.org](https://www.nuget.org) website
2. Search for "DotNetCleanTemplate.Shared"
3. Verify the new version is available

## Workflow Details

The workflow file `.github/workflows/nuget-publish.yml` contains:

- **Trigger**: Pushes to tags matching pattern 'v*'
- **Environment**: Ubuntu latest
- **Steps**:
  1. Checkout code
  2. Setup .NET 9.0 SDK
  3. Extract version from tag
  4. Restore dependencies
  5. Build project
  6. Pack into NuGet package
  7. Publish to NuGet.org

## Package Information

- **Package ID**: DotNetCleanTemplate.Shared
- **Description**: Shared DTOs, models, and common utilities for DotNetCleanTemplate projects
- **Tags**: clean-architecture, ddd, cqrs, mediatr, dotnet, template
- **License**: MIT
- **Repository**: GitHub repository URL

## Troubleshooting

### Common Issues

1. **Authentication Failed**: Ensure `NUGET_API_KEY` secret is correctly set in GitHub
2. **Version Already Exists**: Make sure the version number is unique
3. **Build Failures**: Check that all dependencies are properly restored

### Manual Publishing

If you need to publish manually:

```bash
# Restore dependencies
dotnet restore DotNetCleanTemplate.Shared/DotNetCleanTemplate.Shared.csproj

# Build
dotnet build DotNetCleanTemplate.Shared/DotNetCleanTemplate.Shared.csproj --configuration Release

# Pack
dotnet pack DotNetCleanTemplate.Shared/DotNetCleanTemplate.Shared.csproj --configuration Release --output ./nupkgs

# Publish (replace YOUR_API_KEY with actual key)
dotnet nuget push ./nupkgs/*.nupkg --api-key YOUR_API_KEY --source nuget.org
```

## Versioning Strategy

We follow [Semantic Versioning](https://semver.org/) (SemVer):

- **MAJOR.MINOR.PATCH**
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

Example: v1.2.3
- 1 = Major version
- 2 = Minor version  
- 3 = Patch version 