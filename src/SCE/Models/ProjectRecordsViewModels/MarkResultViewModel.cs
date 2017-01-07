using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SCE.Data;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SCE.Models.ProjectRecordsViewModels
{
    public class MarkResultViewModel
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
        public ApplicationDbContext context{get;set;}
        public int ShouldCount { get; set; }
    }
}
