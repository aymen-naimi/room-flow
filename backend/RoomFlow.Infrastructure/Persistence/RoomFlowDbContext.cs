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

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.Email).HasMaxLength(256);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.PasswordHash).HasMaxLength(500);
            entity.Property(user => user.FirstName).HasMaxLength(100);
            entity.Property(user => user.LastName).HasMaxLength(100);
        });
    }
}
