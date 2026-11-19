using System.ComponentModel.DataAnnotations;

namespace DirectorioDocente.Models
{
    public class AsignarActividadViewModel
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        public string Titulo { get; set; } = string.Empty;

        public string Categoria { get; set; } = "Administrativa";

        [Required(ErrorMessage = "La fecha límite es obligatoria")]
        public DateTime FechaLimite { get; set; } = DateTime.Today.AddDays(7);

        public string ModoAsignacion { get; set; } = "individual";

        public int? DocenteId { get; set; }
    }
}