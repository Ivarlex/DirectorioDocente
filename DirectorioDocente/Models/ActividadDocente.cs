using System.ComponentModel.DataAnnotations;

namespace DirectorioDocente.Models
{
    public class ActividadDocente
    {
        public int Id { get; set; }

        public int DocenteId { get; set; }
        public Docente? Docente { get; set; }

        [Required]
        [Display(Name = "Título de la Actividad")]
        public string Titulo { get; set; } = string.Empty;

        public string Categoria { get; set; } = "Administrativa";

        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Fecha Límite")]
        public DateTime FechaLimite { get; set; }

        public DateTime? FechaCumplimiento { get; set; }

        public string Estatus { get; set; } = "Pendiente";

        public string? EvidenciaUrl { get; set; }

        [MaxLength(1000)]
        public string? ComentarioDirector { get; set; }

        public DateTime? FechaRevision { get; set; }

        public ICollection<DocumentoActividad> Documentos { get; set; } = new List<DocumentoActividad>();

        public ICollection<ComentarioActividad> Comentarios { get; set; } = new List<ComentarioActividad>();
    }
}