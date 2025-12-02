using ChallengeAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ChallengeAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<PizzaType> PizzaTypes { get; set; }
        public DbSet<Pizza> Pizzas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure primary keys
            modelBuilder.Entity<PizzaType>().HasKey(pt => pt.Id);
            modelBuilder.Entity<Pizza>().HasKey(p => p.Id);
            modelBuilder.Entity<Order>().HasKey(o => o.Id);
            modelBuilder.Entity<OrderDetail>().HasKey(od => od.Id);

            // Configure relationships
            modelBuilder.Entity<Pizza>()
                .HasOne(p => p.PizzaType)
                .WithMany(pt => pt.Pizzas)
                .HasForeignKey(p => p.PizzaTypeId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Pizza)
                .WithMany()
                .HasForeignKey(od => od.PizzaId);
        }
    }
}
