using Microsoft.EntityFrameworkCore;
using NetCore1.Models;
using System.Diagnostics.Metrics;

namespace NetCore1.Services
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<MoscowWeatherData> MoscowWeatherData { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MoscowWeatherData>()
                .Property(m => m.Date)
                .HasColumnType("date");
        }
    }
}