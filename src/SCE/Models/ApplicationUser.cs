using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SCE.Data;
using Microsoft.EntityFrameworkCore;
using SCE.Models.ProjectRecordsViewModels;

namespace SCE.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        public string StaffNo { get; set; }

        [DataType(DataType.Html)]
        public string Description { get; set; }


        public int PartId { get; set; }
        [ForeignKey(nameof(PartId))]
        public Part Part { get; set; }

        public virtual int UserTypeId { get; set; }
        public UserType UserType { get; set; }
        /// <summary>
        /// 本方法能够求出这个用户作为被测评人时，指定的测评人给了他多少分
        /// </summary>
        /// <param name="MyId"></param>
        /// <param name="pr"></param>
        /// <param name="p"></param>
        /// <returns></returns>

        public double ThePointsIGaveHim(string MyId, IEnumerable<ProjectRecord> pr, IEnumerable<Project> p)
        {
            var AllRecords = pr.Where(t => t.DoneUserId == this.Id && t.WorkUserId == MyId);
            double Sum = 0;
            var AllProjects = p.ToList();
            foreach (var Record in AllRecords)
            {
                var TargetProject = AllProjects.SingleOrDefault(t => t.ProjectId == Record.ProjectId);
                Sum += TargetProject.ProjectArg * (int)Record.Point;
            }
            if (AllRecords.Count() == 0)
            {
                return -1;
            }
            return Sum;
        }


        //求特定类型给该用户打分的均值
        public double MarkTypePoint(UserType Type, IEnumerable<ProjectRecord> AllRecords, int Projects, ref int AllCount)
        {
            double Sum = 0;
            var AllInThatType = AllRecords.Where(t => t.WorkUser.UserTypeId == Type.UserTypeId);
            foreach (var Record in AllInThatType)
            {
                //领导全题打分分数和
                Sum += Record.Project.ProjectArg * (int)Record.Point;
            }
            //给他打过分的领导人数
            int PointUserCount = AllInThatType.GroupBy(t => t.WorkUserId).Count();// Projects;
            if (Type.InCount)
            {
                AllCount += PointUserCount;
            }
            if (PointUserCount != 0)
            {
                //平均领导打分均值
                return Sum / PointUserCount;
            }
            else
            {
                return 0;
            }
        }

        //求我的得分
        public PointResult MyPoints(ApplicationDbContext DbContext, MarkDetailsViewModel Detail = null)
        {
            var AllRecords = DbContext.ProjectRecord
                .Include(t => t.Project)
                .Include(t => t.WorkUser)
                .Include(t => t.WorkUser.UserType)
                .Where(t => t.DoneUserId == this.Id)
                .ToList();

            var AllProjectsCount = DbContext.Project.Count();

            int UserCount = 0;
            double Sum = 0;
            double Pons = 0;

            foreach (var Type in DbContext.UserType)
            {
                //只有领导打分的平均分
                double TypeAverage = MarkTypePoint(Type, AllRecords, AllProjectsCount, ref UserCount);
                //如果确实有领导打过分
                if (TypeAverage != 0)
                {
                    //给领导加权计入总和
                    Sum += TypeAverage * Type.Arg;
                    //总权重增加次类型
                    Pons += Type.Arg;
                }

                if (Detail != null)
                {
                    Detail.TypeMark.Add(new TypeMark
                    {
                        Type = Type,
                        MarkValue = TypeAverage
                    });
                }
            }

            //最终得分为带权各类人打分分数和 / 权重和
            return new PointResult
            {
                Sum = Sum,
                FinalResult = Pons == 0 ? 0 : Sum / Pons,
                DoneUserCount = UserCount,
            };
        }
    }

    public class Part
    {
        public int PartId { get; set; }
        [Required]
        public string PartName { get; set; }
        [InverseProperty(nameof(ApplicationUser.Part))]

        public List<ApplicationUser> Users { get; set; }
    }

    public class UserType
    {
        public virtual int UserTypeId { get; set; }
        [Required]
        public virtual double Arg { get; set; }
        [Required]
        public virtual string UserTypeName { get; set; }

        public virtual bool IsAdmin { get; set; }
        public virtual bool AvaliableToMark { get; set; }
        //此项设为true后，用户打分时可以直接输入分数，而不是做题。
        public virtual bool GivePointDirectly { get; set; }
        //用户计入统计
        public virtual bool InCount { get; set; } = true;

        [InverseProperty(nameof(ApplicationUser.UserType))]

        public List<ApplicationUser> Users { get; set; }
    }

    public class ProjectRecord
    {
        public virtual int ProjectRecordId { get; set; }
        [Required]
        [Range(4, 10)]
        public virtual double Point { get; set; }

        [Required]
        public virtual string WorkUserId { get; set; }
        [ForeignKey(nameof(WorkUserId))]
        public ApplicationUser WorkUser { get; set; }

        [Required]
        public virtual string DoneUserId { get; set; }
        [ForeignKey(nameof(DoneUserId))]
        public ApplicationUser DoneUser { get; set; }

        [Required]
        public virtual int ProjectId { get; set; }
        [ForeignKey(nameof(ProjectId))]
        public virtual Project Project { get; set; }

        public virtual bool Keyboarded { get; set;}
    }

    public class Project
    {
        public virtual int ProjectId { get; set; }
        [Required]
        public virtual string ProjectName { get; set; }
        [Required]
        public virtual double ProjectArg { get; set; }
        public virtual string Comment { get; set; }
    }

    public class PointResult
    {
        //带权和
        public virtual double Sum { get; set; }
        //加权平均
        public virtual double FinalResult { get; set; }
        public virtual int DoneUserCount { get; set; }
        public virtual int TypeCount { get; set; }
    }
}
