using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using VOtingSystemdraft.Models;

namespace VOtingSystemdraft.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly DatabaseContext _context;

        public UserManagementController(DatabaseContext context)
        {
            _context = context;
        }

        // =========================
        // LIST ALL USERS
        // =========================
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderBy(u => u.Username)
                .ToListAsync();

            ViewData["Title"] = "Manage Users";
            return View(users);
        }

        public async Task<IActionResult> Voters()
        {
            var users = await _context.Users
                .Where(u => u.Role == "Voter")
                .OrderBy(u => u.Username)
                .ToListAsync();

            ViewData["Title"] = "Manage Voters";
            return View("Index", users);
        }

        public async Task<IActionResult> Candidates()
        {
            var users = await _context.Users
                .Where(u => u.Role == "Candidate")
                .OrderBy(u => u.Username)
                .ToListAsync();

            ViewData["Title"] = "Manage Candidates";
            return View("Index", users);
        }

        // =========================
        // CREATE VOTER (GET)
        // =========================
        public IActionResult CreateVoters()
        {
            return View("CreateVoters");
        }

        // =========================
        // CREATE VOTER (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVoters(Voter voter, string username, string email)
        {
            if (!ModelState.IsValid)
                return View("CreateVoters", voter);

            // 1️⃣ Create User first
            var user = new User
            {
                Username = username,
                Email = email,
                Role = "Voter"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Save to get Id

            // 2️⃣ Link Voter to User
            voter.Id = user.Id; // Assuming shared PK
            _context.Voters.Add(voter);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Voter created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // CREATE CANDIDATE (GET)
        // =========================
        public IActionResult CreateCandidates()
        {
            return View("CreateCandidates");
        }

        // =========================
        // CREATE CANDIDATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCandidates(string username, string email, string password, string partyName, string symbol, string bio)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(partyName) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Username, Email, and Party Name are required.";
                return View();
            }

            // 1️⃣ Create User
            var user = new User
            {
                Username = username,
                Password=password,
                Email = email,
                Role = "Candidate"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Save to get User.Id

            // 2️⃣ Create Candidate
            var candidate = new Candidate
            {
                Id = user.Id, // Shared PK
                PartyName = partyName,
                Symbol = symbol,
                Bio = bio
            };
            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Candidate created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT (AUTO BY ROLE)
        // =========================
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.Role == "Voter")
            {
                var voter = await _context.Voters.FirstOrDefaultAsync(v => v.Id == id);
                if (voter == null) return NotFound();
                ViewBag.Username = user.Username;
                ViewBag.Email = user.Email;
                return View("EditVoter", voter);
            }

            if (user.Role == "Candidate")
            {
                var candidate = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == id);
                if (candidate == null) return NotFound();
                ViewBag.Username = user.Username;
                ViewBag.Email = user.Email;
                return View("EditCandidate", candidate);
            }

            return NotFound();
        }

        // =========================
        // EDIT VOTER (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVoter(Voter voter, string username, string email)
        {
            if (!ModelState.IsValid)
                return View("EditVoter", voter);

            _context.Voters.Update(voter);

            var user = await _context.Users.FindAsync(voter.Id);
            if (user != null)
            {
                user.Username = username;
                user.Email = email;
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Voter updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT CANDIDATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCandidate(Candidate candidate, string username, string email)
        {
            if (!ModelState.IsValid)
                return View("EditCandidate", candidate);

            _context.Candidates.Update(candidate);

            var user = await _context.Users.FindAsync(candidate.Id);
            if (user != null)
            {
                user.Username = username;
                user.Email = email;
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Candidate updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE USER
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(currentUserId) && int.Parse(currentUserId) == id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var voter = await _context.Voters.FirstOrDefaultAsync(v => v.Id == id);
            if (voter != null) _context.Voters.Remove(voter);

            var candidate = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == id);
            if (candidate != null) _context.Candidates.Remove(candidate);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
