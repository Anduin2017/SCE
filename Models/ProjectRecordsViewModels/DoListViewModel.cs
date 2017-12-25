using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SCE.Models.ProjectRecordsViewModels
{
    public class DoListViewModel
    {
        public ApplicationUser Target { get; set; }
        public List<UserWithMark> Done { get; set; } = new List<UserWithMark>();
        public List<ApplicationUser> Pending { get; set; } = new List<ApplicationUser>();
    }
    public class UserWithMark
    {

        public ApplicationUser User { get; set; }
        public double MarkGave { get; set; }
    }
}
