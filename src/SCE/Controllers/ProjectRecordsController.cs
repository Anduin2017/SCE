using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SCE.Data;
using SCE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SCE.Services;
using Microsoft.Extensions.Logging;
using SCE.Models.ProjectRecordsViewModels;
using System.ComponentModel.DataAnnotations;

namespace SCE.Controllers
{
    [Authorize]
    public class ProjectRecordsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public ProjectRecordsController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _context = context;

        }

        public async Task<IActionResult> Mark()
        {
            var cuser = await GetCurrentUserAsync();
            int Done = 0, All = 1;
            var role = await _context.UserType.SingleOrDefaultAsync(t => t.UserTypeId == cuser.UserTypeId);
            try
            {
                All = _context
                    .Users
                    .Include(t => t.UserType)
                    .Where(t => t.UserType.AvaliableToMark == true && t.Id != cuser.Id)
                    .Count();
                Done = _context
                    .ProjectRecord
                    .Where(t => t.WorkUserId == cuser.Id)
                    .GroupBy(t => t.DoneUserId)
                    .Count();
            }
            catch
            {

            }
            if (All == 0)
            {
                Done = 1;
                All = 1;
            }
            var Model = new MarkViewModel
            {
                PR = _context.ProjectRecord.ToList(),
                P = _context.Project.ToList(),
                MeId = cuser.Id,
                Users = _context
                    .Users
                    .Include(t => t.UserType)
                    .Where(t => t.UserType.AvaliableToMark == true)
                    .ToList(),
                Role = role.UserTypeName,
                Arg = role.Arg,
                Progress = Convert.ToInt32(((double)Done / All) * 100),
                Left = All - Done
            };
            return View(Model);

        }

        public async Task<IActionResult> MarkNow(string id)
        {
            var user = await _context
                .Users
                .Include(t => t.UserType)
                .SingleOrDefaultAsync(t => t.Id == id);

            if (user.UserType.AvaliableToMark)
            {
                var questions = _context.Project;
                ViewBag.TargetUserId = id;
                ViewBag.UserName = user.Name;
                ViewBag.Content = user.Description;
                return View(questions);
            }
            return Unauthorized();
        }

        public async Task<IActionResult> MarkType(string id)
        {
            if (!_signInManager.IsSignedIn(User) || !_context.Users.Include(t => t.UserType).SingleOrDefault(t => t.UserName == User.Identity.Name).UserType.GivePointDirectly)
            {
                return Unauthorized();
            }
            var user = await _context
                .Users
                .Include(t => t.UserType)
                .SingleOrDefaultAsync(t => t.Id == id);

            if (user.UserType.AvaliableToMark)
            {
                var questions = _context.Project;
                ViewBag.TargetUserId = id;
                ViewBag.UserName = user.Name;
                ViewBag.Content = user.Description;
                return View(questions);
            }
            return Unauthorized();
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }


        [HttpPost]
        public async Task<IActionResult> MarkNow([FromForm]string TargetUserId, object obj)
        {
            //System Validate
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(MarkNow));
            }
            //Target Validate
            var _cuser = await GetCurrentUserAsync();
            var _targetuser = await _context.Users.Include(t => t.UserType).SingleOrDefaultAsync(t => t.Id == TargetUserId);
            if (_targetuser == null)
            {
                return NotFound();
            }
            if (!_targetuser.UserType.AvaliableToMark)
            {
                return Unauthorized();
            }

            //Start Logic
            var _toDelete = _context.ProjectRecord.Where(t => t.DoneUserId == _targetuser.Id && t.WorkUserId == _cuser.Id);
            _context.ProjectRecord.RemoveRange(_toDelete);
            await _context.SaveChangesAsync();

            var _tempList = new List<ProjectRecord>();
            int QuestId = 0;
            foreach (var Pair in HttpContext.Request.Form)
            {
                if (int.TryParse(Pair.Key, out QuestId))
                {
                    var TargetQuest = await _context.Project.SingleOrDefaultAsync(t => t.ProjectId == QuestId);
                    var _tempRecord = new ProjectRecord
                    {
                        WorkUserId = _cuser.Id,
                        DoneUserId = _targetuser.Id,
                        Point = Convert.ToDouble(Pair.Value.ToString()),
                        ProjectId = QuestId,
                    };
                    _tempList.Add(_tempRecord);
                }
            }

            _context.ProjectRecord.AddRange(_tempList);
            await _context.SaveChangesAsync();

            var questions = _context.Project;
            return RedirectToAction(nameof(Mark));
        }

        [HttpPost]
        public async Task<IActionResult> MarkType([FromForm]double MarkValue, [FromForm]string TargetUserId, object obj)
        {
            //System Validate
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(MarkType));
            }
            //User Validate
            var _cuser = _context.Users.Include(t => t.UserType).SingleOrDefault(t => t.UserName == User.Identity.Name);
            if (!_signInManager.IsSignedIn(User) || !_cuser.UserType.GivePointDirectly)
            {
                return Unauthorized();
            }
            //Target Validate
            var _targetuser = await _context.Users.Include(t => t.UserType).SingleOrDefaultAsync(t => t.Id == TargetUserId);
            if (_targetuser == null)
            {
                return NotFound();
            }
            if (!_targetuser.UserType.AvaliableToMark)
            {
                return Unauthorized();
            }
            //Start Logic
            var _toDelete = _context.ProjectRecord.Where(t => t.DoneUserId == _targetuser.Id && t.WorkUserId == _cuser.Id);
            _context.ProjectRecord.RemoveRange(_toDelete);
            await _context.SaveChangesAsync();


            var FirstQuest = await _context.Project.FirstAsync(t => t.ProjectArg > 0);
            var RealRecord = new ProjectRecord
            {
                WorkUserId = _cuser.Id,
                DoneUserId = _targetuser.Id,
                Point = MarkValue / FirstQuest.ProjectArg,
                ProjectId = FirstQuest.ProjectId,
                Keyboarded = true
            };
            _context.ProjectRecord.Add(RealRecord);
            await _context.SaveChangesAsync();

            //#warning Not Implemented!
            var questions = _context.Project;
            return RedirectToAction(nameof(Mark));
        }

        public async Task<IActionResult> MarkResult()
        {
            if (_context.Users.Include(t => t.UserType).SingleOrDefault(t => t.UserName == User.Identity.Name).UserType.IsAdmin)
            {
                var Model = new MarkResultViewModel
                {
                    context = _context,
                    ShouldCount = _context.Users.Include(t => t.UserType).Where(t => t.UserType.InCount).Count() - 1,
                    Users = await _context.Users.Include(t => t.UserType).Where(t => t.UserType.AvaliableToMark == true).ToListAsync()
                };
                return View(Model);
            }
            return Unauthorized();
        }


        // GET: ProjectRecords/Create
        public IActionResult Create()
        {
            ViewData["DoneUserId"] = new SelectList(_context.Users, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Project, "ProjectId", "ProjectName");
            ViewData["WorkUserId"] = new SelectList(_context.Users, "Id", "Name");
            return View();
        }

        // POST: ProjectRecords/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectRecordId,DoneUserId,Point,ProjectId,WorkUserId")] ProjectRecord projectRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(projectRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["DoneUserId"] = new SelectList(_context.Users, "Id", "Id", projectRecord.DoneUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "ProjectId", "ProjectId", projectRecord.ProjectId);
            ViewData["WorkUserId"] = new SelectList(_context.Users, "Id", "Id", projectRecord.WorkUserId);
            return View(projectRecord);
        }

        // GET: ProjectRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectRecord = await _context.ProjectRecord.SingleOrDefaultAsync(m => m.ProjectRecordId == id);
            if (projectRecord == null)
            {
                return NotFound();
            }
            ViewData["DoneUserId"] = new SelectList(_context.Users, "Id", "Id", projectRecord.DoneUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "ProjectId", "ProjectId", projectRecord.ProjectId);
            ViewData["WorkUserId"] = new SelectList(_context.Users, "Id", "Id", projectRecord.WorkUserId);
            return View(projectRecord);
        }

        // POST: ProjectRecords/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProjectRecordId,DoneUserId,Point,ProjectId,WorkUserId")] ProjectRecord projectRecord)
        {
            if (id != projectRecord.ProjectRecordId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projectRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectRecordExists(projectRecord.ProjectRecordId))
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
            ViewData["DoneUserId"] = new SelectList(_context.Users, "Id", "Id", projectRecord.DoneUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "ProjectId", "ProjectId", projectRecord.ProjectId);
            ViewData["WorkUserId"] = new SelectList(_context.Users, "Id", "Id", projectRecord.WorkUserId);
            return View(projectRecord);
        }

        // GET: ProjectRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectRecord = await _context.ProjectRecord.SingleOrDefaultAsync(m => m.ProjectRecordId == id);
            if (projectRecord == null)
            {
                return NotFound();
            }

            return View(projectRecord);
        }

        // POST: ProjectRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var projectRecord = await _context.ProjectRecord.SingleOrDefaultAsync(m => m.ProjectRecordId == id);
            _context.ProjectRecord.Remove(projectRecord);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ProjectRecordExists(int id)
        {
            return _context.ProjectRecord.Any(e => e.ProjectRecordId == id);
        }

        public async Task<IActionResult> ViewDes(string id)
        {
            var _target = await _context.Users.SingleOrDefaultAsync(t => t.Id == id);
            if (_target == null)
            {
                return Unauthorized();
            }

            return View(_target);
        }

        public async Task<IActionResult> DoList(string id)
        {
            var target = await _context.Users.SingleOrDefaultAsync(t => t.Id == id);
            var _model = new DoListViewModel
            {
                Target = target
            };
            double tempmark = 0;
            foreach (var user in _context
                .Users
                .Include(t => t.UserType)
                .Include(t => t.Part)
                .Where(t => t.UserType.InCount == true)
                .ToList())
            {
                tempmark = target.ThePointsIGaveHim(user.Id, _context.ProjectRecord, _context.Project);
                if (tempmark != -1)
                {
                    _model.Done.Add(new UserWithMark
                    {
                        User = user,
                        MarkGave = tempmark
                    });
                }
                else if (user.UserType.InCount && user.Id != target.Id)
                {
                    _model.Pending.Add(user);
                }
            }
            return View(_model);
        }

        public async Task<IActionResult> MarkDetails(string id)
        {
            var target = await _context.Users.SingleOrDefaultAsync(t => t.Id == id);
            var _model = new MarkDetailsViewModel
            {
                Target = target
            };
            var result = target.MyPoints(_context, _model);
            foreach (var proj in _context.Project.ToList())
            {
                var _tempList = _context
                    .ProjectRecord
                    .Where(t => t.ProjectId == proj.ProjectId && t.DoneUserId == target.Id && t.Keyboarded == false);
                double Sum = 0;
                if (_tempList.Count() != 0)
                {
                    Sum = _tempList.Average(t => t.Point);
                }
                _model.ProjectMark.Add(new ProjectMark
                {
                    MarkValue = Sum,
                    Project = proj
                });
            }

            _model.Result = result;
            return View(_model); ;
        }
    }
}
