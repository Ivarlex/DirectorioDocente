using System.ComponentModel.DataAnnotations;

namespace DirectorioDocente.Models
{
    public class MiPerfilViewModel
    {
        public int Id { get; set; }

        public string Rol { get; set; } = string.Empty;

        [Display(Name = "Usuario / Matrícula")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido")]
        [Display(Name = "Correo Institucional")]
        public string Correo { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Teléfono de Contacto")]
        public string? Telefono { get; set; }

        public string? FotoUrl { get; set; }

        [Display(Name = "Título o Grado Académico / Cargo")]
        public string? TituloCargoOGrado { get; set; }

        [Display(Name = "Cubículo u Horario de Atención")]
        public string? Cubiculo { get; set; }

        [Display(Name = "Área de Especialidad / Academia")]
        public string? Especialidad { get; set; }

        [Display(Name = "Semblanza / Biografía")]
        public string? Biografia { get; set; }

        [Display(Name = "Tipo de Contrato")]
        public string? TipoContrato { get; set; }
    }

    public class AgregarComentarioDTO
    {
        public int ActividadId { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
