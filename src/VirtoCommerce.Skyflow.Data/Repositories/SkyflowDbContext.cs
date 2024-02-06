using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.Skyflow.Data.Repositories;

public class SkyflowDbContext : DbContextWithTriggers
{
    public SkyflowDbContext(DbContextOptions<SkyflowDbContext> options)
        : base(options)
    {
    }

    protected SkyflowDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.Entity<SkyflowEntity>().ToTable("Skyflow").HasKey(x => x.Id);
        //modelBuilder.Entity<SkyflowEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
    }
}
