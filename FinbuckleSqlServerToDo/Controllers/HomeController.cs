using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FinbuckleSqlServerToDo.Models;
using FinbuckleSqlServerToDo.Data;
using Finbuckle.MultiTenant;

namespace FinbuckleSqlServerToDo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ToDoDbContext dbContext;

        public HomeController(ToDoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            // Get the list of to do items.
            // Note: This will only return items for the current tenant.
            IEnumerable<ToDoItem> toDoItems = null;
            if (HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo != null)
            {
                toDoItems = dbContext.ToDoItems.ToList();
            }

            return View(toDoItems);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
