using Aptabase.IntegrationTests.Clients;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Aptabase.IntegrationTests;

public class IntegrationTestsFixture : IAsyncLifetime, IDisposable
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    public AccountClient UserA { get; private set; } = null!;
    public AccountClient UserB { get; private set; } = null!;

    public IntegrationTestsFixture()
    {
        _factory = new CustomWebApplicationFactory<Program>();
    }

    public HttpClient CreateClient()
    {
        var opts = new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        };
        return _factory.CreateClient(opts);
    }

    /// <summary>
    /// Creates a brand-new registered account with its own cookie session. Use this for
    /// tests that mutate or delete the account so they don't disturb the shared UserA/UserB.
    /// </summary>
    public async Task<AccountClient> CreateFreshAccountAsync(string name)
    {
        var account = new AccountClient(CreateClient(), CreateClient);
        await account.CreateAccount(name, $"{name.Replace(' ', '.').ToLowerInvariant()}.{Guid.NewGuid()}@example.com");
        return account;
    }

    public T GetHostedService<T>() where T : notnull
    {
        using var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetServices<IHostedService>().OfType<T>().Single();
    }

    public T GetService<T>() where T : notnull
    {
        using var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    public async Task InitializeAsync()
    {
        UserA = new AccountClient(CreateClient(), CreateClient);
        await UserA.CreateAccount("Jon Snow", $"jon.snow.{Guid.NewGuid()}@got.com");

        UserB = new AccountClient(CreateClient(), CreateClient);
        await UserB.CreateAccount("Arya Stark", $"arya.stark.{Guid.NewGuid()}@got.com");
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}


[CollectionDefinition("Integration Tests")]
public class IntegrationTests : ICollectionFixture<IntegrationTestsFixture>
{
    
}