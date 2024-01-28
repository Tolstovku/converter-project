using Microsoft.EntityFrameworkCore;

namespace Common.Database;

public class AppDbContext : DbContext
{
    public DbSet<FileEntity> Files { get; set; }

    protected AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FileEntity>().ToTable("files");
    }
}