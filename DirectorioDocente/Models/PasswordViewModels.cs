using System.ComponentModel.DataAnnotations;

namespace DirectorioDocente.Models
{
    public class CambiarPasswordViewModel
    {
        [Required(ErrorMessage = "Ingresa tu contraseña actual")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string PasswordNuevo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma la nueva contraseña")]
        [DataType(DataType.Password)]
        [Compare("PasswordNuevo", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar nueva contraseña")]
        public string ConfirmarPassword { get; set; } = string.Empty;

        public bool EsObligatorio { get; set; } = false;
    }

    public class RestablecerPasswordViewModel
    {
        public int UsuarioId { get; set; }

        [Display(Name = "Nombre del docente")]
        public string NombreDocente { get; set; } = string.Empty;

        [Display(Name = "Matrícula")]
        public string Matricula { get; set; } = string.Empty;

        public string? PasswordTemporal { get; set; }
    }
}
