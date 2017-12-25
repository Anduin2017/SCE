using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCE.Models.ProjectRecordsViewModels
{
    public class MarkDetailsViewModel
    {
        public ApplicationUser Target { get; set; }
        public List<TypeMark> TypeMark { get; set; } = new List<TypeMark>();
        public List<ProjectMark> ProjectMark { get; set; } = new List<ProjectMark>();
        public PointResult Result { get; set; }
    }
    public class TypeMark
    {
        public UserType Type { get; set; }
        public double MarkValue { get; set; }
    }

    public class ProjectMark
    {
        public Project Project { get; set; }
        public double MarkValue { get; set; }
    }
}
