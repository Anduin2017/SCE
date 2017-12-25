using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCE.Models.ApplicationUsersViewModels
{
    public class IndexViewModel
    {
        public virtual IEnumerable<ApplicationUser> Users { get; set; }
        public virtual IEnumerable<UserType> UserTypes { get; set; }
        public virtual IEnumerable<Part> UserParts { get; set; }
    }
}
