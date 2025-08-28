using Microsoft.EntityFrameworkCore;
using Reservas.Models;

namespace Reservas.Context
{
    public class BDContext : DbContext
    {
        public BDContext(DbContextOptions<BDContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<Center> Centers { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<ResourceType> ResourceTypes { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<ResourceValidator> ResourceValidators { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User (obligatorio)
            modelBuilder.Entity<ResourceValidator>()
                .HasOne(rv => rv.User)
                .WithMany()
                .HasForeignKey(rv => rv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Center (opcional)
            modelBuilder.Entity<ResourceValidator>()
                .HasOne(rv => rv.Center)
                .WithMany()
                .HasForeignKey(rv => rv.CenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Resource (opcional)
            modelBuilder.Entity<ResourceValidator>()
                .HasOne(rv => rv.Resource)
                .WithMany()
                .HasForeignKey(rv => rv.ResourceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice único compuesto
            modelBuilder.Entity<ResourceValidator>()
                .HasIndex(rv => new { rv.UserId, rv.CenterId, rv.ResourceId })
                .IsUnique();

            // Check: al menos CenterId o ResourceId
                modelBuilder.Entity<ResourceValidator>(entity =>
                {
                    entity.HasCheckConstraint(
                        "CK_ResourceValidator_Target",
                        "(CenterId IS NOT NULL) OR (ResourceId IS NOT NULL)");
                });


        }
    }
}