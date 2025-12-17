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

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.OrderBy(u => u.Username).ToListAsync();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentIdClaim))
            {
                return RedirectToAction("Login", "Users");
            }
            if (int.Parse(currentIdClaim) == id)
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

            var voter = await _context.Voters.FindAsync(id);
            if (voter != null) _context.Voters.Remove(voter);

            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate != null) _context.Candidates.Remove(candidate);

            var admin = await _context.Admins.FindAsync(id);
            if (admin != null) _context.Admins.Remove(admin);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "User deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
