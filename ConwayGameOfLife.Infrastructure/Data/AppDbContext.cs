using ConwayGameOfLife.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ConwayGameOfLife.Infrastructure.Data;

/// <summary>
/// Application database context
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the board states
    /// </summary>
    public DbSet<BoardState> BoardStates { get; set; } = null!;

    /// <summary>
    /// Creates a new instance of the application database context
    /// </summary>
    /// <param name="options">The database context options</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the model
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BoardState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Width).IsRequired();
            entity.Property(e => e.Height).IsRequired();
            entity.Property(e => e.SerializedGrid).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}