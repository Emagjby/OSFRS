using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OSFRS.Backend.Data;
using System;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OSFRSDbContext>
{
    public OSFRSDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OSFRSDbContext>();

        var conn = Environment.GetEnvironmentVariable("OSFRS_DB_CONN");
        if (string.IsNullOrEmpty(conn)) throw new InvalidOperationException("Enviroment variable OSFRS_DB_CONN not set.");

        optionsBuilder.UseNpgsql(conn);

        return new OSFRSDbContext(optionsBuilder.Options);
    }
}