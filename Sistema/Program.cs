// Necessário para registrar a política de autorização de ADM
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
// AdminRequirement, AdminHandler e Politicas (Authorization/AdminRequirement.cs)
using Sistema.Authorization;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    //AddSignInManager()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
// Separado em outra linha (antes estava colado no AddDefaultTokenProviders)
builder.Services.AddControllersWithViews();

// Política de ADM: apenas usuários com FuncaoId = 1
builder.Services.AddScoped<IAuthorizationHandler, AdminHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Politicas.Admin, policy =>
        policy.RequireAuthenticatedUser().AddRequirements(new AdminRequirement()));
});





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
