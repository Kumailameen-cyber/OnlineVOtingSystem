using System.Collections.Generic;

namespace VOtingSystemdraft.Models.ViewModels
{
    public class ElectionResultViewModel
    {
        public string ElectionTitle { get; set; } = string.Empty;
        public int TotalVotes { get; set; }
        public List<CandidateResult> Results { get; set; } = new List<CandidateResult>();
        public CandidateResult? Winner { get; set; }
    }

    public class CandidateResult
    {
        public string CandidateName { get; set; } = string.Empty;
        public string Party { get; set; } = string.Empty;
        public int VoteCount { get; set; }
        public double Percentage { get; set; }
    }
}
