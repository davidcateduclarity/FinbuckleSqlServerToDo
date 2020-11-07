using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using FinbuckleSqlServerToDo.Data;
using FinbuckleSqlServerToDo.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinbuckleSqlServerToDo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Before running, setup the database if needed.
            SetupTenantStore(host);
            SetupToDoDb(host);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void SetupTenantStore(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var store = scope.ServiceProvider.GetRequiredService<TenantStoreDbContext>();

                if (!store.Database.CanConnect())
                    throw new Exception("Can't connect to tenant store database");

                // If no tenants exist, create the defaults.
                if (!store.TenantInfo.Any())
                {
                    var tenantConnectionString = "Data Source=Data/ToDoList.db";
                    store.TenantInfo.Add(new TenantInfo { Id = "megacorp", Identifier = "megacorp", Name = "MegaCorp", ConnectionString = tenantConnectionString });
                    store.TenantInfo.Add(new TenantInfo { Id = "initech", Identifier = "initech", Name = "Initech LLC", ConnectionString = tenantConnectionString });
                    store.TenantInfo.Add(new TenantInfo { Id = "acme", Identifier = "acme", Name = "ACME", ConnectionString = tenantConnectionString });
                    store.SaveChanges();
                }
            }
        }

        private static void SetupToDoDb(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var tenantStore = scope.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();

                var tenants = tenantStore.GetAllAsync().Result.ToList();

                // Note: this setup assumes there are three tenants defined in the tenant store.

                // First, clear out any existing databases so the sample always has a known state when running.
                foreach (var tenant in tenants)
                    using (var db = new ToDoDbContext(tenant))
                    {
                        db.Database.EnsureDeleted();
                    }


                using (var db = new ToDoDbContext(tenants[0]))
                {
                    db.Database.EnsureCreated();
                    db.ToDoItems.Add(new ToDoItem { Title = "Call Lawyer ", Completed = false });
                    db.ToDoItems.Add(new ToDoItem { Title = "File Papers", Completed = false });
                    db.ToDoItems.Add(new ToDoItem { Title = "Send Invoices", Completed = true });
                    db.SaveChanges();
                }

                using (var db = new ToDoDbContext(tenants[1]))
                {
                    db.Database.EnsureCreated();
                    db.ToDoItems.Add(new ToDoItem { Title = "Send Invoices", Completed = true });
                    db.ToDoItems.Add(new ToDoItem { Title = "Construct Additional Pylons", Completed = true });
                    db.ToDoItems.Add(new ToDoItem { Title = "Call Insurance Company", Completed = false });
                    db.SaveChanges();
                }

                using (var db = new ToDoDbContext(tenants[2]))
                {
                    db.Database.EnsureCreated();
                    db.ToDoItems.Add(new ToDoItem { Title = "Send Invoices", Completed = false });
                    db.ToDoItems.Add(new ToDoItem { Title = "Pay Salaries", Completed = true });
                    db.ToDoItems.Add(new ToDoItem { Title = "Write Memo", Completed = false });
                    db.SaveChanges();
                }
            }
        }
    }
}
