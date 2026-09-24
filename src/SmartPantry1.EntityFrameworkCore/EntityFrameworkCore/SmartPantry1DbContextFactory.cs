using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SmartPantry1.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class SmartPantry1DbContextFactory : IDesignTimeDbContextFactory<SmartPantry1DbContext>
{
    public SmartPantry1DbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        SmartPantry1EfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<SmartPantry1DbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new SmartPantry1DbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SmartPantry1.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
