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
    [Authorize(Roles = "Voter, Admin")]
    public class VotersController : Controller
    {
        private readonly DatabaseContext _context;

        public VotersController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Voters
        public async Task<IActionResult> Index()
        {
            var databaseContext = _context.Voters.Include(v => v.User);
            return View(await databaseContext.ToListAsync());
        }

        // GET: Voters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (User.IsInRole("Voter"))
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return RedirectToAction("Login", "Users");
                }
                id = int.Parse(userIdClaim);
            }
            else
            {
                if (id == null)
                {
                    return NotFound();
                }
            }

            var voter = await _context.Voters
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voter == null)
            {
                return NotFound();
            }

            return View(voter);
        }

        // GET: Voters/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Voters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NationalId,Address")] Voter voter)
        {
            if (ModelState.IsValid)
            {
                _context.Add(voter);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email", voter.Id);
            return View(voter);
        }

        // GET: Voters/Edit/5
        public async Task<IActionResult> Edit()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Users");
            }
            var userId = int.Parse(userIdClaim);

            var voter = await _context.Voters.FindAsync(userId);
            if (voter == null)
            {
                return RedirectToAction("Create");
            }
            return View(voter);
        }

        // POST: Voters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string NationalId, string? Address)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Users");
            }
            var userId = int.Parse(userIdClaim);

            if (string.IsNullOrEmpty(NationalId))
            {
                ModelState.AddModelError("NationalId", "National ID is required.");
            }

            if (ModelState.IsValid)
            {
                var voter = await _context.Voters.FindAsync(userId);
                if (voter == null)
                {
                    return RedirectToAction("Create");
                }
                voter.NationalId = NationalId;
                voter.Address = Address;
                _context.Update(voter);
                await _context.SaveChangesAsync();
                return RedirectToAction("MyProfile");
            }
            var existing = await _context.Voters.FindAsync(userId);
            return View(existing);
        }

        // GET: Voters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voter = await _context.Voters
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voter == null)
            {
                return NotFound();
            }

            return View(voter);
        }

        // POST: Voters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var voter = await _context.Voters.FindAsync(id);
            if (voter != null)
            {
                _context.Voters.Remove(voter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VoterExists(int id)
        {
            return _context.Voters.Any(e => e.Id == id);
        }
        public IActionResult VoterDashboard()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Users");
            }

            int userId = int.Parse(userIdClaim);

            var voter = _context.Voters.FirstOrDefault(v => v.Id == userId);
            ViewBag.LatestAnnouncements = _context.Announcements
                .Include(a => a.Admin)
                .ThenInclude(ad => ad.User)
                .OrderByDescending(a => a.CreatedDate)
                .Take(5)
                .ToList();
            return View(voter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVoterInfo(int UserId, string NationalId, string Address)
        {
            var voter = await _context.Voters.FirstOrDefaultAsync(v => v.Id == UserId);

            if (voter == null)
            {
                voter = new Voter
                {
                    Id = UserId,
                    NationalId = NationalId,
                    Address = Address
                };
                _context.Add(voter);
            }
            else
            {
                voter.NationalId = NationalId;
                voter.Address = Address;
                _context.Update(voter);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("VoterDashboard", "Voters");
        }

        // GET: Voters/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Users");
            }

            int userId = int.Parse(userIdClaim);

            // 2. Try to find the Voter profile linked to this User
            var voterProfile = await _context.Voters
                .Include(v => v.User) // Load the User data (Email, Username) too
                .FirstOrDefaultAsync(m => m.Id == userId);

            // 3. If profile doesn't exist yet, send them to Create it
            if (voterProfile == null)
            {
                // Optional: Add a message saying "Please complete your profile"
                return RedirectToAction("Create");
            }

            // 4. If profile exists, show the "MyProfile" view
            return View(voterProfile);
        }

        // GET: ElectionInfo
        public IActionResult ElectionInfo()
        {
            return View();
        }

        // Post: ElectionInfo

    }
}
