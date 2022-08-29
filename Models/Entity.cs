using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfElmaBot.Models
{
    public class Entity
    {
        public string Id { get; set; }
        public string TypeUid { get; set; }
        public string Uid { get; set; }
        public string IdTelegram { get; set; }
        public string IdUserElma { get; set; }
        public string AuthToken { get; set; }
        public string SessionToken { get; set; }
        public string IdLastSms { get; set; }
        public string AuthorizationUser { get; set; }
        public string Login { get; set; }
    }
}
