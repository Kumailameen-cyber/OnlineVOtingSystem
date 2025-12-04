using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using VOtingSystemdraft.Models;
using VOtingSystemdraft.Models.ViewModels;

namespace VOtingSystemdraft.Controllers
{
    public class UsersController : Controller
    {
        private readonly DatabaseContext _context;

        public UsersController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: /Users/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: /Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // 1. Basic model validation for common fields (Username, Email, Password, Role)
            if (!ModelState.IsValid)
                return View(model);

            // 2. Uniqueness check
            bool exists = await _context.Users.AnyAsync(u => u.Email == model.Email || u.Username == model.Username);
            if (exists)
            {
                ModelState.AddModelError("", "Username or Email already exists.");
                return View(model);
            }

            // 3. Conditional validation: only enforce Voter fields if Role == "Voter", etc.
            if (string.IsNullOrWhiteSpace(model.Role))
            {
                ModelState.AddModelError("Role", "Please select a role.");
            }
            else if (model.Role == "Voter")
            {
                if (string.IsNullOrWhiteSpace(model.VoterNationalId))
                    ModelState.AddModelError("VoterNationalId", "National ID is required for voters.");
                if (string.IsNullOrWhiteSpace(model.VoterAddress))
                    ModelState.AddModelError("VoterAddress", "Address is required for voters.");
            }
            else if (model.Role == "Candidate")
            {
                if (string.IsNullOrWhiteSpace(model.CandidatePartyName))
                    ModelState.AddModelError("CandidatePartyName", "Party name is required for candidates.");
                if (string.IsNullOrWhiteSpace(model.CandidateSymbol))
                    ModelState.AddModelError("CandidateSymbol", "Symbol is required for candidates.");
            }
            else if (model.Role != "Admin")
            {
                ModelState.AddModelError("Role", "Invalid role selected.");
            }

            if (!ModelState.IsValid)
                return View(model);

            // 4. Create user and hash password
            var user = new User
            {
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                Role = model.Role
            };

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, model.Password);

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // user.Id generated

                // 5. Save role-specific data using shared PK
                if (model.Role == "Voter")
                {
                    var voter = new Voter
                    {
                        Id = user.Id,
                        NationalId = model.VoterNationalId?.Trim(),
                        Address = model.VoterAddress?.Trim(),
                        User = user
                    };
                    _context.Voters.Add(voter);
                }
                else if (model.Role == "Candidate")
                {
                    var candidate = new Candidate
                    {
                        Id = user.Id,
                        PartyName = model.CandidatePartyName?.Trim(),
                        Symbol = model.CandidateSymbol?.Trim(),
                        Bio = model.CandidateBio?.Trim(),
                        User = user
                    };
                    _context.Candidates.Add(candidate);
                }
                else // Admin
                {
                    _context.Admins.Add(new Admin { Id = user.Id, User = user });
                }

                await _context.SaveChangesAsync();

                // 6. Redirect to Login page (user will sign in using their email and password)
                TempData["Success"] = "Account created successfully. Please log in.";
                return RedirectToAction("Login", "Users");
            }
            catch (Exception ex)
            {
                // log or inspect ex in Visual Studio Output; return a friendly message
                Console.WriteLine(ex);
                ModelState.AddModelError("", "An error occurred while creating your account. Please try again.");
                return View(model);
            }
        }

        // GET: /Users/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email.Trim());
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            // Set session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Role", user.Role);

            // Redirect based on role (adjust controller/action names to match your project)
            if (user.Role == "Voter") return RedirectToAction("VoterDashboard", "Voters");
            if (user.Role == "Admin") return RedirectToAction("AdminDashboard", "Admins");
            if (user.Role == "Candidate") return RedirectToAction("CandidateDashboard", "Candidates");

            return RedirectToAction("Index", "Home");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Users");
        }
    }
}
