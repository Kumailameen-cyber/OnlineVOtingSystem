using System.ComponentModel.DataAnnotations;

namespace VOtingSystemdraft.Models.ViewModels
{
    public class RegisterViewModel
    {
        // common fields
        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } // "Voter", "Candidate", "Admin"

        // voter-specific (NOT required at attribute-level)
        public string? VoterNationalId { get; set; }
        public string? VoterAddress { get; set; }

        // candidate-specific (NOT required at attribute-level)
        public string? CandidatePartyName { get; set; }
        public string? CandidateSymbol { get; set; }
        public string? CandidateBio { get; set; }
    }
}
