using Microsoft.EntityFrameworkCore;
using SCE.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCE.Models.ProjectRecordsViewModels
{
    public class MarkViewModel
    {
        public IEnumerable<ApplicationUser> Users { get; set; }
        public virtual string MeId { get; set; }

        public virtual IEnumerable<ProjectRecord> PR { get; set; }
        public virtual IEnumerable<Project> P { get; set; }

        public virtual string Role { get; set; }
        public virtual double Arg { get; set; }

        public virtual int Progress { get; set; }
        public virtual int Left { get; set; }
    }
}
