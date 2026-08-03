using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MnemoToad.Data;

namespace MnemoToad.Tests.TestSupport;

internal sealed class MockedDbWebApplicationFactory : WebApplicationFactory<Program>
{
    public MockableAppDbContext Db { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAppDbContext>();
            services.AddSingleton<IAppDbContext>(Db);
        });
    }
}
