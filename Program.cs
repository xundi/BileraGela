using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});


// 🔌 Conexión a la base de datos MySQL
var serverVersion = new MySqlServerVersion(new Version(8, 0, 27));
string connectionString = builder.Configuration.GetConnectionString("Connection");
Connect.ConnectionString = connectionString;
builder.Services.AddDbContext<BDContext>(options => options.UseMySql(connectionString, serverVersion));

// 🕒 Configuración de sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
});

// 🔐 Autenticación con cookies (Login y Logout)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

// Otros servicios
builder.Services.AddRazorPages();
builder.Services.AddMvc().AddRazorRuntimeCompilation();

var app = builder.Build();

// 📦 Middleware de la aplicación
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthentication(); // 🔐
app.UseAuthorization();

// 🔀 Rutas
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
  pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
