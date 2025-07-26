namespace DotNetCleanTemplate.Domain.Services
{
    /// <summary>
    /// Интерфейс для чтения данных из кэша
    /// </summary>
    public interface ICacheReader
    {
        Task<T> GetOrCreateAsync<T>(
            string key,
            string? region,
            Func<Task<T>> factory,
            CancellationToken cancellationToken
        );
    }

    /// <summary>
    /// Интерфейс для инвалидации данных в кэше
    /// </summary>
    public interface ICacheInvalidator
    {
        void Invalidate(string key);
        void InvalidateRegion(string region);
    }

    /// <summary>
    /// Полный интерфейс кэш-сервиса, объединяющий чтение и инвалидацию
    /// </summary>
    public interface ICacheService : ICacheReader, ICacheInvalidator { }
}
