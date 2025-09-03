using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Reservas.Context;
using Reservas.Models;
using Reservas.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuración de EmailSettings desde appsettings.json
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// Registrar el servicio de envío de correos
builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();


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

app.UseRouting();

app.UseSession();          // 👈 Primero sesión
app.UseAuthentication();   // 👈 Luego autenticación
app.UseAuthorization();    // 👈 Y autorización

// 🔀 Rutas
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
  pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
