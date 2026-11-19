using Microsoft.EntityFrameworkCore;
using DirectorioDocente.Models;

namespace DirectorioDocente.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Docente>            Docentes              => Set<Docente>();
        public DbSet<ActividadDocente>   ActividadesDocentes   => Set<ActividadDocente>();
        public DbSet<InvitacionDocente>  InvitacionesDocentes  => Set<InvitacionDocente>();
        public DbSet<Usuario>            Usuarios              => Set<Usuario>();
        public DbSet<DocumentoActividad> DocumentosActividades => Set<DocumentoActividad>();
        public DbSet<ComentarioActividad> ComentariosActividades => Set<ComentarioActividad>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Docente)
                .WithMany()
                .HasForeignKey(u => u.DocenteId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DocumentoActividad>()
                .HasOne(d => d.ActividadDocente)
                .WithMany(a => a.Documentos)
                .HasForeignKey(d => d.ActividadDocenteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComentarioActividad>()
                .HasOne(c => c.ActividadDocente)
                .WithMany(a => a.Comentarios)
                .HasForeignKey(c => c.ActividadDocenteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            modelBuilder.Entity<Usuario>().HasData(new Usuario
            {
                Id              = 1,
                NombreUsuario   = "director",
                PasswordHash    = "8f14e45fceea167a5a36dedd4bea2543b9ecca02b49ede9a571527a5e6fb9c9a",
                Rol             = RolUsuario.Administrador,
                DocenteId       = null,
                FechaCreacion   = new DateTime(2026, 1, 1),
                Activo          = true,
                RequiereCambioPassword = true
            });
        }
    }
}