# DotNetCleanTemplate.Shared

Shared DTOs, models, and common utilities for DotNetCleanTemplate projects.

## Features

- **Common DTOs**: Request and response DTOs for authentication, user management, and health checks
- **Result Pattern**: Generic Result<T> type for consistent error handling
- **Error Types**: Standardized error types and error handling utilities
- **Clean Architecture Support**: Shared models that follow Clean Architecture principles

## Installation

```bash
dotnet add package DotNetCleanTemplate.Shared
```

## Usage

### DTOs

```csharp
using DotNetCleanTemplate.Shared.DTOs;

// Authentication DTOs
var loginRequest = new LoginRequestDto 
{
    Email = "user@example.com",
    Password = "password123"
};

var loginResponse = new LoginResponseDto 
{
    AccessToken = "jwt-token",
    RefreshToken = "refresh-token",
    ExpiresIn = 3600
};

// User management DTOs
var registerUser = new RegisterUserDto 
{
    UserName = "john_doe",
    Email = "john@example.com",
    Password = "password123"
};
```

### Result Pattern

```csharp
using DotNetCleanTemplate.Shared.Common;

// Success result
var successResult = Result<UserDto>.Success(userDto);

// Error result
var errorResult = Result<UserDto>.Failure(
    Error.Validation("Invalid email format")
);

// Check result
if (result.IsSuccess)
{
    var user = result.Value;
    // Use user data
}
else
{
    var error = result.Error;
    // Handle error
}
```

### Error Handling

```csharp
using DotNetCleanTemplate.Shared.Common;

// Create different types of errors
var validationError = Error.Validation("Field is required");
var notFoundError = Error.NotFound("User not found");
var conflictError = Error.Conflict("User already exists");
var unauthorizedError = Error.Unauthorized("Invalid credentials");
```

## Dependencies

- MediatR.Contracts (2.0.1)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 