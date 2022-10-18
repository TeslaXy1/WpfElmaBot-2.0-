using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfElmaBot.Models;

namespace WpfElmaBot_2._0_.Models.EntityPack
{

        public class Category
        {
            public string Id { get; set; }
            public string TypeUid { get; set; }
            public string Uid { get; set; }
            public string Name { get; set; }
        }

        public class Comment
        {
            public string Id { get; set; }
            public string TypeUid { get; set; }
            public string Uid { get; set; }
            public string Name { get; set; }
        }

        public class ControlUser
        {
            public string Id { get; set; }
            public string TypeUid { get; set; }
            public string Uid { get; set; }
            public string Name { get; set; }
        }

        public class CreationAuthor
        {
            public string Id { get; set; }
            public string TypeUid { get; set; }
            public string Uid { get; set; }
            public string Name { get; set; }
        }

        public class Executor
        {
            public string Id { get; set; }
            public string TypeUid { get; set; }
            public string Uid { get; set; }
            public string Name { get; set; }
        }

        public class PeriodTemplate
        {
            public string Id { get; set; }
            public string TypeUid { get; set; }
            public string Uid { get; set; }
            public string Name { get; set; }
        }

        public class Permission
        {
            public string Id { get; set; }
            public string TypeUid { get; set; }
            public string Uid { get; set; }
            public string Name { get; set; }
        }

        public class TaskBase: Entity
        {
            public string Id { get; set; }
            public string TypeUid { get; set; }
            public object ExecutorReplaced { get; set; }
            public string Uid { get; set; }
            public string Subject { get; set; }
            public string Description { get; set; }
            public string CreationDate { get; set; }
            public CreationAuthor CreationAuthor { get; set; }
            public Executor Executor { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string Priority { get; set; }
            public List<Comment> Comments { get; set; }
            public object Attachments { get; set; }
            public object Tags { get; set; }
            public object ParentTask { get; set; }
            public object ChildTasks { get; set; }
            public string Status { get; set; }
            public string StartWorkDate { get; set; }
            public string EndWorkDate { get; set; }
            public object InformTo { get; set; }
            public object Harmonizator { get; set; }
            public object TimeSet { get; set; }
            public List<Permission> Permissions { get; set; }
            public string NotShowInLists { get; set; }
            public string InformToHash { get; set; }
            public Category Category { get; set; }
            public string ExpiredNotificationSent { get; set; }
            public string PlanWorkLog { get; set; }
            public object FactWorkLog { get; set; }
            public string IsEmulation { get; set; }
            public object ExecutorIsEmulation { get; set; }
            public object Contractor { get; set; }
            public object Contact { get; set; }
            public object Lead { get; set; }
            public object Sale { get; set; }
            public object DocumentAttachments { get; set; }
            public object WorkflowBookmark { get; set; }
            public string AssignedToResponsible { get; set; }
            public object ControlUserReplaced { get; set; }
            public string UnderControl { get; set; }
            public ControlUser ControlUser { get; set; }
            public string ControlType { get; set; }
            public string ControlNotifyMe { get; set; }
            public string Period { get; set; }
            public string UntilDate { get; set; }
            public string InformAboutExpiration { get; set; }
            public string PauseDate { get; set; }
            public PeriodTemplate PeriodTemplate { get; set; }
            public string IsTemplate { get; set; }
            public string NextTasksCreationCount { get; set; }
            public string LastPeriodTaskDate { get; set; }
            public object CurrentControllers { get; set; }
        }

}
