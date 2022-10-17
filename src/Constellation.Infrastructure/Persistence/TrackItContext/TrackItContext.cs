using Constellation.Infrastructure.Persistence.TrackItContext.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Constellation.Infrastructure.Persistence.TrackItContext
{
    public partial class TrackItContext : DbContext
    {
        public TrackItContext(DbContextOptions<TrackItContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Models.Index> Indexes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(), t => t.GetTypeInfo().Namespace.Contains("TrackItContext"));

            //builder.ApplyConfiguration(new CustomerConfiguration());
            //builder.ApplyConfiguration(new DepartConfiguration());
            //builder.ApplyConfiguration(new LocationConfiguration());
            //builder.ApplyConfiguration(new SmsysrecnumConfiguration());

            base.OnModelCreating(builder);
        }
    }

}
