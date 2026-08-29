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
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(entity =>
        {
            entity.Property(room => room.Name).HasMaxLength(200);
            entity.Property(room => room.Location).HasMaxLength(200);
            entity.HasIndex(room => room.Name).IsUnique();
            entity.Property(room => room.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(room => room.CreatedBy)
                .WithMany(user => user.CreatedRooms)
                .HasForeignKey(room => room.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.Email).HasMaxLength(256);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.PasswordHash).HasMaxLength(500);
            entity.Property(user => user.FirstName).HasMaxLength(100);
            entity.Property(user => user.LastName).HasMaxLength(100);
            entity.Property(user => user.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(booking => booking.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(booking => booking.Room)
                .WithMany(room => room.Bookings)
                .HasForeignKey(booking => booking.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(booking => booking.User)
                .WithMany(user => user.Bookings)
                .HasForeignKey(booking => booking.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(token => token.TokenHash).HasMaxLength(88);
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
