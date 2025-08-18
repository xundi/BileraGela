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

            // Puedes añadir configuraciones adicionales si lo deseas:
           // modelBuilder.Entity<User>()
            //    .HasIndex(u => u.Dni)
             //   .IsUnique(); // Evita usuarios duplicados por DNI

            // Relaciones opcionales explícitas (normalmente EF las infiere)
            modelBuilder.Entity<Resource>()
                .HasOne(r => r.Center)
                .WithMany(c => c.Resources)
                .HasForeignKey(r => r.CenterId);

            modelBuilder.Entity<Resource>()
                .HasOne(r => r.ResourceType)
                .WithMany(rt => rt.Resources)
                .HasForeignKey(r => r.ResourceTypeId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserType)
                .WithMany(ut => ut.Users)
                .HasForeignKey(u => u.UserTypeId);
        }
    }
}
