namespace DotNetCleanTemplate.Domain.Decorators
{
    public interface IInitDataServiceDecorator
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}
