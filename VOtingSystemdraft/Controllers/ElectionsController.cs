using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VOtingSystemdraft.Models;
using Microsoft.AspNetCore.Authorization;

namespace VOtingSystemdraft.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ElectionsController : Controller
    {
        private readonly DatabaseContext _context;

        public ElectionsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Elections
        public async Task<IActionResult> Index()
        {
            return View(await _context.Elections.OrderByDescending(e => e.StartDate).ToListAsync());
        }

        // GET: Elections/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var election = await _context.Elections
                .Include(e => e.Candidates)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (election == null)
            {
                return NotFound();
            }

            return View(election);
        }

        // GET: Elections/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Elections/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,StartDate,EndDate,IsActive")] Election election)
        {
            if (election.EndDate <= election.StartDate)
            {
                ModelState.AddModelError("EndDate", "End Date must be later than Start Date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(election);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(election);
        }
    }
}
