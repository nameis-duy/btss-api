using Domain.Entities;
using Domain.JsonEntities;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Account> Account { get; set; }
        public DbSet<Announcement> Announcement { get; set; }
        public DbSet<Destination> Destination { get; set; }
        public DbSet<DestinationComment> DestinationComment { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<Plan> Plan { get; set; }
        public DbSet<PlanMember> PlanMember { get; set; }
        public DbSet<PlanSavedProvider> PlanSavedProvider { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Provider> Provider { get; set; }
        public DbSet<Province> Province { get; set; }
        public DbSet<StatisticalData> StatisticalData { get; set; }
        public DbSet<Transaction> Transaction { get; set; }

        //public DbSet<PlanRating> PlanRating { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //optionsBuilder.LogTo(Console.WriteLine).EnableSensitiveDataLogging();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DestinationConfig).Assembly);

            modelBuilder.HasPostgresExtension("citext");
            modelBuilder.HasPostgresExtension("unaccent");
            modelBuilder.HasPostgresExtension("pg_trgm");

        }
    }
}
