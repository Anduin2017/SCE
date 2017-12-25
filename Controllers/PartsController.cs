using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SCE.Data;
using SCE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace SCE.Controllers
{
    public class PartsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public PartsController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _context = context;

        }

        private bool Auth()
        {
            if (_signInManager.IsSignedIn(User) && _context.Users.Include(t => t.UserType).SingleOrDefault(t => t.UserName == User.Identity.Name).UserType.IsAdmin)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // GET: Parts
        public async Task<IActionResult> Index()
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            return View(await _context.Part.Include(t=>t.Users).ToListAsync());
        }

        // GET: Parts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var part = await _context.Part.SingleOrDefaultAsync(m => m.PartId == id);
            if (part == null)
            {
                return NotFound();
            }

            return View(part);
        }

        // GET: Parts/Create
        public IActionResult Create()
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            return View();
        }

        // POST: Parts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PartId,PartName")] Part part)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            if (ModelState.IsValid)
            {
                _context.Add(part);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(part);
        }

        // GET: Parts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            if (id == null)
            {
                return NotFound();
            }

            var part = await _context.Part.SingleOrDefaultAsync(m => m.PartId == id);
            if (part == null)
            {
                return NotFound();
            }
            return View(part);
        }

        // POST: Parts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PartId,PartName")] Part part)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            if (id != part.PartId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(part);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartExists(part.PartId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(part);
        }

        // GET: Parts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            if (id == null)
            {
                return NotFound();
            }

            var part = await _context.Part.SingleOrDefaultAsync(m => m.PartId == id);
            if (part == null)
            {
                return NotFound();
            }

            return View(part);
        }

        // POST: Parts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            var part = await _context.Part.SingleOrDefaultAsync(m => m.PartId == id);
            _context.Part.Remove(part);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool PartExists(int id)
        {
            return _context.Part.Any(e => e.PartId == id);
        }
    }
}
