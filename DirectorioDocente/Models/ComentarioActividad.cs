using System.ComponentModel.DataAnnotations;

namespace DirectorioDocente.Models
{
    public class ComentarioActividad
    {
        public int Id { get; set; }

        public int ActividadDocenteId { get; set; }
        public ActividadDocente? ActividadDocente { get; set; }

        [Required]
        [MaxLength(150)]
        public string AutorNombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RolAutor { get; set; } = "Docente";

        [Required]
        [MaxLength(2000)]
        public string Mensaje { get; set; } = string.Empty;

        public DateTime FechaEnvio { get; set; } = DateTime.Now;
    }
}
