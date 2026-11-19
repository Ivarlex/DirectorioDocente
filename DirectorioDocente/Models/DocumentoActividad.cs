using System.ComponentModel.DataAnnotations;

namespace DirectorioDocente.Models
{
    public class DocumentoActividad
    {
        public int Id { get; set; }

        public int ActividadDocenteId { get; set; }
        public ActividadDocente? ActividadDocente { get; set; }

        [Required]
        [StringLength(260)]
        [Display(Name = "Nombre original del archivo")]
        public string NombreOriginal { get; set; } = string.Empty;

        [Required]
        [StringLength(260)]
        public string NombreAlmacenado { get; set; } = string.Empty;

        [Required]
        public string RutaRelativa { get; set; } = string.Empty;

        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long TamanoBytes { get; set; }

        public DateTime FechaSubida { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string SubidoPor { get; set; } = string.Empty;
    }
}
