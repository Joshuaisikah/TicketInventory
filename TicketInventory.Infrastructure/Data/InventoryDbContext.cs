using Microsoft.EntityFrameworkCore;
using TicketInventory.Domain.Entities;

namespace TicketInventory.Infrastructure.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options) { }

    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(e =>
        {
            e.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(e => e.Sold).IsRequired().HasDefaultValue(0);
            e.HasMany(e => e.Orders)
                .WithOne(e => e.Event)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(e => e.Name).IsUnique();
        });
        modelBuilder.Entity<Order>(o =>
        {
            o.Property(o => o.Id).HasDefaultValueSql("gen_random_uuid()");
            o.Property(o => o.PurchasedAt).IsRequired().HasDefaultValueSql("now()");
            o.Property(o => o.Status)
                .HasConversion<string>()
                .HasDefaultValue(OrderStatus.Confirmed);
            o.HasIndex(o => o.EventId);
            o.HasIndex(o => o.PurchasedAt);
            o.HasIndex(o => o.Status);
        });
    }
}
