using Microsoft.EntityFrameworkCore;
using DirectorioDocente.Data;
using DirectorioDocente.Models;
using DirectorioDocente.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("SigedCookie")
    .AddCookie("SigedCookie", options =>
    {
        options.LoginPath        = "/Auth/Login";
        options.LogoutPath       = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Acceso";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name      = "siged_session";
        options.Cookie.HttpOnly  = true;
        options.Cookie.SameSite  = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (!db.Usuarios.Any(u => u.NombreUsuario == "director"))
    {
        db.Usuarios.Add(new Usuario
        {
            NombreUsuario = "director",
            PasswordHash  = PasswordHelper.HashPassword("director", "director2024"),
            Rol           = RolUsuario.Administrador,
            DocenteId     = null,
            FechaCreacion = DateTime.Now,
            Activo        = true
        });
        db.SaveChanges();
    }
    else
    {
        var admin = db.Usuarios.First(u => u.NombreUsuario == "director" && u.Rol == RolUsuario.Administrador);
        var correctHash = PasswordHelper.HashPassword("director", "director2024");
        if (!string.Equals(admin.PasswordHash, correctHash, StringComparison.OrdinalIgnoreCase))
        {
            admin.PasswordHash = correctHash;
            db.SaveChanges();
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
