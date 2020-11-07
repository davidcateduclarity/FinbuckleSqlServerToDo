using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;

namespace FinbuckleSqlServerToDo.Data
{
    public class TenantStoreDbContext : EFCoreStoreDbContext<TenantInfo>
    {
        public TenantStoreDbContext(DbContextOptions<TenantStoreDbContext> options) : base(options)
        {
        }
    }
}
