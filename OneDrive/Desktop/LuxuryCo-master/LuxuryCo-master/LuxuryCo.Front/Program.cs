using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;

// Enable Legacy Timestamp Behavior to prevent DateTime Kind errors when saving to PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session support (required by AdminController)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Authentication for the Frontend
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddDbContext<LuxuryCo.Database.Data.LuxuryCoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LuxuryCoDbConnection")));

// DataProtection: persist keys in DB so antiforgery tokens survive container restarts on Render
builder.Services.AddDataProtection()
    .SetApplicationName("LuxuryCo")
    .PersistKeysToDbContext<LuxuryCo.Database.Data.LuxuryCoDbContext>();

var app = builder.Build();

// Auto-apply any pending DB migrations at startup (creates DataProtectionKeys table if missing)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<LuxuryCo.Database.Data.LuxuryCoDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("DB migration on startup failed (may already be up to date): {Message}", ex.Message);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // Debe ir antes de Authentication y Authorization

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
