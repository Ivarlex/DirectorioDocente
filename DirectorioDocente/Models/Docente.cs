using System.ComponentModel.DataAnnotations;

namespace DirectorioDocente.Models
{
    public class Docente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La matrícula es obligatoria")]
        [Display(Name = "Matrícula / Clave")]
        public string ClaveDocente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress]
        [Display(Name = "Correo Institucional")]
        public string CorreoInstitucional { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Display(Name = "Tipo de Contrato")]
        public string TipoContrato { get; set; } = "Tiempo Completo";

        public bool Activo { get; set; } = true;
 
        [Display(Name = "Foto de Perfil")]
        public string? FotoUrl { get; set; }

        [Display(Name = "Título o Grado Académico")]
        public string? TituloAcademico { get; set; }

        [Display(Name = "Cubículo / Ubicación")]
        public string? Cubiculo { get; set; }

        [Display(Name = "Semblanza / Biografía")]
        public string? Biografia { get; set; }

        [Display(Name = "Área de Especialidad")]
        public string? Especialidad { get; set; }

        public ICollection<ActividadDocente> Actividades { get; set; } = new List<ActividadDocente>();
    }
}