using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VOtingSystemdraft.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }

        public int ElectionId { get; set; }
        public Election Election { get; set; } = null!;

        public int VoterId { get; set; }
        public Voter Voter { get; set; } = null!;

        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
    }
}
