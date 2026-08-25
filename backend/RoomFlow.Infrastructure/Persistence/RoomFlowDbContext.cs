using Microsoft.EntityFrameworkCore;
using RoomFlow.Domain.Entities;

namespace RoomFlow.Infrastructure.Persistence;

public class RoomFlowDbContext : DbContext
{
    public RoomFlowDbContext(DbContextOptions<RoomFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(entity =>
        {
            entity.Property(room => room.Name).HasMaxLength(200);
            entity.Property(room => room.Location).HasMaxLength(200);
            entity.HasIndex(room => room.Name).IsUnique();
            entity.Property(room => room.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });
    }
}
