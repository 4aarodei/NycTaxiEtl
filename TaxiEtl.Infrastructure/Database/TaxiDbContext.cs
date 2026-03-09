using Microsoft.EntityFrameworkCore;
using TaxiEtl.Domain.Entities;

namespace TaxiEtl.Infrastructure.Database;

public sealed class TaxiDbContext : DbContext
{
    public DbSet<TaxiRide> TaxiRides => Set<TaxiRide>();

    public TaxiDbContext(DbContextOptions<TaxiDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaxiRide>(entity =>
        {
            entity.ToTable("TaxiRides");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .UseIdentityColumn();

            entity.Property(e => e.PickupDatetimeUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            entity.Property(e => e.DropoffDatetimeUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            entity.Property(e => e.PassengerCount)
                .IsRequired();

            entity.Property(e => e.TripDistance)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.StoreAndFwdFlag)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(e => e.PULocationID)
                .IsRequired();

            entity.Property(e => e.DOLocationID)
                .IsRequired();

            entity.Property(e => e.FareAmount)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.TipAmount)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            // Query: AVG tip_amount by PULocationID + search with PULocationID
            entity.HasIndex(e => e.PULocationID)
                .HasDatabaseName("IX_TaxiRides_PULocationID");

            // Query: TOP 100 longest fares by trip_distance
            entity.HasIndex(e => e.TripDistance)
                .IsDescending()
                .HasDatabaseName("IX_TaxiRides_TripDistance_Desc");

            // Query: TOP 100 longest fares by time spent traveling
            entity.HasIndex(e => new { e.PickupDatetimeUtc, e.DropoffDatetimeUtc })
                .HasDatabaseName("IX_TaxiRides_Pickup_Dropoff");
        });
    }
}