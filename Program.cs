using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using ContractMonthlyClaimSystem.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// Configure services
// ==========================

// Add DbContext with SQL Server and retry logic
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
            sqlOptions.CommandTimeout(180); // 3 minutes
        }
    ));

// Add Identity with roles
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add MVC with Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ==========================
// Register application services
// ==========================
builder.Services.AddScoped<ClaimService>();
builder.Services.AddScoped<HRService>();
builder.Services.AddScoped<EmailNotificationService>();

// Register FluentValidation validators for role-based claim validation
builder.Services.AddValidatorsFromAssemblyContaining<ClaimValidator>();           // Lecturer
builder.Services.AddValidatorsFromAssemblyContaining<CoordinatorClaimValidator>(); // Coordinator
builder.Services.AddValidatorsFromAssemblyContaining<ManagerClaimValidator>();     // Manager

// Optional: configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// ==========================
// Build the app
// ==========================
var app = builder.Build();

// ==========================
// Apply pending migrations at startup
// ==========================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
        throw;
    }
}

// ==========================
// Configure middleware pipeline
// ==========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ==========================
// Map routes
// ==========================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();

app.Run();
