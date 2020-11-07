using FinbuckleSqlServerToDo.Models;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinbuckleSqlServerToDo.Data
{
    public class ToDoDbContext : MultiTenantDbContext
    {

        public DbSet<ToDoItem> ToDoItems { get; set; }


        public ToDoDbContext(ITenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        public ToDoDbContext(ITenantInfo tenantInfo, DbContextOptions<ToDoDbContext> options) : base(tenantInfo, options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // We'll use the tenant's specific connection string.
            optionsBuilder.UseSqlite(TenantInfo.ConnectionString);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Designate ToDoItem as a multitenant entitty.
            // This adds a hidden column for the tenant id and automatically
            // filters requests for the current tenant id (subject to the
            // limitations of an Entity Framework Core global query filters
            // described here:
            // https://docs.microsoft.com/en-us/ef/core/querying/filters#limitations
            modelBuilder.Entity<ToDoItem>().IsMultiTenant();

            base.OnModelCreating(modelBuilder);
        }

    }
}