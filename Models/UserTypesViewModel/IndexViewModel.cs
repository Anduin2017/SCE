using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCE.Models.UserTypesViewModel
{
    public class IndexViewModel
    {
        public IEnumerable<UserType> UserTypes { get; set; }
        public IEnumerable<ApplicationUser> Users { get; set; }
    }
}
