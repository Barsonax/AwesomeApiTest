﻿using Api.MsSql.Sut;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;
using TestExamplesDotnet;
using TestExamplesDotnet.Mssql;

namespace Api.MsSql.Nunit;

public class MigrationTests
{
#pragma warning disable NUnit1032
    private MsSqlContainer _databaseContainer = null!;
    private ILogger<MigrationTests> _logger = null!;
#pragma warning restore NUnit1032
    private AsyncServiceScope _scope;

    [SetUp]
    public void BeforeTestCase()
    {
        _scope = GlobalSetup.Provider.CreateAsyncScope();
        _databaseContainer = _scope.ServiceProvider.GetRequiredService<MsSqlContainer>();
        _logger = _scope.ServiceProvider.GetRequiredService<ILogger<MigrationTests>>();
    }

    [TearDown]
    public async Task AfterTestCase()
    {
        await _scope.DisposeAsync();
    }

    [Test]
    public async Task MigrationsUpAndDown_NoErrors()
    {
        await using DbContext context = new BloggingContext(new DbContextOptionsBuilder<BloggingContext>().UseSqlServer().Options);
        var migrations = context.GenerateMigrationScripts();

        var databaseName = "MigrationsTest";
        await _databaseContainer.CreateDatabase(databaseName);
        var migrator = new SqlMigrator(_databaseContainer, _logger, databaseName);
        foreach (var migration in migrations)
        {
            await migrator.Up(migration);
        }

        foreach (var migration in migrations.Reverse())
        {
            await migrator.Down(migration);
        }
    }
}
