using GestionCommerciale.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Infrastructure.Persistence;

public class CommercialDbContext : DbContext
{
    public CommercialDbContext(DbContextOptions<CommercialDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100);
            entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasIndex(x => x.Reference).IsUnique();
            entity.Property(x => x.Reference).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasIndex(x => x.OrderNumber).IsUnique();
            entity.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.TotalHT).HasPrecision(18, 2);
            entity.Property(x => x.TotalTTC).HasPrecision(18, 2);
            entity.Property(x => x.OrderDate)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(x => x.Client)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.ToTable("OrderLines");
            entity.HasIndex(x => new { x.OrderId, x.ProductId }).IsUnique();
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal)
                .HasPrecision(18, 2)
                .HasComputedColumnSql(
                    "CONVERT(DECIMAL(18,2), [Quantity] * [UnitPrice])",
                    stored: true);

            entity.HasOne(x => x.Order)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.OrderLines)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
