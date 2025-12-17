using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace VOtingSystemdraft.Models
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Title { get; set; } // e.g., "Voting Date Change"

        [Required]
        public required string Description { get; set; } // The full details

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int AdminId { get; set; } // The ID of the Admin who posted it
        public  Admin? Admin { get; set; } // Navigation property
    }
}
