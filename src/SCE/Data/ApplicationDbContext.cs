using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SCE.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace SCE.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public async Task Seed(IApplicationBuilder app)
        {
            var _userManager = app.ApplicationServices.GetRequiredService<UserManager<ApplicationUser>>();

            var Admin = await this.UserType.SingleOrDefaultAsync(t => t.IsAdmin == true);
            if (Admin == null)
            {
                Admin = new UserType
                {
                    IsAdmin = true,
                    Arg = 1,
                    UserTypeName = "系统默认"
                };
                UserType.Add(Admin);
                await SaveChangesAsync();
            }

            var Part = await this.Part.SingleOrDefaultAsync(t => t.PartName == "其它");
            if(Part==null)
            {
                Part = new Part
                {
                    PartName = "其它"
                };
                this.Part.Add(Part);
                await this.SaveChangesAsync();
            }

            var wang = new ApplicationUser
            {
                Email = "example@example.com",
                UserName = "basic",
                StaffNo = "08608",
                Name = "王蓓蕾",
                UserTypeId = Admin.UserTypeId,
                PartId = Part.PartId
            };

            var currentWang = await this.Users.SingleOrDefaultAsync(t => t.Name == "王蓓蕾");
            if (currentWang == null)
            {
                await _userManager.CreateAsync(wang, "123456");
            }
            else
            {
                wang = currentWang;
            }

            await this.SaveChangesAsync();
        }

        public DbSet<Part> Part { get; set; }
        public DbSet<UserType> UserType { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<ProjectRecord> ProjectRecord { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
    }
}
