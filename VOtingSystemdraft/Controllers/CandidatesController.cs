using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VOtingSystemdraft.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace VOtingSystemdraft.Controllers
{
    [Authorize]
    public class CandidatesController : Controller
    {
        private readonly DatabaseContext _context;

        public CandidatesController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Candidates
        [Authorize(Roles = "Admin, Voter, Candidate")]
        public async Task<IActionResult> Index()
        {
            var databaseContext = _context.Candidates.Include(c => c.User);
            return View(await databaseContext.ToListAsync());
        }

        // GET: Candidates/Details/5
        [Authorize(Roles = "Admin, Voter, Candidate")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var candidate = await _context.Candidates
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (candidate == null)
            {
                return NotFound();
            }

            return View(candidate);
        }

        // GET: Candidates/Create
        [Authorize(Roles = "Candidate, Admin")]
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Candidates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Candidate, Admin")]
        public async Task<IActionResult> Create([Bind("Id,PartyName,Symbol,Bio")] Candidate candidate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(candidate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email", candidate.Id);
            return View(candidate);
        }

        // GET: Candidates/Edit
        [Authorize(Roles = "Candidate, Admin")]
        public async Task<IActionResult> Edit()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Users");
            }
            var currentUserId = int.Parse(userIdClaim);

            var candidate = await _context.Candidates.FindAsync(currentUserId);
            if (candidate == null)
            {
                return RedirectToAction(nameof(Create));
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email", candidate.Id);
            return View(candidate);
        }

        // POST: Candidates/Edit
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Candidate, Admin")]
        public async Task<IActionResult> Edit([Bind("PartyName,Symbol,Bio")] Candidate formModel)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return RedirectToAction("Login", "Users");
                }
                var currentUserId = int.Parse(userIdClaim);

                var candidate = await _context.Candidates.FindAsync(currentUserId);
                if (candidate == null)
                {
                    return RedirectToAction(nameof(Create));
                }

                candidate.PartyName = formModel.PartyName;
                candidate.Symbol = formModel.Symbol;
                candidate.Bio = formModel.Bio;

                _context.Update(candidate);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(CandidateDashboard));
            }
            var userIdClaim2 = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserId2 = string.IsNullOrEmpty(userIdClaim2) ? 0 : int.Parse(userIdClaim2);
            var existing = await _context.Candidates.FindAsync(currentUserId2);
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email", existing?.Id);
            return View(existing);
        }

        // GET: Candidates/Delete/5
        [Authorize(Roles = "Candidate, Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var candidate = await _context.Candidates
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (candidate == null)
            {
                return NotFound();
            }

            return View(candidate);
        }

        // POST: Candidates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Candidate, Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate != null)
            {
                _context.Candidates.Remove(candidate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CandidateExists(int id)
        {
            return _context.Candidates.Any(e => e.Id == id);
        }
        [Authorize(Roles = "Candidate, Admin")]
        public IActionResult CandidateDashboard()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Users");
            }

            int userId = int.Parse(userIdClaim);

            // Fetch candidate info for this user
            var candidate = _context.Candidates.FirstOrDefault(c => c.Id == userId);

            ViewBag.LatestAnnouncements = _context.Announcements
                .OrderByDescending(a => a.CreatedDate)
                .Take(3)
                .ToList();

            return View(candidate);  // pass model to the view
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Candidate, Admin")]
        public async Task<IActionResult> UpdateCandidateInfo(int UserId, string PartyName, string Symbol, string Bio)
        {
            if (UserId == 0 || string.IsNullOrEmpty(PartyName))
            {
                return BadRequest("Invalid data");
            }

            // Check if candidate already exists for this user
            var candidate = await _context.Candidates.FindAsync(UserId);

            if (candidate == null)
            {
                // If not exists, create new candidate
                candidate = new Candidate
                {
                    Id = UserId,
                    PartyName = PartyName,
                    Symbol = Symbol,
                    Bio = Bio
                };

                _context.Candidates.Add(candidate);
            }
            else
            {
                // If exists, update existing candidate info
                candidate.PartyName = PartyName;
                candidate.Symbol = Symbol;
                candidate.Bio = Bio;

                _context.Candidates.Update(candidate);
            }

            await _context.SaveChangesAsync();

            // Redirect to the Candidate Dashboard (now model exists, so normal dashboard shows)
            return RedirectToAction("CandidateDashboard", "Candidates");
        }

        public IActionResult ElectionGuideLine()
        {
            return View();
        }

    }
}
