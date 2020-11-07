This app demonstrates using Finbuckle.MultiTenant in a basic ToDo List app.
The application uses Sql Server for the tenant store via EFCoreStore and SQLite
for the ToDo List data. I generally recommend using seperate databases/contexts
for the tenant store and the application data, but it can be combined if needed.

The tenant store configured is an EFCore store that connects to a SQL Server
instance using the connection string in the appsettings.json file. You’ll need
to set this accordingly then run the approprate migration commands to set up the
scheme on the database:

dotnet ef migrations add Initial -c FinbuckleSqlServerToDo.Data.TenantStoreDbContext
dotnet ef database update -c FinbuckleSqlServerToDo.Data.TenantStoreDbContext

Identity is setup to use the default UI based on Razor Pages. This also requires
modifications to the Razor Pages routing conventions which are in
`ConfigureServices`. It also requires a custom link generator implementation
which is included and its setup is also in `ConfigureServices`. Identity uses
its own db context and is configured to use a shared SQLite database, but this
can be changed to another database or be per-tenant database by using a custom
tenant property for its Identity connection string. The Identity database(s) are
not seeded and need to be migrated with the commands below:

dotnet ef migrations add Initial -c FinbuckleSqlServerToDo.Areas.Identity.Data.FinbuckleSqlServerToDoIdentityDbContext
dotnet ef database update -c FinbuckleSqlServerToDo.Areas.Identity.Data.FinbuckleSqlServerToDoIdentityDbContext

When the app runs the tenant store is seeded in the SetupTenantStore method in
Program.cs. This method will only seed the tenants if non are found. You can
modify them in the database to see the effect on the application when restarting it.

Per-tenant app data is seeded in the SetupToDoDb method in Program.cs. This will
reset the tenant app data (to do list items) every time the sample is run. This
data is house in SQLite databases, but could be changed. Currently all tenants
will share the same database but you can modify this so that they use different
databases by changing the tenant connection string in the tenant store.
Migrations are not used since the database is recreated from scratch each run,
but they could be used if desired.