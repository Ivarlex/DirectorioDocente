using System.ComponentModel.DataAnnotations;

namespace DirectorioDocente.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "La matrícula o usuario es obligatorio")]
        [Display(Name = "Matrícula / Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Mantener sesión iniciada")]
        public bool Recordarme { get; set; } = false;
    }
}
