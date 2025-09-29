using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;
using TechXpress.DataAccess.Data;
using TechXpress.Entities.Repositories;
using TechXpress.Utilities;
using TexhXpress.DataAccess.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.Configure<StripeData>(builder.Configuration.GetSection("stripe"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1))
    .AddDefaultTokenProviders().AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<TechXpress.Utilities.IEmailSender, TechXpress.Utilities.EmailSender>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
// Define Authorization Policy for Admin role
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("stripe:Secretkey").Get<string>();

// Make sure to include authentication middleware
app.UseAuthentication(); // Ensure this line is added before authorization

app.UseAuthorization();
app.UseSession();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Admin}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "Customer",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
