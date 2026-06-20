using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NdlovuBetting.Data;
using NdlovuBetting.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1) Connect to SQL Server using the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
    maxRetryCount: 5,
    maxRetryDelay: TimeSpan.FromSeconds(10),
    errorNumbersToAdd: null);
    }));

// 2) Add ASP.NET Identity with roles1, storing users in our database
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 3) Where to send users who aren't logged in
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();


// Apply migrations + seed on startup with RETRIES
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<NdlovuBetting.Data.ApplicationDbContext>();

        // Retry 5 times with 2-second delays
        int retries = 0;
        while (retries < 5)
        {
            try
            {
                db.Database.Migrate();
                await DbSeeder.SeedAsync(scope.ServiceProvider);
                break; // Success!
            }
            catch (Exception) when (retries < 4)
            {
                retries++;
                await Task.Delay(2000); // Wait 2 seconds
            }
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database migrate/seed failed on startup. The app will still start.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// NdlovuBetting - Full GitOps Pipeline with GitHub Actions + ArgoCD
// Deployed to Kubernetes via automated CI/CD

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // must come BEFORE UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// NdlovuBetting - Full GitOps Pipeline with GitHub Actions + ArgoCD