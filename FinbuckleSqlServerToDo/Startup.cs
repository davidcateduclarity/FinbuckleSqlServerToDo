using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using FinbuckleSqlServerToDo.Areas.Identity.Data;
using FinbuckleSqlServerToDo.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FinbuckleSqlServerToDo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // Register the ToDo db context, but do not specify a provider/connection string since
            // these vary by tenant.
            services.AddDbContext<ToDoDbContext>();

            // Configure Identity
            services.AddRazorPages(options =>
            {
                // Since we are using the route multitenant strategy we must add the
                // route parameter to the Pages conventions used by Identity.
                options.Conventions.AddAreaFolderRouteModelConvention("Identity", "/Account", model =>
                {
                    foreach (var selector in model.Selectors)
                    {
                        selector.AttributeRouteModel.Template =
                            AttributeRouteModel.CombineTemplates("{__tenant__}", selector.AttributeRouteModel.Template);
                    }
                });
            });

            // Preserve the tenant route param when new links are generated.
            services.DecorateService<LinkGenerator, AmbientValueLinkGenerator>(new List<string> { "__tenant__" });
            services.AddDbContext<FinbuckleSqlServerToDoIdentityDbContext>(options =>
                    options.UseSqlite(Configuration.GetConnectionString("FinbuckleSqlServerToDoIdentityDbContextConnection")));

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<FinbuckleSqlServerToDoIdentityDbContext>();

            // Register the tenant store db context.
            services.AddDbContext<TenantStoreDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("TenantStoreConnectionString"));
            });

            // Configure Finbuckle, the store db context does not need to be
            // added separately.
            // Also note this must come after Identity configuration.
            services.AddMultiTenant<TenantInfo>()
                    .WithEFCoreStore<TenantStoreDbContext, TenantInfo>()
                    .WithRouteStrategy()
                    .WithPerTenantAuthentication();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();


            // Add Finbuckle.MultiTenant to the app pipeline.
            // Note the tenant route parameter configured below.
            app.UseMultiTenant();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{__tenant__=}/{controller=Home}/{action=Index}");
                endpoints.MapRazorPages();
            });
        }
    }
}
