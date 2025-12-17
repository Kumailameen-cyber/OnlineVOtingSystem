using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VOtingSystemdraft.Models;
using VOtingSystemdraft.Models.ViewModels;

namespace VOtingSystemdraft.Controllers
{
    [Authorize]
    public class ResultsController : Controller
    {
        private readonly DatabaseContext _context;

        public ResultsController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var elections = await _context.Elections
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            var voteCounts = await _context.Votes
                .GroupBy(v => v.ElectionId)
                .Select(g => new { ElectionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ElectionId, x => x.Count);

            var model = elections.Select(e => new
            {
                Election = e,
                TotalVotes = voteCounts.TryGetValue(e.Id, out var c) ? c : 0
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var election = await _context.Elections
                .Include(e => e.Candidates)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(e => e.Id == id.Value);
            if (election == null) return NotFound();

            var votes = await _context.Votes
                .Where(v => v.ElectionId == election.Id)
                .ToListAsync();

            var totalVotes = votes.Count;

            var candidateCounts = votes
                .GroupBy(v => v.CandidateId)
                .Select(g => new { CandidateId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.CandidateId, x => x.Count);

            var results = election.Candidates
                .Select(c =>
                {
                    var count = candidateCounts.TryGetValue(c.Id, out var cnt) ? cnt : 0;
                    var pct = totalVotes > 0 ? (count * 100.0) / totalVotes : 0.0;
                    return new CandidateResult
                    {
                        CandidateName = c.User?.Username ?? "Unknown",
                        Party = c.PartyName,
                        VoteCount = count,
                        Percentage = pct
                    };
                })
                .OrderByDescending(r => r.VoteCount)
                .ToList();

            var viewModel = new ElectionResultViewModel
            {
                ElectionTitle = election.Title,
                TotalVotes = totalVotes,
                Results = results,
                Winner = results.FirstOrDefault()
            };

            return View(viewModel);
        }
    }
}
