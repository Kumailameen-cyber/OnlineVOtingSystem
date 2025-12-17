using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VOtingSystemdraft.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using VOtingSystemdraft.Services;
using Microsoft.AspNetCore.Authorization;

namespace VOtingSystemdraft.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly PasswordHelper _passwordHelper;

        public UsersController(DatabaseContext context, PasswordHelper passwordHelper)
        {
            _context = context;
            _passwordHelper = passwordHelper;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,Password,Email,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Password,Email,Role")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            [Bind("Id,Username,Password,Email,Role")] User user,
            string? NationalId, string? Address,              // Changed to string? (Optional)
            string? PartyName, string? Symbol, string? Bio)   // Changed to string? (Optional)
        {
            // 1. MANUAL VALIDATION (Because we made fields optional, we must check them ourselves)
            if (user.Role == "Voter")
            {
                if (string.IsNullOrEmpty(NationalId)) ModelState.AddModelError("NationalId", "National ID is required.");
                if (string.IsNullOrEmpty(Address)) ModelState.AddModelError("Address", "Address is required.");
            }
            else if (user.Role == "Candidate")
            {
                if (string.IsNullOrEmpty(PartyName)) ModelState.AddModelError("PartyName", "Party Name is required.");
                if (string.IsNullOrEmpty(Symbol)) ModelState.AddModelError("Symbol", "Symbol is required.");
                if (string.IsNullOrEmpty(Bio)) ModelState.AddModelError("Bio", "Bio is required.");
            }

            if (ModelState.IsValid)
            {
                // 2. Check for duplicates
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ViewBag.Message = "Email already registered.";
                    return View(user);
                }
                if (_context.Users.Any(u => u.Username.ToLower() == user.Username.ToLower()))
                {
                    ViewBag.Message = "Username already taken.";
                    return View(user);
                }

                // 3. Hash Password & Save User
                user.Password = _passwordHelper.HashPassword(user.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();

                // 4. Save Role Data
                if (user.Role == "Voter")
                {
                    var voter = new Voter
                    {
                        Id = user.Id,
                        NationalId = NationalId, // We know this is safe now because of Step 1
                        Address = Address
                    };
                    _context.Add(voter);
                    await _context.SaveChangesAsync();
                }
                else if (user.Role == "Candidate")
                {
                    var candidate = new Candidate
                    {
                        Id = user.Id,
                        PartyName = PartyName,
                        Symbol = Symbol,
                        Bio = Bio
                    };
                    _context.Add(candidate);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Login", "Users");
            }

            // If validation failed (e.g. Voter didn't enter NationalID), show the form again
            return View(user);
        }
        // GET: Users/Login
        // This runs when you click the link to open the page
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user != null && _passwordHelper.VerifyPassword(password, user.Password))
            {
                // Create Claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Redirect according to role
                if (user.Role == "Voter")
                    return RedirectToAction("VoterDashboard", "Voters");

                if (user.Role == "Admin")
                    return RedirectToAction("AdminDashboard", "Admins");

                if (user.Role == "Candidate")
                    return RedirectToAction("CandidateDashboard", "Candidates");
            }

            ViewBag.Message = "Invalid email or password";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Users"); 
        }

    }
}
