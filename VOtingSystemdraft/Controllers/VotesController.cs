using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VOtingSystemdraft.Models;

namespace VOtingSystemdraft.Controllers
{
    [Authorize(Roles = "Voter")]
    public class VotesController : Controller
    {
        private readonly DatabaseContext _context;

        public VotesController(DatabaseContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        // GET: Votes/Index
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Users");

            var activeElections = await _context.Elections
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            // Check which elections the user has already voted in
            var userVotes = await _context.Votes
                .Where(v => v.VoterId == userId)
                .Select(v => v.ElectionId)
                .ToListAsync();

            // Create a list of ViewModel/DTO to pass to view
            var model = activeElections.Select(e => new ElectionStatusViewModel
            {
                Election = e,
                HasVoted = userVotes.Contains(e.Id)
            }).ToList();

            return View(model);
        }

        // GET: Votes/Ballot/5
        public async Task<IActionResult> Ballot(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Users");

            // Check if already voted
            var hasVoted = await _context.Votes.AnyAsync(v => v.ElectionId == id && v.VoterId == userId);
            if (hasVoted)
            {
                TempData["Message"] = "You have already voted in this election.";
                return RedirectToAction(nameof(Index));
            }

            var election = await _context.Elections
                .Include(e => e.Candidates)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (election == null || !election.IsActive)
            {
                return NotFound();
            }

            // Also check dates just in case
            var now = DateTime.Now;
            if (now < election.StartDate || now > election.EndDate)
            {
                TempData["Error"] = "This election is not currently active (date range).";
                return RedirectToAction(nameof(Index));
            }

            return View(election);
        }

        // POST: Votes/CastVote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CastVote(int electionId, int candidateId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Users");

            // Security Check 1: Already voted?
            var hasVoted = await _context.Votes.AnyAsync(v => v.ElectionId == electionId && v.VoterId == userId);
            if (hasVoted)
            {
                TempData["Error"] = "You have already voted in this election.";
                return RedirectToAction(nameof(Index));
            }

            // Security Check 2: Election active?
            var election = await _context.Elections.FindAsync(electionId);
            if (election == null || !election.IsActive)
            {
                TempData["Error"] = "Election is not active.";
                return RedirectToAction(nameof(Index));
            }

            var now = DateTime.Now;
            if (now < election.StartDate || now > election.EndDate)
            {
                TempData["Error"] = "Election is outside of voting period.";
                return RedirectToAction(nameof(Index));
            }

            // Create Vote
            var vote = new Vote
            {
                ElectionId = electionId,
                VoterId = userId,
                CandidateId = candidateId,
                Timestamp = DateTime.Now
            };

            _context.Add(vote);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your vote has been cast successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Votes/MyVotes
        public async Task<IActionResult> MyVotes()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Users");

            var votes = await _context.Votes
                .Include(v => v.Election)
                .Include(v => v.Candidate)
                .ThenInclude(c => c.User)
                .Where(v => v.VoterId == userId)
                .OrderByDescending(v => v.Timestamp)
                .ToListAsync();

            return View(votes);
        }
    }

    public class ElectionStatusViewModel
    {
        public Election Election { get; set; }
        public bool HasVoted { get; set; }
    }
}
