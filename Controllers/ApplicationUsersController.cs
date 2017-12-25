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
using Microsoft.AspNetCore.Authorization;
using SCE.Models.ApplicationUsersViewModels;

namespace SCE.Controllers
{
    
    public class ApplicationUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public ApplicationUsersController(
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

        // GET: ApplicationUsers
        public async Task<IActionResult> Index()
        {
            if(!Auth())
            {
                return Unauthorized();
            }
            var Model = new IndexViewModel
            {
                Users = await _context.Users.Include(a => a.Part).Include(a => a.UserType).ToListAsync(),
                UserParts = _context.Part,
                UserTypes = _context.UserType,
            };
            return View(Model);
        }

        // GET: ApplicationUsers/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // GET: ApplicationUsers/Create
        public IActionResult Create()
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            ViewData["PartId"] = new SelectList(_context.Part, "PartId", "PartName");
            ViewData["UserTypeId"] = new SelectList(_context.UserType, "UserTypeId", "UserTypeName");
            return View();
        }

        // POST: ApplicationUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AccessFailedCount,ConcurrencyStamp,Description,Email,EmailConfirmed,LockoutEnabled,LockoutEnd,Name,NormalizedEmail,NormalizedUserName,PartId,PasswordHash,PhoneNumber,PhoneNumberConfirmed,SecurityStamp,StaffNo,TwoFactorEnabled,UserName,UserTypeId")] ApplicationUser applicationUser)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            if (ModelState.IsValid)
            { 
                applicationUser.UserName = applicationUser?.Name?.GetHashCode().ToString();
                var result = await _userManager.CreateAsync(applicationUser, applicationUser.PasswordHash);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                    AddErrors(result);
            }
            ViewData["PartId"] = new SelectList(_context.Part, "PartId", "PartName");
            ViewData["UserTypeId"] = new SelectList(_context.UserType, "UserTypeId", "UserTypeName");
            return View(applicationUser);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        // GET: ApplicationUsers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            var applicationUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }
            ViewData["PartId"] = new SelectList(_context.Part, "PartId", "PartName");
            ViewData["UserTypeId"] = new SelectList(_context.UserType, "UserTypeId", "UserTypeName");
            return View(applicationUser);
        }

        // POST: ApplicationUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,AccessFailedCount,ConcurrencyStamp,Description,Email,EmailConfirmed,LockoutEnabled,LockoutEnd,Name,NormalizedEmail,NormalizedUserName,PartId,PasswordHash,PhoneNumber,PhoneNumberConfirmed,SecurityStamp,StaffNo,TwoFactorEnabled,UserName,UserTypeId")] ApplicationUser applicationUser)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            if (id != applicationUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user =  await _userManager.FindByIdAsync(applicationUser.Id);
                    user.Description = applicationUser.Description;
                    user.Name = applicationUser.Name;
                    user.PartId = applicationUser.PartId;
                    user.UserTypeId = applicationUser.UserTypeId;
                    user.StaffNo = applicationUser.StaffNo;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationUserExists(applicationUser.Id))
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
            ViewData["PartId"] = new SelectList(_context.Part, "PartId", "PartName", applicationUser.PartId);
            ViewData["UserTypeId"] = new SelectList(_context.UserType, "UserTypeId", "UserTypeName", applicationUser.UserTypeId);
            return View(applicationUser);
        }

        // GET: ApplicationUsers/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            var applicationUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // POST: ApplicationUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!Auth())
            {
                return Unauthorized();
            }
            var applicationUser = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);
            var templist = _context.ProjectRecord.Where(t => t.WorkUserId == id || t.DoneUserId == id);
            _context.ProjectRecord.RemoveRange(templist);
            _context.Users.Remove(applicationUser);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ApplicationUserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
