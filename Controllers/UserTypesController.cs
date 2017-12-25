using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SCE.Data;
using SCE.Models;
using SCE.Models.UserTypesViewModel;

namespace SCE.Controllers
{
    public class UserTypesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public UserTypesController(
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
        // GET: UserTypes
        public async Task<IActionResult> Index()
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            var _model = new IndexViewModel
            {
                Users = await _context.Users.Include(t => t.UserType).ToListAsync(),
                UserTypes = await _context.UserType.ToListAsync()
            };
            return View(_model);
        }

        // GET: UserTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            if (id == null)
            {
                return NotFound();
            }

            var userType = await _context.UserType.SingleOrDefaultAsync(m => m.UserTypeId == id);
            if (userType == null)
            {
                return NotFound();
            }

            return View(userType);
        }

        // GET: UserTypes/Create
        public IActionResult Create()
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            return View();
        }

        // POST: UserTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserType userType)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            if (ModelState.IsValid)
            {
                _context.Add(userType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(userType);
        }

        // GET: UserTypes/Edit/5
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

            var userType = await _context.UserType.SingleOrDefaultAsync(m => m.UserTypeId == id);
            if (userType == null)
            {
                return NotFound();
            }
            return View(userType);
        }

        // POST: UserTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,UserType userType)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            if (id != userType.UserTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var target = await _context.UserType.SingleOrDefaultAsync(t=>t.UserTypeId==userType.UserTypeId);
                    target.Arg = userType.Arg;
                    target.UserTypeName = userType.UserTypeName;
                    target.InCount = userType.InCount;
                    target.IsAdmin = userType.IsAdmin;
                    target.GivePointDirectly = userType.GivePointDirectly;
                    target.AvaliableToMark = userType.AvaliableToMark;
                    _context.Update(target);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserTypeExists(userType.UserTypeId))
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
            return View(userType);
        }

        // GET: UserTypes/Delete/5
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

            var userType = await _context.UserType.SingleOrDefaultAsync(m => m.UserTypeId == id);
            if (userType == null)
            {
                return NotFound();
            }

            return View(userType);
        }

        // POST: UserTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!Auth())
            {
                return Unauthorized();
            }

            var _userToRemove = _context.Users.Where(t=>t.UserTypeId==id);
            _context.Users.RemoveRange(_userToRemove);

            var _projectRecordsToRemove = _context
                .ProjectRecord
                .Include(t=>t.WorkUser)
                .Include(t=>t.DoneUser)
                .Where(t => t.DoneUser.UserTypeId == id || t.WorkUser.UserTypeId == id);
            _context.ProjectRecord.RemoveRange(_projectRecordsToRemove);

            var userType = await _context.UserType.SingleOrDefaultAsync(m => m.UserTypeId == id);
            _context.UserType.Remove(userType);

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool UserTypeExists(int id)
        {
            return _context.UserType.Any(e => e.UserTypeId == id);
        }
    }
}
