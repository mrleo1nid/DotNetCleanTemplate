using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Health;

public class HealthCheckQuery : IRequest<HealthCheckResponseDto> { }
