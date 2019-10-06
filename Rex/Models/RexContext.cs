using Microsoft.EntityFrameworkCore;
namespace Rex.Models
{
    public class RexContext : DbContext
    {
        public RexContext(DbContextOptions<RexContext> options) : base(options)
        {

        }

        public DbSet<Collection> Collections { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasKey(u => u.PrincipalId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.DefaultCollection)
                .WithOne()
                .HasPrincipalKey(nameof(Collection), nameof(Collection.Id))
                .HasForeignKey(nameof(User), nameof(User.PrincipalId));

            modelBuilder.Entity<Collection>()
                .HasMany(c => c.Ideas)
                .WithOne()
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoleAssignment>()
                .HasKey(r => new { r.CollectionId, r.UserId });

            modelBuilder.Entity<Collection>()
                .HasMany(c => c.RoleAssignments)
                .WithOne()
                .HasForeignKey(r => r.CollectionId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.RoleAssignments)
                .WithOne()
                .HasForeignKey(r => r.UserId);
        }
    }
}