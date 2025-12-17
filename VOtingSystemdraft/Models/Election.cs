using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VOtingSystemdraft.Models
{
    public class Election
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        // Relationships
        public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}
