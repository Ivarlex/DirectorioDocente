using System.ComponentModel.DataAnnotations;

namespace DirectorioDocente.Models
{
    public enum RolUsuario
    {
        Administrador,
        Docente
    }

    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Nombre de usuario / Matrícula")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        public RolUsuario Rol { get; set; } = RolUsuario.Docente;

        public int? DocenteId { get; set; }
        public Docente? Docente { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;

        public bool RequiereCambioPassword { get; set; } = true;

        [StringLength(150)]
        public string? NombreCompleto { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? Correo { get; set; }

        [StringLength(30)]
        public string? Telefono { get; set; }

        public string? FotoUrl { get; set; }

        [StringLength(100)]
        public string? TituloCargo { get; set; }

        [StringLength(1000)]
        public string? Biografia { get; set; }
    }
}
