using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DirectorioDocente.Data;
using DirectorioDocente.Helpers;
using DirectorioDocente.Models;

namespace DirectorioDocente.Controllers
{
    [Authorize]
    public class DocentesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly HashSet<string> ExtPermitidas =
            new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".xlsx", ".xls" };

        private const long MaxTamanoBytes = 10 * 1024 * 1024;

        public DocentesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env     = env;
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index(string? buscar, string? tipoContrato)
        {
            var query = _context.Docentes
                .Include(d => d.Actividades)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var b = buscar.Trim().ToLower();
                query = query.Where(d =>
                    d.NombreCompleto.ToLower().Contains(b) ||
                    d.ClaveDocente.ToLower().Contains(b) ||
                    d.CorreoInstitucional.ToLower().Contains(b));
            }

            if (!string.IsNullOrWhiteSpace(tipoContrato) && tipoContrato != "Todos")
                query = query.Where(d => d.TipoContrato == tipoContrato);

            ViewBag.Buscar       = buscar;
            ViewBag.TipoContrato = tipoContrato ?? "Todos";

            var docentes = await query.OrderBy(d => d.NombreCompleto).ToListAsync();

            var hoy = DateTime.Now;
            var todasActividades = docentes.SelectMany(d => d.Actividades).ToList();
            var total     = docentes.Count;
            var conRetraso = docentes.Count(d =>
                d.Actividades.Any(a => a.Estatus != "Cumplido" && a.FechaLimite < hoy));
            var alCorriente = total - conRetraso;
            var cumplimiento = todasActividades.Count == 0
                ? 100
                : (int)(todasActividades.Count(a => a.Estatus == "Cumplido") * 100.0 / todasActividades.Count);

            ViewBag.TotalDocentes    = total;
            ViewBag.ConRetraso       = conRetraso;
            ViewBag.AlCorriente      = alCorriente;
            ViewBag.Cumplimiento     = cumplimiento;
            ViewBag.TotalActividades = todasActividades.Count;

            var idsDocentes = docentes.Select(d => d.Id).ToList();
            var usuariosMap = await _context.Usuarios
                .Where(u => u.DocenteId != null && idsDocentes.Contains(u.DocenteId!.Value))
                .ToDictionaryAsync(u => u.DocenteId!.Value, u => u.Id);
            ViewBag.UsuarioIds = usuariosMap;

            return View(docentes);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create(Docente docente)
        {
            if (ModelState.IsValid)
            {
                bool existe = await _context.Docentes.AnyAsync(d => d.ClaveDocente == docente.ClaveDocente);
                if (existe)
                {
                    ModelState.AddModelError("ClaveDocente", "Esta matrícula ya está registrada.");
                    return View(docente);
                }

                _context.Add(docente);
                await _context.SaveChangesAsync();

                var usuario = new Usuario
                {
                    NombreUsuario = docente.ClaveDocente.Trim().ToLower(),
                    PasswordHash  = PasswordHelper.HashPassword(
                        docente.ClaveDocente.Trim().ToLower(),
                        docente.ClaveDocente.Trim()),
                    Rol       = RolUsuario.Docente,
                    DocenteId = docente.Id,
                    Activo    = true,
                    RequiereCambioPassword = true
                };
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                TempData["Exito"] = $"Docente '{docente.NombreCompleto}' registrado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(docente);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id)
        {
            var docente = await _context.Docentes.FindAsync(id);
            if (docente == null) return NotFound();
            return View(docente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, Docente docente)
        {
            if (id != docente.Id) return NotFound();

            if (ModelState.IsValid)
            {
                bool duplicada = await _context.Docentes
                    .AnyAsync(d => d.ClaveDocente == docente.ClaveDocente && d.Id != docente.Id);

                if (duplicada)
                {
                    ModelState.AddModelError("ClaveDocente", "Esta matrícula ya está en uso por otro docente.");
                    return View(docente);
                }

                var existente = await _context.Docentes.FindAsync(id);
                if (existente == null) return NotFound();

                existente.ClaveDocente        = docente.ClaveDocente.Trim();
                existente.NombreCompleto      = docente.NombreCompleto.Trim();
                existente.CorreoInstitucional = docente.CorreoInstitucional.Trim();
                existente.Telefono            = docente.Telefono.Trim();
                existente.TipoContrato        = docente.TipoContrato;
                existente.Activo              = docente.Activo;

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Exito"] = "Datos administrativos del docente actualizados correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Docentes.AnyAsync(d => d.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(docente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var docente = await _context.Docentes.FindAsync(id);
            if (docente != null)
            {
                _context.Docentes.Remove(docente);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Docente eliminado del directorio.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AsignarActividades([FromBody] AsignarActividadViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Datos del formulario incompletos." });

            var idsDestino = new List<int>();

            if (model.ModoAsignacion == "todos")
            {
                idsDestino = await _context.Docentes.Select(d => d.Id).ToListAsync();
            }
            else if (model.ModoAsignacion == "tiempo-completo")
            {
                idsDestino = await _context.Docentes
                    .Where(d => d.TipoContrato == "Tiempo Completo")
                    .Select(d => d.Id).ToListAsync();
            }
            else if (model.ModoAsignacion == "asignatura")
            {
                idsDestino = await _context.Docentes
                    .Where(d => d.TipoContrato == "Asignatura")
                    .Select(d => d.Id).ToListAsync();
            }
            else if (model.ModoAsignacion == "individual" && model.DocenteId.HasValue)
            {
                idsDestino.Add(model.DocenteId.Value);
            }
            else
            {
                return BadRequest(new { success = false, message = "Selecciona un docente para la asignación individual." });
            }

            if (!idsDestino.Any())
                return BadRequest(new { success = false, message = "No se encontraron docentes para esta asignación." });

            var nuevas = idsDestino.Select(id => new ActividadDocente
            {
                DocenteId       = id,
                Titulo          = model.Titulo,
                Categoria       = model.Categoria,
                FechaAsignacion = DateTime.Now,
                FechaLimite     = model.FechaLimite,
                Estatus         = "Pendiente"
            }).ToList();

            _context.ActividadesDocentes.AddRange(nuevas);
            await _context.SaveChangesAsync();

            return Json(new { success = true, total = nuevas.Count });
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Tablero(int? anio, int? mes)
        {
            var todasActividades = await _context.ActividadesDocentes
                .Include(a => a.Docente)
                .Include(a => a.Documentos)
                .OrderByDescending(a => a.FechaLimite)
                .ToListAsync();

            var mesesDisponibles = todasActividades
                .Select(a => new { a.FechaLimite.Year, a.FechaLimite.Month })
                .Distinct()
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .Select(m => new MesItem
                {
                    Anio     = m.Year,
                    Mes      = m.Month,
                    Etiqueta = new DateTime(m.Year, m.Month, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-MX"))
                })
                .ToList();

            ViewBag.MesesDisponibles = mesesDisponibles;
            ViewBag.AnioSeleccionado  = anio;
            ViewBag.MesSeleccionado   = mes;

            var actividadesFiltradas = todasActividades.AsEnumerable();
            if (anio.HasValue && mes.HasValue)
            {
                actividadesFiltradas = actividadesFiltradas.Where(a => a.FechaLimite.Year == anio.Value && a.FechaLimite.Month == mes.Value);
            }

            return View(actividadesFiltradas.ToList());
        }

        public class MesItem
        {
            public int Anio { get; set; }
            public int Mes { get; set; }
            public string Etiqueta { get; set; } = string.Empty;
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarEstadoActividad([FromBody] ActualizarEstadoDTO model)
        {
            var actividad = await _context.ActividadesDocentes.FindAsync(model.Id);
            if (actividad == null)
                return NotFound(new { success = false, message = "Actividad no encontrada" });

            actividad.Estatus = model.NuevoEstado;
            actividad.FechaCumplimiento = model.NuevoEstado == "Cumplido" ? DateTime.Now : null;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public class ActualizarEstadoDTO
        {
            public int Id { get; set; }
            public string NuevoEstado { get; set; } = string.Empty;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetDetalleActividad(int id)
        {
            var act = await _context.ActividadesDocentes
                .Include(a => a.Docente)
                .Include(a => a.Documentos)
                .Include(a => a.Comentarios)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (act == null)
                return NotFound(new { success = false, message = "Actividad no encontrada." });

            if (User.IsInRole("Docente"))
            {
                var matricula = User.Identity?.Name ?? "";
                if (act.Docente == null || act.Docente.ClaveDocente.ToLower() != matricula.ToLower())
                    return Forbid();
            }

            var docs = act.Documentos.Select(d => new
            {
                id                = d.Id,
                nombreOriginal    = d.NombreOriginal,
                tamano            = $"{d.TamanoBytes / 1024} KB",
                contentType       = d.ContentType,
                esPrevisualizable = d.ContentType.StartsWith("image/") || d.ContentType == "application/pdf",
                previewUrl        = Url.Action("PreviewDocumento", "Docentes", new { id = d.Id }),
                downloadUrl       = Url.Action("DescargarDocumento", "Docentes", new { id = d.Id })
            }).ToList();

            var comentarios = act.Comentarios
                .OrderBy(c => c.FechaEnvio)
                .Select(c => new
                {
                    id          = c.Id,
                    autorNombre = c.AutorNombre,
                    rolAutor    = c.RolAutor,
                    mensaje     = c.Mensaje,
                    fecha       = c.FechaEnvio.ToString("dd/MM/yyyy HH:mm")
                }).ToList();

            return Json(new
            {
                success            = true,
                id                 = act.Id,
                titulo             = act.Titulo,
                categoria          = act.Categoria,
                estatus            = act.Estatus,
                docenteNombre      = act.Docente?.NombreCompleto ?? "Sin docente asignado",
                docenteClave       = act.Docente?.ClaveDocente ?? "S/M",
                docenteContrato    = act.Docente?.TipoContrato ?? "N/A",
                fechaAsignacion    = act.FechaAsignacion.ToString("dd/MM/yyyy"),
                fechaLimite        = act.FechaLimite.ToString("dd/MM/yyyy"),
                fechaCumplimiento  = act.FechaCumplimiento?.ToString("dd/MM/yyyy HH:mm"),
                fechaRevision      = act.FechaRevision?.ToString("dd/MM/yyyy HH:mm"),
                comentarioDirector = act.ComentarioDirector ?? string.Empty,
                documentos         = docs,
                comentarios        = comentarios
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RevisarActividad([FromBody] RevisarActividadDTO model)
        {
            var act = await _context.ActividadesDocentes
                .Include(a => a.Docente)
                .FirstOrDefaultAsync(a => a.Id == model.ActividadId);

            if (act == null)
                return NotFound(new { success = false, message = "Actividad no encontrada." });

            var comentarioTexto = model.Comentario?.Trim();
            if (!string.IsNullOrWhiteSpace(comentarioTexto))
            {
                act.ComentarioDirector = comentarioTexto;
                act.FechaRevision      = DateTime.Now;

                _context.ComentariosActividades.Add(new ComentarioActividad
                {
                    ActividadDocenteId = act.Id,
                    AutorNombre        = "Dirección Académica",
                    RolAutor           = "Administrador",
                    Mensaje            = comentarioTexto,
                    FechaEnvio         = DateTime.Now
                });
            }

            if (model.Accion == "aprobar")
            {
                act.Estatus           = "Cumplido";
                act.FechaCumplimiento ??= DateTime.Now;
            }
            else if (model.Accion == "regresar")
            {
                act.Estatus = "Pendiente";
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, estatus = act.Estatus, comentario = act.ComentarioDirector });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AgregarComentario([FromBody] AgregarComentarioDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Mensaje))
                return BadRequest(new { success = false, message = "El mensaje no puede estar vacío." });

            var act = await _context.ActividadesDocentes
                .Include(a => a.Docente)
                .FirstOrDefaultAsync(a => a.Id == dto.ActividadId);

            if (act == null)
                return NotFound(new { success = false, message = "Actividad no encontrada." });

            var esAdmin = User.IsInRole("Administrador");
            var matricula = User.Identity?.Name ?? "";

            if (!esAdmin)
            {
                if (act.Docente == null || act.Docente.ClaveDocente.ToLower() != matricula.ToLower())
                    return Forbid();
            }

            var autorNombre = esAdmin ? "Dirección Académica" : (act.Docente?.NombreCompleto ?? matricula);
            var rolAutor    = esAdmin ? "Administrador" : "Docente";

            var comentario = new ComentarioActividad
            {
                ActividadDocenteId = act.Id,
                AutorNombre        = autorNombre,
                RolAutor           = rolAutor,
                Mensaje            = dto.Mensaje.Trim(),
                FechaEnvio         = DateTime.Now
            };

            _context.ComentariosActividades.Add(comentario);

            if (esAdmin)
            {
                act.ComentarioDirector = comentario.Mensaje;
                act.FechaRevision      = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success    = true,
                comentario = new
                {
                    id          = comentario.Id,
                    autorNombre = comentario.AutorNombre,
                    rolAutor    = comentario.RolAutor,
                    mensaje     = comentario.Mensaje,
                    fecha       = comentario.FechaEnvio.ToString("dd/MM/yyyy HH:mm")
                }
            });
        }

        public class RevisarActividadDTO
        {
            public int ActividadId { get; set; }
            public string Accion { get; set; } = "comentar";
            public string? Comentario { get; set; }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarActividad(int id)
        {
            var act = await _context.ActividadesDocentes
                .Include(a => a.Documentos)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (act == null)
                return NotFound(new { success = false, message = "Actividad no encontrada." });

            foreach (var doc in act.Documentos)
            {
                var ruta = Path.Combine(_env.WebRootPath, doc.RutaRelativa.Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(ruta))
                    System.IO.File.Delete(ruta);
            }

            _context.ActividadesDocentes.Remove(act);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DescargarReporteMensual(int? anio, int? mes)
        {
            var anioFiltro = anio ?? DateTime.Now.Year;
            var mesFiltro  = mes  ?? DateTime.Now.Month;

            var fechaInicio = new DateTime(anioFiltro, mesFiltro, 1);
            var fechaFin    = fechaInicio.AddMonths(1);

            var actividades = await _context.ActividadesDocentes
                .Include(a => a.Docente)
                .Include(a => a.Documentos)
                .Where(a => (a.FechaLimite >= fechaInicio && a.FechaLimite < fechaFin) ||
                            (a.FechaAsignacion >= fechaInicio && a.FechaAsignacion < fechaFin))
                .OrderBy(a => a.Docente!.NombreCompleto)
                .ThenBy(a => a.FechaLimite)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ID Actividad,Matrícula,Docente,Contrato,Título,Categoría,Fecha Asignación,Fecha Límite,Estatus,Fecha Cumplimiento,Documentos Probatorios,Comentarios Dirección");

            foreach (var a in actividades)
            {
                var fechaCumpl = a.FechaCumplimiento?.ToString("yyyy-MM-dd HH:mm") ?? "Sin entrega";
                var docsCount  = a.Documentos?.Count ?? 0;
                var comentario = (a.ComentarioDirector ?? "").Replace("\"", "\"\"");
                var titulo     = a.Titulo.Replace("\"", "\"\"");
                var docente    = (a.Docente?.NombreCompleto ?? "Sin docente").Replace("\"", "\"\"");

                sb.AppendLine($"{a.Id},\"{a.Docente?.ClaveDocente}\",\"{docente}\",\"{a.Docente?.TipoContrato}\",\"{titulo}\",\"{a.Categoria}\",{a.FechaAsignacion:yyyy-MM-dd},{a.FechaLimite:yyyy-MM-dd},\"{a.Estatus}\",{fechaCumpl},{docsCount},\"{comentario}\"");
            }

            var nombreArchivo = $"Reporte_Compromisos_{anioFiltro}_{mesFiltro:D2}.csv";
            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv; charset=utf-8", nombreArchivo);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GenerarInvitacion([FromBody] InvitacionDocente? model)
        {
            var invitacion = new InvitacionDocente
            {
                Token             = Guid.NewGuid().ToString("N"),
                MatriculaSugerida = model?.MatriculaSugerida?.Trim(),
                FechaCreacion     = DateTime.Now,
                FechaExpiracion   = DateTime.Now.AddDays(7),
                Utilizado         = false
            };

            _context.InvitacionesDocentes.Add(invitacion);
            await _context.SaveChangesAsync();

            var enlace = Url.Action("Registro", "Docentes", new { token = invitacion.Token }, Request.Scheme);
            return Json(new { success = true, url = enlace, token = invitacion.Token });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Registro(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Content("Enlace de registro no válido o ausente.");

            var invitacion = await _context.InvitacionesDocentes
                .FirstOrDefaultAsync(i => i.Token == token && !i.Utilizado && i.FechaExpiracion >= DateTime.Now);

            if (invitacion == null)
                return Content("Este enlace de registro ya expiró o ya fue utilizado.");

            ViewBag.Token             = token;
            ViewBag.MatriculaSugerida = invitacion.MatriculaSugerida;

            return View(new Docente { ClaveDocente = invitacion.MatriculaSugerida ?? string.Empty });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarRegistro(string token, Docente docente)
        {
            var invitacion = await _context.InvitacionesDocentes
                .FirstOrDefaultAsync(i => i.Token == token && !i.Utilizado && i.FechaExpiracion >= DateTime.Now);

            if (invitacion == null)
                return Content("Error: La invitación ya no es válida.");

            bool existeMatricula = await _context.Docentes
                .AnyAsync(d => d.ClaveDocente == docente.ClaveDocente);

            if (ModelState.IsValid)
            {
                if (existeMatricula)
                {
                    ModelState.AddModelError("ClaveDocente", "Esta matrícula ya está registrada en el sistema.");
                    ViewBag.Token = token;
                    return View("Registro", docente);
                }

                _context.Docentes.Add(docente);
                invitacion.Utilizado = true;
                await _context.SaveChangesAsync();

                var matricula = docente.ClaveDocente.Trim().ToLower();
                if (!await _context.Usuarios.AnyAsync(u => u.NombreUsuario == matricula))
                {
                    _context.Usuarios.Add(new Usuario
                    {
                        NombreUsuario = matricula,
                        PasswordHash  = PasswordHelper.HashPassword(matricula, docente.ClaveDocente.Trim()),
                        Rol           = RolUsuario.Docente,
                        DocenteId     = docente.Id,
                        Activo        = true,
                        RequiereCambioPassword = true
                    });
                    await _context.SaveChangesAsync();
                }

                return View("RegistroExitoso", docente);
            }

            ViewBag.Token = token;
            return View("Registro", docente);
        }

        [HttpGet]
        public async Task<IActionResult> MisActividades(string? matricula)
        {
            if (User.IsInRole("Docente"))
            {
                matricula = User.Identity?.Name;
            }

            if (string.IsNullOrWhiteSpace(matricula))
            {
                ViewBag.Docente = null;
                return View(new List<ActividadDocente>());
            }

            var clave = matricula.Trim().ToLower();
            var docente = await _context.Docentes
                .Include(d => d.Actividades)
                    .ThenInclude(a => a.Documentos)
                .Include(d => d.Actividades)
                    .ThenInclude(a => a.Comentarios)
                .FirstOrDefaultAsync(d => d.ClaveDocente.ToLower() == clave);

            if (docente == null)
            {
                ViewBag.Error   = "No se encontró ningún docente con la matrícula ingresada.";
                ViewBag.Docente = null;
                return View(new List<ActividadDocente>());
            }

            ViewBag.Docente   = docente;
            ViewBag.Matricula = docente.ClaveDocente;

            var actividades = docente.Actividades
                .OrderBy(a => a.Estatus == "Cumplido" ? 2 : (a.Estatus == "En revision" ? 1 : 0))
                .ThenBy(a => a.FechaLimite)
                .ToList();

            return View(actividades);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarEntregada([FromBody] EntregaDocenteDTO model)
        {
            var actividad = await _context.ActividadesDocentes.FindAsync(model.ActividadId);
            if (actividad == null)
                return NotFound(new { success = false, message = "Actividad no encontrada." });

            actividad.Estatus           = "En revision";
            actividad.FechaCumplimiento = DateTime.Now;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public class EntregaDocenteDTO
        {
            public int ActividadId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> SubirDocumento(int actividadId, IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { success = false, message = "No se seleccionó ningún archivo." });

            if (archivo.Length > MaxTamanoBytes)
                return BadRequest(new { success = false, message = "El archivo supera el límite de 10 MB." });

            var ext = Path.GetExtension(archivo.FileName);
            if (!ExtPermitidas.Contains(ext))
                return BadRequest(new { success = false, message = $"Tipo de archivo no permitido ({ext}). Usa PDF, Word, imagen o Excel." });

            var actividad = await _context.ActividadesDocentes.FindAsync(actividadId);
            if (actividad == null)
                return NotFound(new { success = false, message = "Actividad no encontrada." });

            if (User.IsInRole("Docente"))
            {
                var matricula = User.Identity?.Name ?? "";
                var docenteDb = await _context.Docentes
                    .FirstOrDefaultAsync(d => d.ClaveDocente.ToLower() == matricula.ToLower());

                if (docenteDb == null || docenteDb.Id != actividad.DocenteId)
                    return Forbid();
            }

            var carpeta = Path.Combine(_env.WebRootPath, "uploads", "docs");
            Directory.CreateDirectory(carpeta);

            var nombreAlmacenado = $"{Guid.NewGuid()}{ext}";
            var rutaFisica       = Path.Combine(carpeta, nombreAlmacenado);

            await using (var stream = new FileStream(rutaFisica, FileMode.Create))
                await archivo.CopyToAsync(stream);

            var doc = new DocumentoActividad
            {
                ActividadDocenteId = actividadId,
                NombreOriginal     = archivo.FileName,
                NombreAlmacenado   = nombreAlmacenado,
                RutaRelativa       = $"uploads/docs/{nombreAlmacenado}",
                ContentType        = archivo.ContentType,
                TamanoBytes        = archivo.Length,
                SubidoPor          = User.Identity?.Name ?? "desconocido"
            };

            _context.DocumentosActividades.Add(doc);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success    = true,
                id         = doc.Id,
                nombre     = doc.NombreOriginal,
                tamano     = $"{archivo.Length / 1024} KB",
                previewUrl = Url.Action("PreviewDocumento", "Docentes", new { id = doc.Id })
            });
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> PreviewDocumento(int id)
        {
            var doc = await _context.DocumentosActividades.FindAsync(id);
            if (doc == null) return NotFound();

            var ruta = Path.Combine(_env.WebRootPath, doc.RutaRelativa.Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(ruta)) return NotFound("El archivo ya no existe en el servidor.");

            var bytes = await System.IO.File.ReadAllBytesAsync(ruta);

            var disposicion = (doc.ContentType.StartsWith("image/") || doc.ContentType == "application/pdf")
                ? "inline"
                : "attachment";

            Response.Headers.Append("Content-Disposition", $"{disposicion}; filename=\"{doc.NombreOriginal}\"");
            return File(bytes, doc.ContentType);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DescargarDocumento(int id)
        {
            var doc = await _context.DocumentosActividades.FindAsync(id);
            if (doc == null) return NotFound();

            var ruta = Path.Combine(_env.WebRootPath, doc.RutaRelativa.Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(ruta)) return NotFound("El archivo ya no existe en el servidor.");

            var bytes = await System.IO.File.ReadAllBytesAsync(ruta);
            return File(bytes, doc.ContentType, doc.NombreOriginal);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarDocumento(int id)
        {
            var doc = await _context.DocumentosActividades.FindAsync(id);
            if (doc == null) return NotFound(new { success = false });

            if (User.IsInRole("Docente") && doc.SubidoPor != User.Identity?.Name)
                return Forbid();

            var ruta = Path.Combine(_env.WebRootPath, doc.RutaRelativa.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(ruta))
                System.IO.File.Delete(ruta);

            _context.DocumentosActividades.Remove(doc);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> MiPerfil()
        {
            var esAdmin   = User.IsInRole("Administrador");
            var matricula = User.Identity?.Name ?? "";

            if (esAdmin)
            {
                var admin = await _context.Usuarios.FirstOrDefaultAsync(u => u.Rol == RolUsuario.Administrador);
                if (admin == null) return NotFound("Usuario de Dirección no encontrado.");

                var model = new MiPerfilViewModel
                {
                    Id                = admin.Id,
                    Rol               = "Administrador",
                    NombreUsuario     = admin.NombreUsuario,
                    NombreCompleto    = admin.NombreCompleto ?? "Dirección de División Académica",
                    Correo            = admin.Correo ?? "direccion@upvm.edu.mx",
                    Telefono          = admin.Telefono ?? "",
                    FotoUrl           = admin.FotoUrl,
                    TituloCargoOGrado = admin.TituloCargo ?? "Director de División",
                    Cubiculo          = "Edificio A - Dirección Académica",
                    Especialidad      = "Gestión y Supervisión Académica",
                    Biografia         = admin.Biografia ?? "Responsable de la planeación, asignación y seguimiento de compromisos académicos docentes."
                };
                return View(model);
            }
            else
            {
                var docente = await _context.Docentes
                    .FirstOrDefaultAsync(d => d.ClaveDocente.ToLower() == matricula.ToLower());

                if (docente == null) return NotFound("Perfil docente no encontrado.");

                var model = new MiPerfilViewModel
                {
                    Id                = docente.Id,
                    Rol               = "Docente",
                    NombreUsuario     = docente.ClaveDocente,
                    NombreCompleto    = docente.NombreCompleto,
                    Correo            = docente.CorreoInstitucional,
                    Telefono          = docente.Telefono,
                    FotoUrl           = docente.FotoUrl,
                    TituloCargoOGrado = docente.TituloAcademico ?? "",
                    Cubiculo          = docente.Cubiculo ?? "",
                    Especialidad      = docente.Especialidad ?? "",
                    Biografia         = docente.Biografia ?? "",
                    TipoContrato      = docente.TipoContrato
                };
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MiPerfil(
            MiPerfilViewModel model,
            IFormFile? fotoArchivo,
            string? fotoRecortadaBase64 = null,
            bool eliminarFoto = false)
        {
            var esAdmin   = User.IsInRole("Administrador");
            var matricula = User.Identity?.Name ?? "";
            const int MaxFotoBytes = 5 * 1024 * 1024;

            string? nuevaFotoUrl = null;
            if (!eliminarFoto && !string.IsNullOrWhiteSpace(fotoRecortadaBase64))
            {
                string? ext = null;
                string? base64Payload = null;
                if (fotoRecortadaBase64.StartsWith("data:image/jpeg;base64,", StringComparison.OrdinalIgnoreCase))
                {
                    ext = ".jpg";
                    base64Payload = fotoRecortadaBase64["data:image/jpeg;base64,".Length..];
                }
                else if (fotoRecortadaBase64.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase))
                {
                    ext = ".png";
                    base64Payload = fotoRecortadaBase64["data:image/png;base64,".Length..];
                }
                else if (fotoRecortadaBase64.StartsWith("data:image/webp;base64,", StringComparison.OrdinalIgnoreCase))
                {
                    ext = ".webp";
                    base64Payload = fotoRecortadaBase64["data:image/webp;base64,".Length..];
                }

                if (ext != null && !string.IsNullOrWhiteSpace(base64Payload))
                {
                    try
                    {
                        var bytes = Convert.FromBase64String(base64Payload);
                        if (bytes.Length > 0 && bytes.Length <= MaxFotoBytes)
                        {
                            var carpeta = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                            Directory.CreateDirectory(carpeta);
                            var nombre = $"{Guid.NewGuid()}{ext}";
                            var rutaFisica = Path.Combine(carpeta, nombre);
                            await System.IO.File.WriteAllBytesAsync(rutaFisica, bytes);
                            nuevaFotoUrl = $"/uploads/avatars/{nombre}";
                        }
                    }
                    catch (FormatException)
                    {
                    }
                }
            }
            else if (!eliminarFoto && fotoArchivo != null && fotoArchivo.Length > 0 && fotoArchivo.Length <= MaxFotoBytes)
            {
                var ext = Path.GetExtension(fotoArchivo.FileName).ToLower();
                var extFotosPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (extFotosPermitidas.Contains(ext))
                {
                    var carpeta = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                    Directory.CreateDirectory(carpeta);
                    var nombre = $"{Guid.NewGuid()}{ext}";
                    var rutaFisica = Path.Combine(carpeta, nombre);
                    await using (var stream = new FileStream(rutaFisica, FileMode.Create))
                    {
                        await fotoArchivo.CopyToAsync(stream);
                    }
                    nuevaFotoUrl = $"/uploads/avatars/{nombre}";
                }
            }

            string? nombreActualizado = null;
            string? fotoActualizada   = null;

            if (esAdmin)
            {
                var admin = await _context.Usuarios.FirstOrDefaultAsync(u => u.Rol == RolUsuario.Administrador);
                if (admin != null)
                {
                    admin.NombreCompleto = !string.IsNullOrWhiteSpace(model.NombreCompleto) ? model.NombreCompleto.Trim() : admin.NombreCompleto;
                    admin.Correo         = model.Correo?.Trim() ?? admin.Correo;
                    admin.Telefono       = model.Telefono?.Trim();
                    admin.TituloCargo    = model.TituloCargoOGrado?.Trim();
                    admin.Biografia      = model.Biografia?.Trim();
                    if (eliminarFoto)
                    {
                        admin.FotoUrl = null;
                    }
                    else if (!string.IsNullOrEmpty(nuevaFotoUrl))
                    {
                        admin.FotoUrl = nuevaFotoUrl;
                    }
                    await _context.SaveChangesAsync();
                    nombreActualizado = admin.NombreCompleto ?? "Director";
                    fotoActualizada   = admin.FotoUrl ?? "";
                    TempData["Exito"] = "Perfil institucional actualizado correctamente.";
                }
            }
            else
            {
                var docente = await _context.Docentes
                    .FirstOrDefaultAsync(d => d.ClaveDocente.ToLower() == matricula.ToLower());

                if (docente != null)
                {
                    docente.NombreCompleto = !string.IsNullOrWhiteSpace(model.NombreCompleto) ? model.NombreCompleto.Trim() : docente.NombreCompleto;
                    docente.CorreoInstitucional = model.Correo?.Trim() ?? docente.CorreoInstitucional;
                    docente.Telefono       = model.Telefono?.Trim() ?? docente.Telefono;
                    docente.TituloAcademico = model.TituloCargoOGrado?.Trim();
                    docente.Cubiculo       = model.Cubiculo?.Trim();
                    docente.Especialidad   = model.Especialidad?.Trim();
                    docente.Biografia      = model.Biografia?.Trim();
                    if (eliminarFoto)
                    {
                        docente.FotoUrl = null;
                    }
                    else if (!string.IsNullOrEmpty(nuevaFotoUrl))
                    {
                        docente.FotoUrl = nuevaFotoUrl;
                    }
                    await _context.SaveChangesAsync();
                    nombreActualizado = docente.NombreCompleto;
                    fotoActualizada   = docente.FotoUrl ?? "";
                    TempData["Exito"] = "Perfil docente actualizado correctamente.";
                }
            }

            if (nombreActualizado != null)
            {
                var nuevosClaims = User.Claims
                    .Where(c => c.Type != "NombreCompleto" && c.Type != "FotoUrl")
                    .ToList();
                nuevosClaims.Add(new Claim("NombreCompleto", nombreActualizado));
                nuevosClaims.Add(new Claim("FotoUrl", fotoActualizada ?? ""));

                var nuevaIdentidad = new ClaimsIdentity(nuevosClaims, "SigedCookie");
                await HttpContext.SignInAsync("SigedCookie", new ClaimsPrincipal(nuevaIdentidad), new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8)
                });
            }

            return RedirectToAction(nameof(MiPerfil));
        }
    }
}