// Necessário para registrar a política de autorização de ADM
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
// AdminRequirement, AdminHandler e Politicas (Authorization/AdminRequirement.cs)
using Sistema.Authorization;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
// PedidoService (registra vendas/compras) e UsuarioLogadoService (usuário logado)
using Sistema.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Regras de senha e bloqueio da conta. O padrão do Identity aceita senha de 6 caracteres
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Confirmação de e-mail continua desligada: não há servidor de e-mail configurado
    options.SignIn.RequireConfirmedAccount = false;
    // Senha de no mínimo 8 caracteres
    options.Password.RequiredLength = 8;
    // Após 5 senhas erradas, a conta fica bloqueada por 5 minutos
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
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

// Políticas de venda, compra e desempenho do vendedor (ADM também passa nas de venda e compra)
builder.Services.AddScoped<IAuthorizationHandler, FuncaoHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Politicas.Vendedor, policy =>
        policy.RequireAuthenticatedUser().AddRequirements(new FuncaoRequirement(Politicas.FuncaoAdminId, Politicas.FuncaoVendedorId)))
    .AddPolicy(Politicas.Padrao, policy =>
        policy.RequireAuthenticatedUser().AddRequirements(new FuncaoRequirement(Politicas.FuncaoAdminId, Politicas.FuncaoPadraoId)))
    .AddPolicy(Politicas.SomenteVendedor, policy =>
        policy.RequireAuthenticatedUser().AddRequirements(new FuncaoRequirement(Politicas.FuncaoVendedorId)));

// Serviços de venda/compra, busca do comprador por CPF/código e de usuário logado
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<BuscaCompradorService>();
builder.Services.AddScoped<UsuarioLogadoService>();



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

// Cabeçalhos de segurança em todas as respostas: impedem que o sistema seja aberto dentro de um iframe
// (clickjacking), que o navegador "adivinhe" o tipo de arquivo e que a URL vaze para outros sites
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    // Política de conteúdo: scripts só do próprio site (bloqueia script injetado por XSS).
    // Estilos inline ficam liberados porque algumas views usam style="..."; as fontes vêm do Google Fonts.
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; img-src 'self' data:; form-action 'self'; " +
        "base-uri 'self'; frame-ancestors 'none'";
    await next();
});

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
