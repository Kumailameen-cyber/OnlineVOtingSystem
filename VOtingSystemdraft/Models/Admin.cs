using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VOtingSystemdraft.Models
{
    public class Admin
    {
        [Key]
        [ForeignKey("User")]
        public int Id { get; set; }  // Primary Key + Foreign Key to User

        public User User { get; set; } = null!;
    }
}
