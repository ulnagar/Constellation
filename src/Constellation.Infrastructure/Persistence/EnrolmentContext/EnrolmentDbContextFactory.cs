namespace Constellation.Infrastructure.Persistence.EnrolmentContext;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

/// <summary>
/// Allows EF Core design-time tools (migrations add/remove/update-database) to construct
/// EnrolmentDbContext directly, without building the full application host and its DI
/// container. This avoids design-time failures caused by unrelated service registration
/// issues elsewhere in the app (MediatR handlers, Scrutor scans, etc.) that have nothing
/// to do with the DbContext itself.
/// </summary>
public sealed class EnrolmentDbContextFactory : IDesignTimeDbContextFactory<EnrolmentDbContext>
{
    public EnrolmentDbContext CreateDbContext(string[] args)
    {
        string basePath = FindStartupProjectPath();

        string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddJsonFile($"appsettings.{environmentName.ToLowerInvariant()}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string? connectionString = configuration.GetConnectionString("EnrolmentConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Could not find an 'EnrolmentConnection' connection string. Looked for appsettings.json under '{basePath}'.");

        DbContextOptionsBuilder<EnrolmentDbContext> optionsBuilder = new();

        optionsBuilder.UseSqlServer(
            connectionString,
            b =>
            {
                b.MigrationsAssembly(typeof(EnrolmentDbContext).Assembly.FullName);
                b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

        return new EnrolmentDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Walks up from the current working directory (wherever `dotnet ef` was invoked from)
    /// looking for the Constellation.Presentation.Server project folder, so its appsettings.json
    /// can be used as the connection-string source regardless of where the command is run from.
    /// </summary>
    private static string FindStartupProjectPath()
    {
        DirectoryInfo? directory = new(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            string candidate = Path.Combine(directory.FullName, "src", "Constellation.Presentation.Server");

            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "appsettings.json")))
                return candidate;

            if (directory.Name == "Constellation.Presentation.Server" &&
                File.Exists(Path.Combine(directory.FullName, "appsettings.json")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the Constellation.Presentation.Server project directory to read appsettings.json for design-time migrations. " +
            "Run 'dotnet ef' from somewhere inside the repository, or set the EnrolmentConnection connection string via the " +
            "ConnectionStrings__EnrolmentConnection environment variable instead.");
    }
}