using DotNetCleanTemplate.Api;

var builder = WebApplication.CreateBuilder(args);
var bootstrapper = new ApplicationBootstrapper(builder);
var app = bootstrapper.InitializeConfiguration().ConfigureServices().Build();

var runner = new ApplicationRunner(app);
await runner.ConfigureMiddleware().MapEndpoints().RunAsync();
