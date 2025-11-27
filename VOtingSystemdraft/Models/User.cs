using System.ComponentModel.DataAnnotations;

namespace VOtingSystemdraft.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!; // plain for now, will hash later

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Role { get; set; } = "Voter"; // Voter / Candidate / Admin

        

        
    }
}
