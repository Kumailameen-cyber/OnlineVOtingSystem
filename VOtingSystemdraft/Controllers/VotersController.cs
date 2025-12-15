using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VOtingSystemdraft.Models;

namespace VOtingSystemdraft.Controllers
{
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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voter = await _context.Voters.FindAsync(id);
            if (voter == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email", voter.Id);
            return View(voter);
        }

        // POST: Voters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NationalId,Address")] Voter voter)
        {
            if (id != voter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(voter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoterExists(voter.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email", voter.Id);
            return View(voter);
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
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var voter = _context.Voters.FirstOrDefault(v => v.Id == userId);
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
            // 1. Get the logged-in User's ID from Session
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Users"); // Not logged in? Go to login
            }

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

    }
}
