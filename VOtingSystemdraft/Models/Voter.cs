using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VOtingSystemdraft.Models
{
    public class Voter
    {
        [Key]
        [ForeignKey("User")]
        public int Id { get; set; }   // FK + PK (same as User.Id)

        [Required]
        public string NationalId { get; set; } = null!; // CNIC etc.

        public string? Address { get; set; }

        public User User { get; set; } = null!;
    }
}
