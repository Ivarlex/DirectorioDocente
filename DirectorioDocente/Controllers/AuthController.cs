using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DirectorioDocente.Data;
using DirectorioDocente.Helpers;
using DirectorioDocente.Models;

namespace DirectorioDocente.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToUserHome();

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nombreNorm = model.NombreUsuario.Trim().ToLower();

            var usuario = await _context.Usuarios
                .Include(u => u.Docente)
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreNorm && u.Activo);

            if (usuario == null || !PasswordHelper.VerifyPassword(nombreNorm, model.Password, usuario.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Matrícula o contraseña incorrecta. Verifica e intenta de nuevo.");
                return View(model);
            }

            var nombreMostrar = usuario.Docente?.NombreCompleto
                ?? (!string.IsNullOrWhiteSpace(usuario.NombreCompleto) ? usuario.NombreCompleto : "Director");
            var fotoMostrar = usuario.Docente?.FotoUrl ?? usuario.FotoUrl ?? "";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name,           usuario.NombreUsuario),
                new Claim(ClaimTypes.Role,           usuario.Rol.ToString()),
                new Claim("NombreCompleto",          nombreMostrar),
                new Claim("FotoUrl",                 fotoMostrar),
                new Claim("DocenteId",               usuario.DocenteId?.ToString() ?? ""),
                new Claim("RequiereCambioPassword",  usuario.RequiereCambioPassword.ToString())
            };

            var identity  = new ClaimsIdentity(claims, "SigedCookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("SigedCookie", principal, new AuthenticationProperties
            {
                IsPersistent  = model.Recordarme,
                ExpiresUtc    = model.Recordarme
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            });

            if (usuario.RequiereCambioPassword)
                return RedirectToAction("CambiarPassword", new { obligatorio = true });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToUserHome(usuario.Rol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("SigedCookie");
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Acceso() => View();

        [HttpGet]
        [Authorize]
        public IActionResult CambiarPassword(bool obligatorio = false)
        {
            return View(new CambiarPasswordViewModel { EsObligatorio = obligatorio });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var nombreUsuario = User.Identity!.Name!;
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

            if (usuario == null)
                return RedirectToAction("Login");

            if (!model.EsObligatorio)
            {
                if (!PasswordHelper.VerifyPassword(nombreUsuario, model.PasswordActual, usuario.PasswordHash))
                {
                    ModelState.AddModelError("PasswordActual", "La contraseña actual no es correcta.");
                    return View(model);
                }
            }

            if (model.PasswordNuevo.Trim().ToLower() == nombreUsuario)
            {
                ModelState.AddModelError("PasswordNuevo",
                    "La nueva contraseña no puede ser igual a tu matrícula. Elige una más segura.");
                return View(model);
            }

            usuario.PasswordHash         = PasswordHelper.HashPassword(nombreUsuario, model.PasswordNuevo);
            usuario.RequiereCambioPassword = false;
            await _context.SaveChangesAsync();

            var claims = User.Claims
                .Where(c => c.Type != "RequiereCambioPassword")
                .ToList();
            claims.Add(new Claim("RequiereCambioPassword", "False"));

            var identity  = new ClaimsIdentity(claims, "SigedCookie");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("SigedCookie", principal, new AuthenticationProperties
            {
                IsPersistent  = false,
                ExpiresUtc    = DateTimeOffset.UtcNow.AddHours(8)
            });

            TempData["Exito"] = "✓ Contraseña actualizada correctamente.";
            return RedirectToUserHome(usuario.Rol);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RestablecerPassword(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Docente)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
                return NotFound(new { success = false, message = "Usuario no encontrado." });

            var temporal = GenerarPasswordTemporal();

            usuario.PasswordHash           = PasswordHelper.HashPassword(usuario.NombreUsuario, temporal);
            usuario.RequiereCambioPassword = true;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success         = true,
                passwordTemporal = temporal,
                nombre          = usuario.Docente?.NombreCompleto ?? usuario.NombreUsuario,
                matricula       = usuario.NombreUsuario
            });
        }

        private IActionResult RedirectToUserHome(RolUsuario? rol = null)
        {
            var rolEfectivo = rol ?? (User.IsInRole("Administrador") ? RolUsuario.Administrador : RolUsuario.Docente);
            return rolEfectivo == RolUsuario.Administrador
                ? RedirectToAction("Index",          "Docentes")
                : RedirectToAction("MisActividades", "Docentes");
        }

        private static string GenerarPasswordTemporal()
        {
            const string chars  = "abcdefghjkmnpqrstuvwxyz23456789";
            var random  = new Random();
            return new string(Enumerable.Range(0, 8).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
    }
}
