using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfElmaBot_2._0_.Models;

namespace WpfElmaBot.Models
{
    public class CreationAuthor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TypeUid { get; set; }
        public string Uid { get; set; }
    }

    public class Datum
    {
        public int ActionObjectId { get; set; }
        public string ActionObjectUid { get; set; }
        public string ActionUid { get; set; }
        public string AddCommentText { get; set; }
        public bool CanCreateComment { get; set; }
        public bool CanCreateQuestion { get; set; }
        public DateTime ChangeDate { get; set; }
        public int ChildCount { get; set; }
        public CreationAuthor CreationAuthor { get; set; }
        public DateTime CreationDate { get; set; }
        public bool HasUnreadChild { get; set; }
        public string Hash { get; set; }
        public int Id { get; set; }
        public bool IsRead { get; set; }
        public LastComments LastComments { get; set; }
        public List<object> MessageAttachments { get; set; }
        public List<object> MessageObjects { get; set; }
        public string ObjectGroupClass { get; set; }
        public string ObjectGroupText { get; set; }
        public List<ObjectRecipient> ObjectRecipients { get; set; }
        public int Priority { get; set; }
        public string RealHash { get; set; }
        public object Recipient { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
    }

    public class LastComments 
    {
        public object After { get; set; }
        public object Before { get; set; }
        public int Count { get; set; }
        public List<Comments> Data { get; set; }
    }

    public class ObjectRecipient
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TypeUid { get; set; }
    }

    public class MessegesOtvet
    {
        public string After { get; set; }
        public string Before { get; set; }
        public int Count { get; set; }
        public List<Datum> Data { get; set; }
        public int Priority { get; set; }
    }


    

}
