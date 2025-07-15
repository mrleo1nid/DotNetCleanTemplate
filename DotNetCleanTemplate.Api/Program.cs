namespace DotNetCleanTemplate.Api
{
    public partial class Program
    {
        protected Program() { }

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var bootstrapper = new ApplicationBootstrapper(builder);
            var app = bootstrapper.InitializeConfiguration().ConfigureServices().Build();

            var runner = new ApplicationRunner(app);
            await runner.ConfigureMiddleware().MapEndpoints().RunAsync();
        }
    }
}
