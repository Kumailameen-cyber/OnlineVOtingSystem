using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VOtingSystemdraft.Models
{
    public class Candidate
    {
        [Key]
        [ForeignKey("User")]
        public int Id { get; set; }   // FK + PK (same as User.Id)

        [Required]
        public string PartyName { get; set; } = null!;

        public string? Symbol { get; set; }
        public string? Bio { get; set; }

        public User User { get; set; } = null!;
    }
}
