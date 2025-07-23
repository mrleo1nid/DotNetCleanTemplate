namespace DotNetCleanTemplate.Domain.Services
{
    public interface IInitDataService
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}
