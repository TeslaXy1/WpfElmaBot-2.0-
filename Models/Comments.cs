using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfElmaBot_2._0_.Models
{

    
    public class CreationAuthor
    {
        public string TypeUid { get; set; }
        public string Id { get; set; }
        public string Uid { get; set; }
        public string Name { get; set; }
    }

    public class Comments
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string ActionObjectUid { get; set; }
        public string ActionUid { get; set; }
        public int ActionObjectId { get; set; }
        public object Recipient { get; set; }
        public CreationAuthor CreationAuthor { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ChangeDate { get; set; }
        public int ChildCount { get; set; }
        public int Priority { get; set; }
        public bool IsRead { get; set; }
        public string RealHash { get; set; }
        public string ActionText { get; set; }
        public string Url { get; set; }
        public List<object> MessageAttachments { get; set; }
    }
}
