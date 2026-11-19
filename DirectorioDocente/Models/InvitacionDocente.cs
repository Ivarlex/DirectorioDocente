using System.ComponentModel.DataAnnotations;

namespace DirectorioDocente.Models
{
    public class InvitacionDocente
    {
        public int Id { get; set; }

        [Required]
        [StringLength(64)]
        public string Token { get; set; } = Guid.NewGuid().ToString("N");

        [StringLength(50)]
        public string? MatriculaSugerida { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime FechaExpiracion { get; set; } = DateTime.Now.AddDays(7);

        public bool Utilizado { get; set; } = false;
    }
}