using DotNetCleanTemplate.Domain.Decorators;
using DotNetCleanTemplate.Domain.Services;

namespace DotNetCleanTemplate.Application.Services
{
    public class ApplicationInitDataService : IInitDataServiceDecorator
    {
        private readonly IInitDataService _initDataService;

        public ApplicationInitDataService(IInitDataService initDataService)
        {
            _initDataService = initDataService;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await _initDataService.InitializeAsync(cancellationToken);
        }
    }
}
