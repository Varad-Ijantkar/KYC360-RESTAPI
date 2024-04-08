using KYC360RESTAPI.Model;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Entity> Entities { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.OwnsMany(e => e.Addresses);
            entity.OwnsMany(e => e.Dates);
            entity.OwnsMany(e => e.Names);
        });
    }
}
